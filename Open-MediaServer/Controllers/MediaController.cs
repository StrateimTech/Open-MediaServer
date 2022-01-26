using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Open_MediaServer.Schema;
using Open_MediaServer.Utils;

namespace Open_MediaServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        [HttpGet("/api/media/thumbnail/{mediaType}/{mediaName}")]
        public MediaSchema.MediaThumbnail GetThumbnail(MediaType mediaType, string mediaName)
        {
            if (mediaType == MediaType.Other)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return null;
            }
            return Task.Factory.StartNew(() =>
            {
                if (!MediaUtils.IsThumbnailCached(Path.GetFileName(mediaName)))
                {
                    var mediaBytes = MediaUtils.GetMedia(mediaType, Path.GetFileName(mediaName));
                    var thumbnail = ThumbnailUtils.GetReduceThumbnail(mediaType, 115, 115, mediaBytes);
                    MediaUtils.SaveThumbnailCache(Path.GetFileName(mediaName), thumbnail);
                    return new MediaSchema.MediaThumbnail
                    {
                        Thumbnail = thumbnail
                    };
                }
                else
                {
                    var thumbnail = MediaUtils.GetThumbnailCached(Path.GetFileName(mediaName));
                    return new MediaSchema.MediaThumbnail
                    {
                        Thumbnail = thumbnail
                    };
                }
            }).Result;
        }

        [HttpGet("/api/media/images")]
        public MediaSchema.MediaGetSchema GetImages()
        {
            return Task.Factory.StartNew(() =>
            {
                var mediaNames = MediaUtils.GetAllMediaNames(MediaType.Image);

                List<MediaSchema.Media> media = new List<MediaSchema.Media>();
                foreach (var mediaName in mediaNames)
                {
                    var mediaBytes = MediaUtils.GetMedia(MediaType.Image, Path.GetFileName(mediaName));
                    media.Add(new MediaSchema.Media
                    {
                        Name = Path.GetFileNameWithoutExtension(mediaName),
                        Extension = Path.GetExtension(mediaName),
                        Date = System.IO.File.GetCreationTime(mediaName),
                        Size = mediaBytes.Length
                    });
                }

                return new MediaSchema.MediaGetSchema
                {
                    Media = media
                };
            }).Result;
        }


        [HttpGet("/api/media/videos")]
        public MediaSchema.MediaGetSchema GetVideos()
        {
            return Task.Factory.StartNew(() =>
            {
                var mediaNames = MediaUtils.GetAllMediaNames(MediaType.Video);

                List<MediaSchema.Media> media = new List<MediaSchema.Media>();
                foreach (var mediaName in mediaNames)
                {
                    var mediaBytes = MediaUtils.GetMedia(MediaType.Video, Path.GetFileName(mediaName));
                    media.Add(new MediaSchema.Media
                    {
                        Name = Path.GetFileNameWithoutExtension(mediaName),
                        Extension = Path.GetExtension(mediaName),
                        Date = System.IO.File.GetCreationTime(mediaName),
                        Size = mediaBytes.Length
                    });
                }

                return new MediaSchema.MediaGetSchema
                {
                    Media = media
                };
            }).Result;
        }

        [HttpGet("/api/media/other")]
        public MediaSchema.MediaGetSchema GetOther()
        {
            return Task.Factory.StartNew(() =>
            {
                var mediaNames = MediaUtils.GetAllMediaNames(MediaType.Other);

                List<MediaSchema.Media> media = new List<MediaSchema.Media>();
                foreach (var mediaName in mediaNames)
                {
                    var mediaBytes = MediaUtils.GetMedia(MediaType.Other, Path.GetFileName(mediaName));
                    media.Add(new MediaSchema.Media
                    {
                        Name = Path.GetFileNameWithoutExtension(mediaName),
                        Extension = Path.GetExtension(mediaName),
                        Date = System.IO.File.GetCreationTime(mediaName),
                        Size = mediaBytes.Length
                    });
                }

                return new MediaSchema.MediaGetSchema
                {
                    Media = media
                };
            }).Result;
        }

        [HttpGet("/api/media/amount")]
        public MediaSchema.MediaAmountGetSchema GetAmount()
        {
            return Task.Factory.StartNew(() => new MediaSchema.MediaAmountGetSchema
            {
                ImagesSize = MediaUtils.GetAllMediaNames(MediaType.Image).Count,
                VideosSize = MediaUtils.GetAllMediaNames(MediaType.Video).Count,
                OtherSize = MediaUtils.GetAllMediaNames(MediaType.Other).Count
            }).Result;
        }


        [HttpPost("/api/media/upload")]
        public ActionResult UploadFile(MediaSchema.MediaUploadSchema mediaUpload)
        {
            // Very secure method!!
            if (mediaUpload.AuthKey == "password1234")
            {
                if (!MediaUtils.IsMedia(mediaUpload.MediaType, mediaUpload.Extension))
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                if (mediaUpload.Extension == null || mediaUpload.Name == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                if (mediaUpload.Name.Any(Char.IsWhiteSpace))
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                string file = $"{mediaUpload.Name}{mediaUpload.Extension}";

                if (!MediaUtils.MediaExist(mediaUpload.MediaType, file))
                {
                    if (MediaUtils.AddMedia(mediaUpload.MediaType, file, mediaUpload.Bytes))
                    {
                        return StatusCode(StatusCodes.Status200OK);
                    }

                    return StatusCode(StatusCodes.Status409Conflict);
                }

                return StatusCode(StatusCodes.Status409Conflict);
            }

            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        [HttpPost("/api/media/delete")]
        public ActionResult DeleteFile(MediaSchema.MediaDeleteSchema mediaDelete)
        {
            // Very secure method!!
            if (mediaDelete.AuthKey == "password1234")
            {
                if (!MediaUtils.IsMedia(mediaDelete.MediaType, mediaDelete.Extension))
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                if (mediaDelete.Extension == null || mediaDelete.Name == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                string file = $"{mediaDelete.Name}{mediaDelete.Extension}";

                if (MediaUtils.MediaExist(mediaDelete.MediaType, file))
                {
                    if (MediaUtils.MediaDelete(mediaDelete.MediaType, file))
                    {
                        return StatusCode(StatusCodes.Status200OK);
                    }

                    return StatusCode(StatusCodes.Status409Conflict);
                }

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            return StatusCode(StatusCodes.Status401Unauthorized);
        }
    }
}