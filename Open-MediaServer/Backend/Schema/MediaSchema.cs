using System;
using SQLite;

namespace Open_MediaServer.Backend.Schema;

public class MediaSchema
{
    public class MediaIdentity
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    
    // public class MediaUser
    // {
    //     public string Id { get; set; }
    //     public string Username { get; set; }
    // }
    
    public class MediaStats
    {
        public int ContentCount { get; set; }
        public int ContentTotalSize { get; set; }
        public bool VideoContent { get; set; }
        public bool ImageContent { get; set; }
        public bool OtherContent { get; set; }
    }

    public class MediaUpload
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Content { get; set; }
    }
}