using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Open_MediaServer.Backend.Schema;
using Open_MediaServer.Content;
using Open_MediaServer.Database.Schema;
using Open_MediaServer.Utils;

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
        var contentType = ContentUtils.GetContentType(mediaUpload.Extension);
        if (contentType == null)
            return StatusCode(StatusCodes.Status400BadRequest);

        if (contentType == ContentType.Image && !Program.ConfigManager.Config.AllowImages
            || contentType == ContentType.Video && !Program.ConfigManager.Config.AllowVideos
            || contentType == ContentType.Other && !Program.ConfigManager.Config.AllowOther)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

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
        mediaSchema.ContentPath = Program.ContentManager.SaveContent(content, mediaSchema.Id, mediaSchema.Name,
            mediaSchema.Extension, (ContentType) contentType);

        if ((contentType == ContentType.Video || contentType == ContentType.Image) &&
            Program.ConfigManager.Config.PreComputeThumbnail)
        {
            var thumbnail = ContentUtils.GetThumbnail();
            if (thumbnail != null)
            {
                if (Program.ConfigManager.Config.LosslessCompression)
                {
                    thumbnail = LZ4Pickler.Pickle(thumbnail);
                }

                mediaSchema.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnail, mediaSchema.Id,
                    mediaSchema.Name,
                    mediaSchema.Extension, (ContentType) contentType);
            }
            else
            {
                Console.WriteLine($"Failed to compute thumbnail (ID: {mediaSchema.Id}, Name: {mediaSchema.Name}, Extension: {mediaSchema.Extension})");
            }
        }

        await Program.Database.DatabaseConnection.InsertAsync(mediaSchema);
        return null;
    }

    [HttpPost("/api/delete/{id}/{name}")]
    public async Task<ActionResult> PostDeleteContent()
    {
        // Program.Database.DatabaseConnection.DeleteAsync()
        return null;
    }
}