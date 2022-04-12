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

    public void SaveContent(byte[] content, string id, string name, string extension, ContentType contentType)
    {
        var filePath = Path.Combine(_contentDirectory, "Media", contentType.GetDisplayName(), id, $"{name}.{extension}");
        
        Directory.CreateDirectory(filePath);
        if (!File.Exists(filePath))
        {
            File.Create(filePath);
        }
    }
}