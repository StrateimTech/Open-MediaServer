using System;
using System.IO;
using HeyRed.ImageSharp.AVCodecFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Open_MediaServer.Utils
{
    public static class ThumbnailUtils
    {
        public static byte[] GetReduceThumbnail(MediaType mediaType, int width, int height, byte[] array)
        {
            if (mediaType == MediaType.Images)
            {
                try
                {
                    using var image = Image.Load<Rgba32>(array);
                    image.Mutate(x => x.Resize(width, height));
                    using var ms = new MemoryStream();
                    image.Save(ms, PngFormat.Instance);
                    return ms.ToArray();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to resize image ({Directory.GetCurrentDirectory()}/Assets/Icons/icons8-image-file-384.png)");
                    var fileIconPath = $"{Directory.GetCurrentDirectory()}/Assets/Icons/icons8-image-file-384.png";
                    if (File.Exists(fileIconPath))
                    {
                        using var image = Image.Load<Rgba32>(File.ReadAllBytes(fileIconPath));
                        image.Mutate(x => x.Resize(width, height));
                        using var ms = new MemoryStream();
                        image.Save(ms, PngFormat.Instance);
                        return ms.ToArray();
                    }
                    return null;
                }
            }
            
            if(mediaType == MediaType.Videos)
            {
                try
                {
                    var configuration = new Configuration().WithAVDecoders();
                    using var image = Image.Load<Rgba32>(configuration, array);
            
                    image.Mutate(x => x.Resize(width, height)); 

                    using var ms = new MemoryStream();
                    image.Save(ms, PngFormat.Instance);
                    image.Dispose();
                    return ms.ToArray();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to resize video image ({Directory.GetCurrentDirectory()}/Assets/Icons/icons8-video-file-96.png)");
                    var fileIconPath = $"{Directory.GetCurrentDirectory()}/Assets/Icons/icons8-video-file-96.png";
                    if (File.Exists(fileIconPath))
                    {
                        using var image = Image.Load<Rgba32>(File.ReadAllBytes(fileIconPath));
                        image.Mutate(x => x.Resize(width, height));
                        using var ms = new MemoryStream();
                        image.Save(ms, PngFormat.Instance);
                        return ms.ToArray();
                    }
                    return null;
                }
            }
            return null;
        }
    }
}