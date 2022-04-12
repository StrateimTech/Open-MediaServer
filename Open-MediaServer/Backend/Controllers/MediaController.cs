using Microsoft.AspNetCore.Mvc;

namespace Open_MediaServer.Backend.Controllers;

[ApiController]
[Route("/api/v2/[controller]")]
public class MediaController
{
    [HttpGet("/api/v2/test/")]
    public int Test()
    {
        return 42;
    }
}