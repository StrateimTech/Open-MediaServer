using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Open_MediaServer.Backend.Schema;
using Open_MediaServer.Database.Schema;

namespace Open_MediaServer.Frontend.Controllers;

[ApiController]
[Route("[controller]")]
public class ContentController : ControllerBase
{
    [HttpGet("/{id}/{name}")]
    public async Task<ActionResult> GetContent(string id, string name)
    {
        var fileName = Path.GetFileNameWithoutExtension(name);
        var media = Program.Database.MediaDatabase.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == id && media.Name == fileName).Result;
        
        if (media == null)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }
        
        var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);
        if (media.ContentCompressed)
        {
            bytes = LZ4Pickler.Unpickle(bytes);
        }
        
        var file = $"{media.Name}{media.Extension}";
        var fileContentType = new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType)
            ? contentType
            : "application/octet-stream";
        return File(bytes, fileContentType, file);
    }
}