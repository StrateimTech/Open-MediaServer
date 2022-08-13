using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Open_MediaServer.Backend.Schema;
using Open_MediaServer.Content;
using Open_MediaServer.Database.Schema;
using Open_MediaServer.Utils;
using SQLiteNetExtensionsAsync.Extensions;

namespace Open_MediaServer.Backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class MediaApiController : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider FileExtensionContentTypeProvider = new();

    [HttpGet("/api/thumbnail/")]
    public async Task<ActionResult> GetThumbnail([FromQuery] MediaSchema.MediaIdentity identity)
    {
        if (ModelState.IsValid)
        {
            if (!Program.ConfigManager.Config.Thumbnails)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var media = await Program.Database.MediaDatabase.FindAsync<DatabaseSchema.Media>(media =>
                media.Id == identity.Id && media.Name == identity.Name);

            if (media == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (media.ContentType == ContentType.Other)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            if (Request.GetTypedHeaders().IfModifiedSince?.UtcDateTime >= media.UploadDate)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            byte[] thumbnailBytes;
            if (media.ThumbnailPath == null)
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);

                if (media.ContentCompressed)
                {
                    bytes = LZ4Pickler.Unpickle(bytes);
                }

                thumbnailBytes = await ContentUtils.GetThumbnail(bytes,
                    Program.ConfigManager.Config.ThumbnailWidth, media.ContentType,
                    Program.ConfigManager.Config.ThumbnailFormat);

                if (thumbnailBytes == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                media.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnailBytes, media.Id, media.Name,
                    Program.ConfigManager.Config.ThumbnailFormat.FileExtensions.ToList()[0],
                    (ContentType) ContentUtils.GetContentType(media.Extension)!);
                await Program.Database.MediaDatabase.UpdateAsync(media);
            }
            else
            {
                if (System.IO.File.Exists(media.ThumbnailPath))
                {
                    thumbnailBytes = await System.IO.File.ReadAllBytesAsync(media.ThumbnailPath);
                }
                else
                {
                    var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);

                    if (media.ContentCompressed)
                    {
                        bytes = LZ4Pickler.Unpickle(bytes);
                    }

                    thumbnailBytes = await ContentUtils.GetThumbnail(bytes,
                        Program.ConfigManager.Config.ThumbnailWidth, media.ContentType,
                        Program.ConfigManager.Config.ThumbnailFormat);

                    if (thumbnailBytes == null)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }

                    media.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnailBytes, media.Id, media.Name,
                        Program.ConfigManager.Config.ThumbnailFormat.FileExtensions.ToList()[0],
                        (ContentType) ContentUtils.GetContentType(media.Extension)!);
                    await Program.Database.MediaDatabase.UpdateAsync(media);
                }
            }

            if (Program.ConfigManager.Config.Caching)
            {
                var responseHeaders = Response.GetTypedHeaders();
                responseHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };
                responseHeaders.LastModified = new DateTimeOffset(media.UploadDate);
            }

            var file = $"{media.Name}.{Program.ConfigManager.Config.ThumbnailFormat.FileExtensions.ToList()[0]}";
            return File(thumbnailBytes, Program.ConfigManager.Config.ThumbnailFormat.DefaultMimeType, file);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }


    [HttpGet("/api/media/")]
    public async Task<ActionResult> GetMedia([FromQuery] MediaSchema.MediaIdentity identity)
    {
        if (ModelState.IsValid)
        {
            var fileName =
                Uri.EscapeDataString(Uri.UnescapeDataString(Path.GetFileNameWithoutExtension(identity.Name)!));
            var media = await Program.Database.MediaDatabase.FindAsync<DatabaseSchema.Media>(media =>
                media.Id == identity.Id && media.Name == fileName);

            if (media == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (Request.GetTypedHeaders().IfModifiedSince?.UtcDateTime >= media.UploadDate)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(media.ContentPath);
            if (media.ContentCompressed)
            {
                bytes = LZ4Pickler.Unpickle(bytes);
            }

            if (Program.ConfigManager.Config.Caching)
            {
                var responseHeaders = Response.GetTypedHeaders();
                responseHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };
                responseHeaders.LastModified = new DateTimeOffset(media.UploadDate);
            }

            var file = $"{media.Name}{media.Extension}";
            var fileContentType = FileExtensionContentTypeProvider.TryGetContentType(file, out string contentType)
                ? contentType
                : "application/octet-stream";
            return File(bytes, fileContentType, file);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpPost("/api/mass/")]
    public async Task<MediaSchema.MediaReturnMass> GetMass(MediaSchema.MediaParameterMass parameterMass)
    {
        if (ModelState.IsValid)
        {
            var mediaTable = await Program.Database.MediaDatabase.GetAllWithChildrenAsync<DatabaseSchema.Media>();
            mediaTable.RemoveAll(media => !media.Public);

            if (parameterMass.Username != null)
            {
                var userWithoutChildren =
                    await Program.Database.UserDatabase.FindAsync<DatabaseSchema.User>(user =>
                        user.Username == parameterMass.Username);

                if (userWithoutChildren == null)
                {
                    Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return null;
                }

                var user =
                    await Program.Database.UserDatabase.FindWithChildrenAsync<DatabaseSchema.User>(
                        userWithoutChildren.Id);
                mediaTable.RemoveAll(media => media.AuthorId != user.Id);
            }

            if (parameterMass.Type != null)
            {
                mediaTable.RemoveAll(media => media.ContentType == parameterMass.Type);
            }

            var mediaIdentities = mediaTable.Select(media => new MediaSchema.MediaIdentity()
            {
                Name = media.Name,
                Id = media.Id
            }).ToList();

            return new MediaSchema.MediaReturnMass()
            {
                Media = mediaIdentities
            };
        }

        Response.StatusCode = StatusCodes.Status400BadRequest;
        return null;
    }

    [HttpGet("/api/stats/")]
    public async Task<MediaSchema.MediaStats> GetStats()
    {
        var mediaTableQuery = Program.Database.MediaDatabase.Table<DatabaseSchema.Media>();
        var totalContent = await mediaTableQuery.CountAsync();
        var totalContentSize = (await mediaTableQuery.ToListAsync()).Sum(media => media.ContentSize);

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

    [HttpPost("/api/upload/file/")]
    public async Task<ActionResult> PostUploadContentForm()
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return Redirect("/401");
            }

            var formFiles = HttpContext.Request.Form.Files.DistinctBy(file => file.FileName).ToList();
            if (formFiles.Count <= 0)
            {
                return Redirect("/400");
            }

            for (int i = 0; i < formFiles.Count; i++)
            {
                var id = i;
                var file = formFiles[i];
                bool visible = true;
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);

                if (HttpContext.Request.Form.Count(form => form.Key.Contains("Name")) > formFiles.Count)
                {
                    id++;
                }

                if (HttpContext.Request.Form.ContainsKey($"Name {id}"))
                {
                    var name = HttpContext.Request.Form[$"Name {id}"];
                    fileName = Path.GetFileNameWithoutExtension(name);
                }

                if (HttpContext.Request.Form.ContainsKey($"Private {id}") &&
                    HttpContext.Request.Form[$"Private {id}"] == "on")
                {
                    visible = false;
                }

                byte[] data = new byte[file.Length];
                if (file.Length > 0)
                {
                    using var memStream = new MemoryStream(data);
                    await file.CopyToAsync(memStream);
                }
                else
                {
                    continue;
                }

                if (fileName.Length > Program.ConfigManager.Config.UploadNameLimit)
                {
                    if (Path.HasExtension(fileName))
                    {
                        fileName = Path.GetFileNameWithoutExtension(fileName);
                    }

                    fileName = fileName.Remove(fileName.Length -
                                               (fileName.Length - Program.ConfigManager.Config.UploadNameLimit));
                }

                if (Program.ConfigManager.Config.ReplaceUnderscores)
                {
                    fileName = fileName.Replace("_", " ");
                }

                var safeFileName =
                    Uri.EscapeDataString(Uri.UnescapeDataString(fileName.Trim()));

                var upload = new MediaSchema.MediaUpload()
                {
                    Name = safeFileName,
                    Extension = fileExtension,
                    Content = data,
                    Public = visible
                };
                await PostUploadContent(upload);
            }

            return RedirectToPage("/content");
        }

        return Redirect("/400");
    }

    [HttpPost("/api/upload/")]
    public async Task<ActionResult> PostUploadContent(MediaSchema.MediaUpload upload)
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            DatabaseSchema.User user = await UserUtils.GetUserWithChildren(Request.Cookies["user_session"]);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var mediaExtension = ContentUtils.GetContentExtension(upload.Content, upload.Extension);
            if (mediaExtension == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var contentType = ContentUtils.GetContentType(mediaExtension);
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

            if (upload.Name.Length > Program.ConfigManager.Config.UploadNameLimit || upload.Name.Length <= 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool contentCompressed = Program.ConfigManager.Config.LosslessCompression;
            byte[] content;

            if (Program.ConfigManager.Config.LosslessCompression
                && LZ4Codec.MaximumOutputSize(upload.Content.Length) < upload.Content.Length)
            {
                content = LZ4Pickler.Pickle(upload.Content, Program.ConfigManager.Config.LosslessCompressionLevel);
            }
            else
            {
                content = upload.Content;
                contentCompressed = false;
            }

            var mediaSchema = new DatabaseSchema.Media()
            {
                Id = await Program.Database.MediaDatabase.GenerateUniqueMediaId(Program.ConfigManager.Config
                    .UniqueIdLength),
                Name = Uri.EscapeDataString(Uri.UnescapeDataString(upload.Name)),
                Extension = mediaExtension,
                UploadDate = DateTime.UtcNow,
                ContentSize = content.Length,
                ContentCompressed = contentCompressed,
                ContentMime = FileExtensionContentTypeProvider.TryGetContentType($"{upload.Name}{mediaExtension}",
                    out string mimeType)
                    ? mimeType
                    : "application/octet-stream",
                ContentType = (ContentType) contentType,
                Public = upload.Public,
                AuthorId = user.Id
            };

            mediaSchema.ContentPath = Program.ContentManager.SaveContent(content, mediaSchema.Id, mediaSchema.Name,
                mediaSchema.Extension, (ContentType) contentType);

            if (contentType == ContentType.Video || contentType == ContentType.Image)
            {
                mediaSchema.ContentDimensions = ContentUtils.GetDimensions(upload.Content, (ContentType) contentType);
                if (Program.ConfigManager.Config.Thumbnails && Program.ConfigManager.Config.PreComputeThumbnails)
                {
                    var thumbnail = await ContentUtils.GetThumbnail(upload.Content,
                        Program.ConfigManager.Config.ThumbnailWidth, (ContentType) contentType,
                        Program.ConfigManager.Config.ThumbnailFormat);
                    if (thumbnail != null)
                    {
                        mediaSchema.ThumbnailPath = Program.ContentManager.SaveThumbnail(thumbnail, mediaSchema.Id,
                            mediaSchema.Name, Program.ConfigManager.Config.ThumbnailFormat.FileExtensions.ToList()[0],
                            (ContentType) contentType);
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Failed to pre compute thumbnail (ID: {mediaSchema.Id}, Name: {mediaSchema.Name}, Extension: {mediaSchema.Extension})");
                    }
                }
            }

            await Program.Database.MediaDatabase.InsertWithChildrenAsync(mediaSchema);

            user.Uploads.Add(new MediaSchema.MediaIdentity()
            {
                Id = mediaSchema.Id,
                Name = mediaSchema.Name
            });
            await Program.Database.UserDatabase.UpdateWithChildrenAsync(user);
            return StatusCode(StatusCodes.Status200OK);
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }

    [HttpGet("/api/delete/")]
    public async Task<ActionResult> GetDeleteContent([FromQuery] MediaSchema.MediaIdentity identity)
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return RedirectToPage("/login");
            }

            DatabaseSchema.User user = await UserUtils.GetUserWithChildren(Request.Cookies["user_session"]);

            if (user == null)
            {
                return Redirect("/404");
            }

            var media = await Program.Database.MediaDatabase.FindAsync<DatabaseSchema.Media>(media =>
                media.Id == identity.Id && media.Name == identity.Name);

            if (media == null)
            {
                return Redirect("/404");
            }

            if (media.AuthorId != user.Id && !user.Admin)
            {
                return Redirect("/403");
            }

            if (Program.ContentManager.DeleteContent(media.Id, media.Name, media.Extension, media.ContentType))
            {
                user.Uploads.Remove(new MediaSchema.MediaIdentity()
                {
                    Id = media.Id,
                    Name = media.Name
                });
                await Program.Database.MediaDatabase.DeleteAsync<DatabaseSchema.Media>(media.Id);
                await Program.Database.UserDatabase.UpdateWithChildrenAsync(user);
                return RedirectToPage("/content");
            }

            return Redirect("/500");
        }

        return StatusCode(StatusCodes.Status400BadRequest, ModelState);
    }
}