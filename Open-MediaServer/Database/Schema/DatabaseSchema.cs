using System;
using SQLite;

namespace Open_MediaServer.Database.Schema;

public class DatabaseSchema
{
    public class Media
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Author { get; set; }
        public DateTime UploadDate { get; set; }
        public int Size { get; set; }
        public string ThumbnailPath { get; set; }
        public string ContentPath { get; set; }
        public bool ContentCompressed { get; set; }
    }
}