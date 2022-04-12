using System;
using System.Linq;
using Open_MediaServer.Content;

namespace Open_MediaServer.Utils;

public static class ContentUtils
{
    public static ContentType? GetContentType(/*byte[] data,*/string extension)
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

    public static byte[] GetThumbnail()
    {
        return null;
    }
}