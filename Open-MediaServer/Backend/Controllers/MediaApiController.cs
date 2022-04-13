using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi.Extensions;
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
        if (!Program.ConfigManager.Config.Thumbnails)
            return StatusCode(StatusCodes.Status501NotImplemented);

        var fileName = Path.GetFileNameWithoutExtension(name);
        var media = Program.Database.DatabaseConnection.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == id && media.Name == fileName).Result;
        byte[] thumbnail;
        if (media.ThumbnailPath == null)
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);
            thumbnail = ContentUtils.GetThumbnail(bytes, Program.ConfigManager.Config.ThumbnailSize?.Item1,
                Program.ConfigManager.Config.ThumbnailSize?.Item2, media.ContentType);
            if (thumbnail == null)
                return StatusCode(StatusCodes.Status500InternalServerError);
            media.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnail, media.Id, media.Name, media.Extension,
                (ContentType) ContentUtils.GetContentType(media.Extension)!);
            await Program.Database.DatabaseConnection.UpdateAsync(media);
        }
        else
        {
            thumbnail = await System.IO.File.ReadAllBytesAsync(media.ThumbnailPath);
        }

        var file = $"{media.Name}{Program.ConfigManager.Config.ThumbnailType}";
        return File(thumbnail,
            new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType)
                ? contentType
                : "application/octet-stream", file);
    }


    [HttpGet("/api/media/{id}/{name}")]
    public async Task<ActionResult> GetMedia(string id, string name)
    {
        var fileName = Path.GetFileNameWithoutExtension(name);
        var media = Program.Database.DatabaseConnection.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == id && media.Name == fileName).Result;

        var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);

        if (media.ContentCompressed)
        {
            bytes = LZ4Pickler.Unpickle(bytes);
        }

        var file = $"{media.Name}{media.Extension}";
        return File(bytes,
            new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType)
                ? contentType
                : "application/octet-stream", file);
    }

    [HttpGet("/api/stats/")]
    public MediaSchema.MediaStats GetStats()
    {
        var mediaTableQuery = Program.Database.DatabaseConnection.Table<DatabaseSchema.Media>();
        var totalContent = mediaTableQuery.CountAsync();
        var totalContentSize = mediaTableQuery.ToListAsync().Result.Sum(media => media.Size);

        var statSchema = new MediaSchema.MediaStats()
        {
            ContentCount = totalContent.Result,
            ContentTotalSize = totalContentSize
        };
        return statSchema;
    }

    [HttpPost("/api/upload/")]
    public async Task<ActionResult> PostUploadContent(MediaSchema.MediaUpload mediaUpload)
    {
        Console.WriteLine($"Handling media upload");
        var contentType = ContentUtils.GetContentType(mediaUpload.Extension);
        Console.WriteLine($"ContentType: {contentType?.GetDisplayName()}");
        if (contentType == null)
            return StatusCode(StatusCodes.Status400BadRequest);

        if (contentType == ContentType.Image && !Program.ConfigManager.Config.AllowImages
            || contentType == ContentType.Video && !Program.ConfigManager.Config.AllowVideos
            || contentType == ContentType.Other && !Program.ConfigManager.Config.AllowOther)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        bool contentCompressed = Program.ConfigManager.Config.LosslessCompression;
        byte[] content = new byte[LZ4Codec.MaximumOutputSize(mediaUpload.Content.Length)];
        if (Program.ConfigManager.Config.LosslessCompression)
        {
            var dataWritten = LZ4Codec.Encode(mediaUpload.Content, content,
                Program.ConfigManager.Config.LosslessCompressionLevel);
            if (dataWritten != -1 && mediaUpload.Content.Length > dataWritten)
            {
                Array.Resize(ref content, dataWritten);
            }
            else
            {
                content = mediaUpload.Content;
                contentCompressed = false;
            }
        }
        else
        {
            content = mediaUpload.Content;
            contentCompressed = false;
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
            ContentCompressed = contentCompressed,
            ContentType = (ContentType) contentType
        };
        mediaSchema.ContentPath = Program.ContentManager.SaveContent(content, mediaSchema.Id, mediaSchema.Name,
            mediaSchema.Extension, (ContentType) contentType);

        if ((contentType == ContentType.Video || contentType == ContentType.Image) &&
            Program.ConfigManager.Config.Thumbnails && Program.ConfigManager.Config.PreComputeThumbnails)
        {
            var thumbnail = ContentUtils.GetThumbnail(content, Program.ConfigManager.Config.ThumbnailSize?.Item1,
                Program.ConfigManager.Config.ThumbnailSize?.Item2, (ContentType)contentType);
            if (thumbnail != null)
            {
                mediaSchema.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnail, mediaSchema.Id,
                    mediaSchema.Name, mediaSchema.Extension, (ContentType) contentType);
            }
            else
            {
                Console.WriteLine(
                    $"Failed to pre compute thumbnail (ID: {mediaSchema.Id}, Name: {mediaSchema.Name}, Extension: {mediaSchema.Extension})");
            }
        }

        Console.WriteLine("Inserting media into sqlite db");
        await Program.Database.DatabaseConnection.InsertAsync(mediaSchema);
        string serializedJson = JsonSerializer.Serialize(mediaSchema, new JsonSerializerOptions()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });
        Console.WriteLine(serializedJson);
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpPost("/api/delete/{id}/{name}")]
    public async Task<ActionResult> PostDeleteContent(string id, string name)
    {
        var fileName = Path.GetFileNameWithoutExtension(name);
        var media = Program.Database.DatabaseConnection.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == id && media.Name == fileName).Result;
        await Program.Database.DatabaseConnection.DeleteAsync<DatabaseSchema.Media>(media);
        return StatusCode(StatusCodes.Status200OK);
    }
}