using System;
using System.IO;
using Microsoft.OpenApi.Extensions;

namespace Open_MediaServer.Content;

public class ContentManager
{
    private readonly string _contentDirectory;

    public ContentManager(string directory)
    {
        _contentDirectory = directory;
    }

    public string SaveContent(byte[] content, string id, string name, string extension, ContentType contentType)
    {
        var safeName = Path.GetFileNameWithoutExtension(name);
        var safeExtension = Path.GetExtension(extension);

        var filePath = Path.Join(_contentDirectory, "Media", contentType.GetDisplayName(), id,
            $"{safeName}{safeExtension}");
        if (Path.GetFullPath(filePath) != filePath)
            return null;

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        if (!File.Exists(filePath))
        {
            File.WriteAllBytes(filePath, content);
        }

        return filePath;
    }

    public string SaveThumbnail(byte[] thumbnail, string id, string name, string extension, ContentType contentType)
    {
        var safeName = Path.GetFileNameWithoutExtension(name);
        var safeExtension = Path.GetExtension(extension.Insert(0, "."));

        var filePath = Path.Join(_contentDirectory, "Media", contentType.GetDisplayName(), id,
            $"{safeName}_thumbnail{safeExtension}");
        if (Path.GetFullPath(filePath) != filePath)
            return null;

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        if (!File.Exists(filePath))
        {
            File.WriteAllBytes(filePath, thumbnail);
        }

        return filePath;
    }

    public bool DeleteContent(string id, string name, string extension, ContentType contentType)
    {
        var safeName = Path.GetFileNameWithoutExtension(name);
        var safeExtension = Path.GetExtension(extension);

        var filePath = Path.Join(_contentDirectory, "Media", contentType.GetDisplayName(), id,
            $"{safeName}{safeExtension}");
        if (Path.GetFullPath(filePath) != filePath)
            return false;

        if (Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Directory.Delete(Path.GetDirectoryName(filePath)!, true);
            return true;
        }
        return false;
    }
}