using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Open_MediaServer.Backend.Controllers;
using Open_MediaServer.Backend.Schema;

namespace Open_MediaServer.Frontend.Controllers;

[ApiController]
[Route("/api/frontend/[controller]")]
public class UserController : Controller
{
    [HttpPost("/api/frontend/account/login/")]
    public async Task<ActionResult> PostLogin([FromForm] UserSchema.User userLogin)
    {
        if (ModelState.IsValid)
        {
            Console.WriteLine("AA\nA");
            var controller = new UserApiController
            {
                ControllerContext = new ControllerContext(ControllerContext)
            };
            
            var result = await (controller).PostLogin(userLogin);
            Console.WriteLine($"{result}");
            return result;
        }
        
        ModelState.AddModelError("ErrorMessage", "An error occured internally!");
        return View("~/Frontend/Pages/Login.cshtml", userLogin);
    }
}