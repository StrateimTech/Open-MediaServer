using System;
using System.Collections.Generic;
using Open_MediaServer.Utils;

namespace Open_MediaServer.Schema
{
    public static class MediaSchema
    {
        public class MediaGetSchema
        {
            public List<Media> Media { get; set; }
        }
        
        public class MediaAmountGetSchema
        {
            public int ImagesSize { get; set; }
            public int VideosSize { get; set; }
            public int OtherSize { get; set; }
        }

        public class MediaUploadSchema
        {
            public string AuthKey { get; set; }

            public string Name { get; set; }

            public string Extension { get; set; }

            public byte[] Bytes { get; set; }

            public MediaType MediaType { get; set; }
        }
        
        public class MediaDeleteSchema
        {
            public string AuthKey { get; set; }

            public string Name { get; set; }

            public string Extension { get; set; }

            public MediaType MediaType { get; set; }
        }
        
        public class Media
        {
            public string Name { get; set; }
            public string Extension { get; set; }
            
            public DateTime Date { get; set; }
            public int Size { get; set; }
        }

        public class MediaThumbnail
        {
            public byte[] Thumbnail { get; set; }
        }
    }
}