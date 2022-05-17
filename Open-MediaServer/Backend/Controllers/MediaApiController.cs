using System;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    [HttpGet("/api/thumbnail/")]
    public async Task<ActionResult> GetThumbnail(MediaSchema.MediaIdentity identity)
    {
        if (!Program.ConfigManager.Config.Thumbnails)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var fileName = Path.GetFileNameWithoutExtension(identity.Name);
        var media = Program.Database.MediaDatabase.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == identity.Id && media.Name == fileName).Result;
        
        if (media.ContentType == ContentType.Other)
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }
        
        byte[] thumbnail;
        if (media.ThumbnailPath == null)
        {
            var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);

            if (media.ContentCompressed)
            {
                bytes = LZ4Pickler.Unpickle(bytes);
            }

            thumbnail = ContentUtils.GetThumbnail(bytes, Program.ConfigManager.Config.ThumbnailSize?.Item1,
                Program.ConfigManager.Config.ThumbnailSize?.Item2, media.ContentType);

            if (thumbnail == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            media.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnail, media.Id, media.Name,
                Program.ConfigManager.Config.ThumbnailType,
                (ContentType) ContentUtils.GetContentType(media.Extension)!);
            await Program.Database.MediaDatabase.UpdateAsync(media);
        }
        else
        {
            thumbnail = await System.IO.File.ReadAllBytesAsync(media.ThumbnailPath);
        }

        var file = $"{media.Name}.png";
        return File(thumbnail, "image/png", file);
    }


    [HttpGet("/api/media/")]
    public async Task<ActionResult> GetMedia(MediaSchema.MediaIdentity identity)
    {
        var fileName = Path.GetFileNameWithoutExtension(identity.Name);
        var media = Program.Database.MediaDatabase.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == identity.Id && media.Name == fileName).Result;

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

    [HttpGet("/api/stats/")]
    public async Task<MediaSchema.MediaStats> GetStats()
    {
        var mediaTableQuery = Program.Database.MediaDatabase.Table<DatabaseSchema.Media>();
        var totalContent = await mediaTableQuery.CountAsync();
        var totalContentSize = mediaTableQuery.ToListAsync().Result.Sum(media => media.Size);

        var statSchema = new MediaSchema.MediaStats()
        {
            ContentCount = totalContent,
            ContentTotalSize = totalContentSize,
            VideoContent = Program.ConfigManager.Config.AllowVideos,
            ImageContent = Program.ConfigManager.Config.AllowImages,
            OtherContent = Program.ConfigManager.Config.AllowOther
        };
        return statSchema;
    }

    [HttpPost("/api/upload/")]
    public async Task<ActionResult> PostUploadContent(MediaSchema.MediaUpload mediaUpload)
    {
        var contentType = ContentUtils.GetContentType(mediaUpload.Extension);
        if (contentType == null)
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        if (contentType == ContentType.Image && !Program.ConfigManager.Config.AllowImages
            || contentType == ContentType.Video && !Program.ConfigManager.Config.AllowVideos
            || contentType == ContentType.Other && !Program.ConfigManager.Config.AllowOther)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        bool contentCompressed = Program.ConfigManager.Config.LosslessCompression;
        byte[] content;

        if (Program.ConfigManager.Config.LosslessCompression
            && LZ4Codec.MaximumOutputSize(mediaUpload.Content.Length) < mediaUpload.Content.Length)
        {
            content = LZ4Pickler.Pickle(mediaUpload.Content, Program.ConfigManager.Config.LosslessCompressionLevel);
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
            var thumbnail = ContentUtils.GetThumbnail(mediaUpload.Content,
                Program.ConfigManager.Config.ThumbnailSize?.Item1,
                Program.ConfigManager.Config.ThumbnailSize?.Item2, (ContentType) contentType);
            if (thumbnail != null)
            {
                mediaSchema.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnail, mediaSchema.Id,
                    mediaSchema.Name, Program.ConfigManager.Config.ThumbnailType, (ContentType) contentType);
            }
            else
            {
                Console.WriteLine(
                    $"Failed to pre compute thumbnail (ID: {mediaSchema.Id}, Name: {mediaSchema.Name}, Extension: {mediaSchema.Extension})");
            }
        }

        Console.WriteLine("Inserting media into sqlite db");
        await Program.Database.MediaDatabase.InsertAsync(mediaSchema);
        string serializedJson = JsonSerializer.Serialize(mediaSchema, new JsonSerializerOptions()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });
        Console.WriteLine(serializedJson);
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpPost("/api/delete/")]
    public async Task<ActionResult> PostDeleteContent(MediaSchema.MediaIdentity identity)
    {
        var fileName = Path.GetFileNameWithoutExtension(identity.Name);
        var media = Program.Database.MediaDatabase.GetAsync<DatabaseSchema.Media>(media =>
            media.Id == identity.Id && media.Name == fileName).Result;
        await Program.Database.MediaDatabase.DeleteAsync<DatabaseSchema.Media>(media);
        return StatusCode(StatusCodes.Status200OK);
    }
}