using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeyRed.ImageSharp.AVCodecFormats;
using HeyRed.ImageSharp.AVCodecFormats.Mp4;
using Microsoft.AspNetCore.Http;
using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Definitions.Licensing;
using MimeDetective.Storage;
using Open_MediaServer.Content;
using Open_MediaServer.Database.Schema;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Open_MediaServer.Utils;

public static class ContentUtils
{
    public static ContentType? GetContentType(string extension)
    {
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

    public static string GetContentExtension(byte[] data, string extension)
    {
        List<string> extensionList = new List<string>();
        extensionList.AddRange(Program.ConfigManager.Config.VideoTypes.ToList()
            .Select(str => str.Replace(".", "").ToUpper()));
        extensionList.AddRange(Program.ConfigManager.Config.ImageTypes.ToList()
            .Select(str => str.Replace(".", "").ToUpper()));
        if (Program.ConfigManager.Config.OtherTypes != null || Program.ConfigManager.Config.OtherTypes?.Length > 0)
        {
            extensionList.AddRange(Program.ConfigManager.Config.OtherTypes.ToList()
                .Select(str => str.Replace(".", "").ToUpper()));
        }
        
        if (Program.ConfigManager.Config.UseMimeDetective)
        {
            var definitions = new ExhaustiveBuilder
            {
                UsageType = UsageType.PersonalNonCommercial
            }.Build();

            if (extensionList.Count > 0)
            {
                definitions = definitions
                    .ScopeExtensions(extensionList)
                    .ToImmutableArray();
            }
            
            definitions
                .TrimMeta()
                .TrimDescription()
                .TrimMimeType()
                .ToImmutableArray();
            
            var allCondensedDefinitions = new ContentInspectorBuilder
            {
                Definitions = definitions
            }.Build();
            
            var inspection = allCondensedDefinitions.Inspect(data);
            
            if (inspection.ByFileExtension().Length > 0)
            {
                var extensionMatch = inspection.ByFileExtension()[0];
                extension = extensionMatch.Extension;
            
                if (!extension.Contains("."))
                {
                    extension = extension.Insert(0, ".");
                }
            
                return extension;
            }

            if (Program.ConfigManager.Config.FallBackToFileExtension)
            {
                if (extensionList.Count > 0)
                {
                    if (extensionList.Contains(extension.Replace(".", "").ToUpper()))
                    {
                        return extension;
                    }
                }
                else
                {
                    return extension;
                }
            }
            
            return null;
        }
        
        if (extensionList.Count > 0)
        {
            if (extensionList.Contains(extension.Replace(".", "").ToUpper()))
            {
                return extension;
            }
        }
        else
        {
            return extension;
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

        await using var ms = new MemoryStream();
        await image.SaveAsync(ms, format);
        return ms.ToArray();
    }

    public static async Task<byte[]> WatermarkContent(byte[] data)
    {
        Image<Rgba32> image = Image.Load<Rgba32>(data);
        IImageFormat format = Image.DetectFormat(data);

        if (format == null)
        {
            return null;
        }
        
        SystemFonts.TryGet("Lato", out FontFamily fontFamily);
        
        var font = fontFamily.CreateFont(96, FontStyle.Regular);
        
        var textOptions = new TextOptions(font)
        {
            KerningMode = KerningMode.Normal
        };
        
        var text = Program.ConfigManager.Config.Watermark;
        
        var fontRectangle = TextMeasurer.Measure(text, textOptions);
        
        float scalingFactor = Math.Min(image.Width / fontRectangle.Width, image.Height / fontRectangle.Height);
        var factor = scalingFactor / 4.5;
        var fontFactored = factor * 96;
        
        var scaledFont = new Font(font, (int)fontFactored);
        
        var scaledTextOptions = new TextOptions(scaledFont)
        {
            KerningMode = KerningMode.Normal
        };
        
        var scaledFontRectangle = TextMeasurer.Measure(text, scaledTextOptions);
        
        image.Mutate(x => x.DrawText(
            text,
            scaledFont,
            Color.White,
            new PointF(image.Width - scaledFontRectangle.Width - 16, image.Height - scaledFontRectangle.Height - 16)
        ));
        
        await using var ms = new MemoryStream();
        await image.SaveAsync(ms, format);
        return ms.ToArray();
    }

    public static List<DatabaseSchema.Media> SortMediaFromQuery(this List<DatabaseSchema.Media> mediaList,
        IQueryCollection queryCollection)
    {
        if (queryCollection.Count > 0)
        {
            foreach (var collection in queryCollection)
            {
                if (collection.Value[0] == null)
                    continue;
                Boolean.TryParse(collection.Value[0], out bool value);
                if (!value)
                    continue;
                switch (collection.Key.ToLower())
                {
                    case "name":
                        mediaList = mediaList.OrderBy(media => Uri.UnescapeDataString(media.Name)).ToList();
                        break;
                    case "author":
                        mediaList = mediaList.OrderBy(media => media.AuthorId).ToList();
                        break;
                    case "date":
                        mediaList = mediaList.OrderBy(media => media.UploadDate).Reverse().ToList();
                        break;
                    case "type":
                        mediaList = mediaList.OrderBy(media => media.Extension).ToList();
                        break;
                    case "size":
                        mediaList = mediaList.OrderBy(media => media.ContentSize).Reverse().ToList();
                        break;
                    case "visibility":
                        mediaList = mediaList.OrderBy(media => media.Public).ToList();
                        break;
                }
            }
        }

        return mediaList;
    }

    public static bool IsSelectedFromSortQuery(IQueryCollection queryCollection, String sortValue)
    {
        if (queryCollection.Count > 0)
        {
            foreach (var collection in queryCollection)
            {
                if (collection.Key.ToLower() == sortValue.ToLower())
                {
                    if (collection.Value[0] == null)
                        continue;
                    Boolean.TryParse(collection.Value[0], out bool value);
                    return value;
                }
            }
        }

        return false;
    }
}