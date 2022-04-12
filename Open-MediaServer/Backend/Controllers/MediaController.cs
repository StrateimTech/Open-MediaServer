using Microsoft.AspNetCore.Mvc;
using Open_MediaServer.Backend.Schema;

namespace Open_MediaServer.Backend.Controllers;

[ApiController]
[Route("/api/v2/[controller]")]
public class MediaController
{
    [HttpGet("/api/v2/stats/")]
    public MediaSchema.MediaStats GetStats()
    {
        return null;
    }
    
    [HttpPost("/api/v2/upload/")]
    public ActionResult PostUploadContent()
    {
        return null;
    }
    
    [HttpPost("/api/v2/upload/")]
    public ActionResult PostDeleteContent()
    {
        return null;
    }
}