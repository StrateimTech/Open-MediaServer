using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Open_MediaServer.Backend.Schema;
using Open_MediaServer.Database.Schema;
using Open_MediaServer.Utils;
using SQLiteNetExtensionsAsync.Extensions;

namespace Open_MediaServer.Backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserApiController : Controller
{
    [HttpPost("/api/account/register/")]
    public async Task<ActionResult> PostRegister([FromForm] UserSchema.User register)
    {
        if (ModelState.IsValid)
        {
            if (!Program.ConfigManager.Config.AllowRegistering)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Registering is disabled.");
            }

            var usernameExists = Program.Database.UserDatabase.Table<DatabaseSchema.User>().ToListAsync().Result
                .Any(user => user.Username == register.Username);

            if (usernameExists)
            {
                ModelState.AddModelError("ErrorMessage", "Username is already in use!");
                return View("~/Frontend/Pages/Register.cshtml", register);
            }

            byte[] salt = new byte[128 / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(salt);

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: register.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            byte[] sessionKey = new byte[64];
            rng.GetNonZeroBytes(sessionKey);
            Response.Cookies.Append("user_session", Convert.ToBase64String(sessionKey), new CookieOptions()
            {
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(30),
            });

            var userSchema = new DatabaseSchema.User()
            {
                Username = register.Username,
                Password = hashedPassword,
                Salt = salt,
                SessionKey = Convert.ToBase64String(sessionKey),
                CreationDate = DateTime.UtcNow,
                Uploads = new()
            };

            await Program.Database.UserDatabase.InsertWithChildrenAsync(userSchema);

            return RedirectToPage("/account");
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpPost("/api/account/login/")]
    public async Task<ActionResult> PostLogin([FromForm] UserSchema.User login)
    {
        if (ModelState.IsValid)
        {
            var user = await Program.Database.UserDatabase
                .FindAsync<DatabaseSchema.User>(user => user.Username == login.Username);
            if (user == null)
            {
                ModelState.AddModelError("ErrorMessage", "Username or Password is incorrect!");
                return View("~/Frontend/Pages/Login.cshtml", login);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: login.Password,
                salt: user.Salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            if (hashedPassword.SequenceEqual(user.Password))
            {
                if (user.SessionKey == null)
                {
                    byte[] sessionKey = new byte[64];
                    using var rng = RandomNumberGenerator.Create();
                    rng.GetNonZeroBytes(sessionKey);
                    Response.Cookies.Append("user_session", Convert.ToBase64String(sessionKey), new CookieOptions()
                    {
                        IsEssential = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.Now.AddDays(30)
                    });
                    user.SessionKey = Convert.ToBase64String(sessionKey);
                    await Program.Database.UserDatabase.UpdateWithChildrenAsync(user);
                }

                if (Request.Cookies["user_session"] != null &&
                    !Request.Cookies["user_session"].SequenceEqual(user.SessionKey))
                {
                    Response.Cookies.Delete("user_session");
                }

                if (Request.Cookies["user_session"] == null)
                {
                    Response.Cookies.Append("user_session", user.SessionKey, new CookieOptions()
                    {
                        IsEssential = true,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.Now.AddDays(30)
                    });
                }

                return RedirectToPage("/account");
            }

            ModelState.AddModelError("ErrorMessage", "Username or Password is incorrect!");
            return View("~/Frontend/Pages/Login.cshtml", login);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpPost("/api/account/delete/")]
    public async Task<ActionResult> PostDelete([FromForm] UserSchema.UserDelete delete)
    {
        if (ModelState.IsValid)
        {
            var userWithoutChildren = await Program.Database.UserDatabase.FindAsync<DatabaseSchema.User>(user =>
                user.Username == delete.User.Username);
            if (userWithoutChildren == null)
            {
                ModelState.AddModelError("ErrorMessage", "Username or password is incorrect!");
                return View("~/Frontend/Pages/AccountDelete.cshtml", delete);
            }

            var user =
                await Program.Database.UserDatabase.FindWithChildrenAsync<DatabaseSchema.User>(userWithoutChildren.Id);

            if (user == null)
            {
                ModelState.AddModelError("ErrorMessage", "Username or password is incorrect!");
                return View("~/Frontend/Pages/AccountDelete.cshtml", delete);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: delete.User.Password,
                salt: user.Salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            if (hashedPassword.SequenceEqual(user.Password))
            {
                if (delete.DeleteContent)
                {
                    foreach (var mediaIdentity in user.Uploads)
                    {
                        await Program.Database.MediaDatabase.DeleteAsync<DatabaseSchema.Media>(mediaIdentity.Id);
                    }
                }

                await Program.Database.UserDatabase.DeleteAsync<DatabaseSchema.User>(user.Id);

                if (Request.Cookies["user_session"] != null)
                {
                    Response.Cookies.Delete("user_session");
                }

                return RedirectToPage("/");
            }

            ModelState.AddModelError("ErrorMessage", "Username or password is incorrect!");
            return View("~/Frontend/Pages/AccountDelete.cshtml", delete);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpGet("/api/account/update/")]
    public async Task<ActionResult> GetUpdate([FromQuery] UserSchema.UserUpdate update)
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return RedirectToPage("/login");
            }

            DatabaseSchema.User cookieUser = await UserUtils.GetUser(Request.Cookies["user_session"]);

            if (cookieUser == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var user = await Program.Database.UserDatabase.FindAsync<DatabaseSchema.User>(user =>
                user.Username == update.Username);
            if (user == null)
            {
                return Redirect("/400");
            }

            if (cookieUser.Id == user.Id)
            {
                if (update.Name != null)
                {
                    user.Username = update.Name;
                }

                await Program.Database.UserDatabase.UpdateAsync(user);
                return Redirect("/account");
            }

            return Redirect("/401");
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpGet("/api/account/passwordchange/")]
    public async Task<ActionResult> GetPasswordChange([FromQuery] UserSchema.UserPasswordChange passwordChange)
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return RedirectToPage("/login");
            }

            DatabaseSchema.User cookieUser = await UserUtils.GetUser(Request.Cookies["user_session"]);

            if (cookieUser == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var user = await Program.Database.UserDatabase.FindAsync<DatabaseSchema.User>(user =>
                user.Username == passwordChange.Username);
            if (user == null)
            {
                return Redirect("/400");
            }

            if (cookieUser.Id == user.Id)
            {
                string previousHashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: passwordChange.CurrentPassword,
                    salt: user.Salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                Console.WriteLine(previousHashedPassword);
                Console.WriteLine(user.Password);
                if (previousHashedPassword.SequenceEqual(user.Password))
                {
                    if (passwordChange.Password.SequenceEqual(passwordChange.PasswordConfirm))
                    {
                        byte[] salt = new byte[128 / 8];
                        using var rng = RandomNumberGenerator.Create();
                        rng.GetNonZeroBytes(salt);

                        string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: passwordChange.Password,
                            salt: salt,
                            prf: KeyDerivationPrf.HMACSHA256,
                            iterationCount: 100000,
                            numBytesRequested: 256 / 8));

                        byte[] sessionKey = new byte[64];
                        rng.GetNonZeroBytes(sessionKey);

                        user.Salt = salt;
                        user.Password = hashedPassword;
                        user.SessionKey = Convert.ToBase64String(sessionKey);

                        if (Request.Cookies["user_session"] != null)
                        {
                            Response.Cookies.Delete("user_session");
                        }

                        await Program.Database.UserDatabase.UpdateWithChildrenAsync(user);
                        return RedirectPermanent("/login");
                    }

                    return Redirect("/account");
                }

                return Redirect("/account");
            }

            return Redirect("/401");
        }

        return Redirect("/account");
    }

    [HttpGet("/api/account/logout/")]
    public ActionResult GetLogout()
    {
        if (Request.Cookies["user_session"] != null)
        {
            Response.Cookies.Delete("user_session");
        }

        return RedirectToPage("/login");
    }
}