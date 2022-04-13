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
        var filePath = Path.Combine(_contentDirectory, "Media", contentType.GetDisplayName(), id, $"{name}{extension}");
        
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        if (!File.Exists(filePath))
        {
            File.WriteAllBytes(filePath, content);
        }

        return filePath;
    }
    
    public string SaveThumbnail(byte[] thumbnail, string id, string name, string extension, ContentType contentType)
    {
        var filePath = Path.Combine(_contentDirectory, "Media", contentType.GetDisplayName(), id, $"{name}_thumbnail{extension}");
        
        Directory.CreateDirectory(filePath);
        if (!File.Exists(filePath))
        {
            File.WriteAllBytes(filePath, thumbnail);
        }

        return filePath;
    }
}