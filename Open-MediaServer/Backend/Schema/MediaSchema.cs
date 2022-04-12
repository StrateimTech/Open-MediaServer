using System;
using SQLite;

namespace Open_MediaServer.Backend.Schema;

public class MediaSchema
{
    public class MediaStats
    {
        public int ContentCount { get; set; }
        public int ContentTotalSize { get; set; }
    }

    public class MediaUpload
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Content { get; set; }
    }
}