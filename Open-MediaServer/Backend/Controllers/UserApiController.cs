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
    public async Task<ActionResult> PostRegister([FromForm] UserSchema.User userRegister)
    {
        if (ModelState.IsValid)
        {
            if (!Program.ConfigManager.Config.AllowRegistering)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Registering is disabled.");
            }

            var usernameExists = Program.Database.UserDatabase.Table<DatabaseSchema.User>().ToListAsync().Result
                .Any(user => user.Username == userRegister.Username);

            if (usernameExists)
            {
                ModelState.AddModelError("ErrorMessage", "Username is already in use!");
                return View("~/Frontend/Pages/Register.cshtml", userRegister);
            }

            byte[] salt = new byte[128 / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(salt);

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: userRegister.Password,
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
                Username = userRegister.Username,
                Password = hashedPassword,
                Salt = salt,
                SessionKey = Convert.ToBase64String(sessionKey),
                CreationDate = DateTime.UtcNow,
                Uploads = new()
            };

            await Program.Database.UserDatabase.InsertWithChildrenAsync(userSchema);

            return RedirectToPage("/Account");
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpPost("/api/account/login/")]
    public async Task<ActionResult> PostLogin([FromForm] UserSchema.User userLogin)
    {
        if (ModelState.IsValid)
        {
            var user = await Program.Database.UserDatabase
                .FindAsync<DatabaseSchema.User>(user => user.Username == userLogin.Username);
            if (user == null)
            {
                ModelState.AddModelError("ErrorMessage", "Username or Password is incorrect!");
                return View("~/Frontend/Pages/Login.cshtml", userLogin);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: userLogin.Password,
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

                return RedirectToPage("/Account");
            }

            ModelState.AddModelError("ErrorMessage", "Username or Password is incorrect!");
            return View("~/Frontend/Pages/Login.cshtml", userLogin);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpPost("/api/account/delete/")]
    public async Task<ActionResult> PostDelete([FromForm] UserSchema.UserDelete userDelete)
    {
        if (ModelState.IsValid)
        {
            var userWithoutChildren = await Program.Database.UserDatabase.FindAsync<DatabaseSchema.User>(user =>
                user.Username == userDelete.User.Username);
            if (userWithoutChildren == null)
            {
                ModelState.AddModelError("ErrorMessage", "Username or password is incorrect!");
                return View("~/Frontend/Pages/AccountDelete.cshtml", userDelete);
            }

            var user =
                await Program.Database.UserDatabase.FindWithChildrenAsync<DatabaseSchema.User>(userWithoutChildren.Id);

            if (user == null)
            {
                ModelState.AddModelError("ErrorMessage", "Username or password is incorrect!");
                return View("~/Frontend/Pages/AccountDelete.cshtml", userDelete);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: userDelete.User.Password,
                salt: user.Salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            if (hashedPassword.SequenceEqual(user.Password))
            {
                if (userDelete.DeleteContent)
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
            return View("~/Frontend/Pages/AccountDelete.cshtml", userDelete);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpGet("/api/account/update/")]
    public async Task<ActionResult> GetUpdate([FromQuery] UserSchema.UserUpdate userUpdate)
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return RedirectToPage("/Login");
            }

            DatabaseSchema.User cookieUser = await UserUtils.GetUser(Request.Cookies["user_session"]);

            if (cookieUser == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var user = await Program.Database.UserDatabase.FindAsync<DatabaseSchema.User>(user =>
                user.Username == userUpdate.Username);
            if (user == null)
            {
                return RedirectToPage("/400");
            }

            if (cookieUser.Id == user.Id)
            {
                if (userUpdate.Name != null)
                {
                    user.Username = userUpdate.Name;
                }

                await Program.Database.UserDatabase.UpdateAsync(user);
                return Redirect("/Account");
            }

            return RedirectToPage("/401");
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpGet("/api/account/logout/")]
    public ActionResult GetLogout()
    {
        if (Request.Cookies["user_session"] != null)
        {
            Response.Cookies.Delete("user_session");
        }

        return RedirectToPage("/Login");
    }
}