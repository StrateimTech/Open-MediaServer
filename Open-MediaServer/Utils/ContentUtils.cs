using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeyRed.ImageSharp.AVCodecFormats;
using Microsoft.AspNetCore.Http;
using Open_MediaServer.Content;
using Open_MediaServer.Database.Schema;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Open_MediaServer.Utils;

public static class ContentUtils
{
    public static ContentType? GetContentType( /*byte[] data,*/ string extension)
    {
        // TODO: This can be easily spoofed just by changing the extension... couldn't find a good file type identifier library
        if (Program.ConfigManager.Config.VideoTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return ContentType.Video;
        }

        if (Program.ConfigManager.Config.ImageTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return ContentType.Image;
        }

        if (Program.ConfigManager.Config.OtherTypes == null ||
            Program.ConfigManager.Config.OtherTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return ContentType.Other;
        }

        return null;
    }

    public static (int, int) GetDimensions(byte[] data, ContentType contentType)
    {
        Image<Rgba32> image;
        switch (contentType)
        {
            case ContentType.Video:
                var configuration = new Configuration().WithAVDecoders();
                image = Image.Load<Rgba32>(configuration, data);
                break;
            default:
                image = Image.Load<Rgba32>(data);
                break;
        }

        return (image.Width, image.Height);
    }

    public static async Task<byte[]> GetThumbnail(byte[] data, int? width, ContentType contentType,
        IImageFormat format)
    {
        Image<Rgba32> image;
        switch (contentType)
        {
            case ContentType.Video:
                var configuration = new Configuration().WithAVDecoders();
                image = Image.Load<Rgba32>(configuration, data);
                break;
            default:
                image = Image.Load<Rgba32>(data);
                break;
        }

        if (width != null)
        {
            image.Mutate(x => x.Resize((int) width, (int) (image.Height / image.Width * width)));
        }

        using var ms = new MemoryStream();
        await image.SaveAsync(ms, format);
        return ms.ToArray();
    }

    public static List<DatabaseSchema.Media> SortMediaFromQuery(this List<DatabaseSchema.Media> mediaList,
        IQueryCollection queryCollection)
    {
        if (queryCollection.Count > 0)
        {
            if (queryCollection.ContainsKey("name"))
            {
                var nameSort = queryCollection["name"];
                if (nameSort[0] != null)
                {
                    if (Boolean.TryParse(nameSort[0], out bool value) && value)
                    {
                        mediaList = mediaList.OrderBy(media => Uri.UnescapeDataString(media.Name)).ToList();
                    }
                }
            }

            if (queryCollection.ContainsKey("author"))
            {
                var authorSort = queryCollection["author"];

                if (authorSort[0] != null)
                {
                    if (Boolean.TryParse(authorSort[0], out bool value) && value)
                    {
                        mediaList = mediaList.OrderBy(media => media.AuthorId).ToList();
                    }
                }
            }

            if (queryCollection.ContainsKey("date"))
            {
                var dateSort = queryCollection["date"];

                if (dateSort[0] != null)
                {
                    if (Boolean.TryParse(dateSort[0], out bool value) && value)
                    {
                        mediaList = mediaList.OrderBy(media => media.UploadDate).Reverse().ToList();
                    }
                }
            }

            if (queryCollection.ContainsKey("type"))
            {
                var typeSort = queryCollection["type"];

                if (typeSort[0] != null)
                {
                    if (Boolean.TryParse(typeSort[0], out bool value) && value)
                    {
                        mediaList = mediaList.OrderBy(media => media.Extension).ToList();
                    }
                }
            }

            if (queryCollection.ContainsKey("size"))
            {
                var sizeSort = queryCollection["size"];

                if (sizeSort[0] != null)
                {
                    if (Boolean.TryParse(sizeSort[0], out bool value) && value)
                    {
                        mediaList = mediaList.OrderBy(media => media.ContentSize).Reverse().ToList();
                    }
                }
            }

            if (queryCollection.ContainsKey("visibility"))
            {
                var visibilitySort = queryCollection["visibility"];

                if (visibilitySort[0] != null)
                {
                    if (Boolean.TryParse(visibilitySort[0], out bool value) && value)
                    {
                        mediaList = mediaList.OrderBy(media => media.Public).ToList();
                    }
                }
            }
        }

        return mediaList;
    }
}