using System;
using System.IO;
using System.Linq;
using HeyRed.ImageSharp.AVCodecFormats;
using Open_MediaServer.Content;
using SixLabors.ImageSharp;
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

    public static byte[] GetThumbnail(byte[] data, int? width, int? height, ContentType contentType)
    {
        switch (contentType)
        {
            case ContentType.Image:
            {
                using var image = Image.Load<Rgba32>(data);
                if(width != null && height != null)
                    image.Mutate(x => x.Resize((int)width, (int)height));
                
                using var ms = new MemoryStream();
                image.Save(ms, PngFormat.Instance);
                return ms.ToArray();
            }
            case ContentType.Video:
            {
                var configuration = new Configuration().WithAVDecoders();
                using var image = Image.Load<Rgba32>(configuration, data);

                if(width != null && height != null)
                    image.Mutate(x => x.Resize((int)width, (int)height));

                using var ms = new MemoryStream();
                image.Save(ms, PngFormat.Instance);
                image.Dispose();
                return ms.ToArray();
            }
        }
        return null;
    }
}