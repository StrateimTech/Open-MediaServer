using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Open_MediaServer.Utils
{
    public static class MediaUtils
    {
        public static byte[] GetMedia(MediaType type, string name)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/{type.ToString()}/{name}";
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return null;
        }
        
        public static byte[] GetThumbnailCached(string name)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/Cache/{name}";
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return null;
        }
        public static bool IsThumbnailCached(string name)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/Cache/{name}";
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }
        
        public static void SaveThumbnailCache(string name, byte[] data)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/Cache/{name}";
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, data);
            }
        }

        public static List<string> GetAllMediaNames(MediaType type)
        {
            List<string> localList = new();
            var directory = $"{Directory.GetCurrentDirectory()}/Media/{type.ToString()}";
            if (Directory.Exists(directory))
            {
                foreach (var fileName in Directory.GetFiles(directory))
                {
                    localList.Add(fileName);
                }
            }

            return localList;
        }

        public static bool AddMedia(MediaType type, string name, byte[] data)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/{type.ToString()}/{name}";
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, data);
                return true;
            }

            return false;
        }

        public static bool MediaExist(MediaType type, string name)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/{type.ToString()}/{name}";
            if (File.Exists(path))
            {
                return true;
            }

            return false;
        }

        public static bool MediaDelete(MediaType type, string name)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Media/{type.ToString()}/{name}";
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        private static readonly string[] VideoExtensions =
        {
            ".avi", ".mkv", ".mov", ".mp4", ".webm", ".wmv"
        };

        private static readonly string[] ImageExtensions =
        {
            ".tif", ".tiff", ".bmp", ".jpg", ".jpeg", ".gif", ".png", ".webp"
        };

        public static bool IsMedia(MediaType type, string name)
        {
            switch (type)
            {
                case MediaType.Images:
                    if (ImageExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                        return true;
                    break;
                case MediaType.Videos:
                    if (VideoExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                        return true;
                    break;
                case MediaType.Other:
                    if (!ImageExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase) &&
                        !VideoExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                        return true;
                    break;
            }
            return false;
        }
    }
}