using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Open_MediaServer.Backend.Schema;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Open_MediaServer.Database.Schema;
using Open_MediaServer.Utils;
using SQLite;

namespace Open_MediaServer.Backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MediaApiController : ControllerBase
{
    [HttpGet("/api/thumbnail/{id}/{name}")]
    public async Task<ActionResult> GetThumbnail(string id, string name)
    {
        var media = Program.Database.DatabaseConnection.Table<DatabaseSchema.Media>()
            .Where(media => media.Id == id && media.Name == name).FirstAsync().Result;
        var bytes = await System.IO.File.ReadAllBytesAsync(media.ThumbnailPath);
        var file = $"{media.Name}.{Program.ConfigManager.Config.ThumbnailType}";
        return File(bytes,
            new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType)
                ? contentType
                : "application/octet-stream", file);
    }


    [HttpGet("/api/media/{id}/{name}")]
    public async Task<ActionResult> GetMedia(string id, string name)
    {
        var media = Program.Database.DatabaseConnection.Table<DatabaseSchema.Media>()
            .Where(media => media.Id == id && media.Name == name).FirstAsync().Result;
        var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);
        var file = $"{media.Name}.{media.Extension}";
        return File(bytes,
            new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType)
                ? contentType
                : "application/octet-stream", file);
        // var file = @"D:\Programming\Projects\StrateimTech\a.mp4";
        // var bytes = await System.IO.File.ReadAllBytesAsync(file);
        // return File(bytes,
        //     new FileExtensionContentTypeProvider().TryGetContentType("a.mp4", out string contentType)
        //         ? contentType
        //         : "application/octet-stream", "a.mp4");
    }

    [HttpGet("/api/stats/")]
    public MediaSchema.MediaStats GetStats()
    {
        return null;
    }

    [HttpPost("/api/upload/")]
    public async Task<ActionResult> PostUploadContent(MediaSchema.MediaUpload mediaUpload)
    {
        byte[] content = mediaUpload.Content;
        if (Program.ConfigManager.Config.LosslessCompression)
        {
            content = LZ4Pickler.Pickle(mediaUpload.Content);
        }
        
        var mediaSchema = new DatabaseSchema.Media()
        {
            // TODO: Actually generate a unique ID
            Id = StringUtils.RandomString(Program.ConfigManager.Config.UniqueIdLength),
            Name = mediaUpload.Name,
            Extension = mediaUpload.Extension,
            Author = "Admin", // TODO: Replace this once user space is done...
            UploadDate = DateTime.Now,
            Size = content.Length,
            ContentCompressed = Program.ConfigManager.Config.LosslessCompression
        };
        
        // Program.Database.DatabaseConnection.InsertAsync();
        return null;
    }

    [HttpPost("/api/delete/{id}/{name}")]
    public async Task<ActionResult> PostDeleteContent()
    {
        
        
        
        // Program.Database.DatabaseConnection.DeleteAsync()
        
        return null;
    }
}