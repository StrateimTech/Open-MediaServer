using System;
using System.ComponentModel.DataAnnotations;
using SQLite;

namespace Open_MediaServer.Backend.Schema;

public class MediaSchema
{
    public class MediaIdentity
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
    
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
        [Required]
        public string Name { get; set; }
        [Required]
        public string Extension { get; set; }
        [Required]
        public byte[] Content { get; set; }
        public bool Public { get; set; }
    }
}