using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HeyRed.ImageSharp.AVCodecFormats;
using HeyRed.ImageSharp.AVCodecFormats.Webm;
using Microsoft.OpenApi.Extensions;
using Open_MediaServer.Content;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Open_MediaServer.Utils;

public static class ContentUtils
{
    public static ContentType? GetContentType( /*byte[] data,*/ string extension)
    {
        // TODO: This can be easily spoofed just by changing the extension... couldn't find a good file type identifier library
        if (Program.ConfigManager.Config.VideoTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return ContentType.Video;
        if (Program.ConfigManager.Config.ImageTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return ContentType.Image;
        if (Program.ConfigManager.Config.OtherTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return ContentType.Other;
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

    public static async Task<byte[]> GetThumbnail(byte[] data, int? width, int? height, ContentType contentType,
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
        
        if (width != null && height != null)
        {
            image.Mutate(x => x.Resize((int) width, (int) height));
        }
    
        using var ms = new MemoryStream();
        await image.SaveAsync(ms, format);
        return ms.ToArray();
    }
}