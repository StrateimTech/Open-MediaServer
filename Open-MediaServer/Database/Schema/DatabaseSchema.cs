using System;
using System.Collections.Generic;
using Open_MediaServer.Backend.Schema;
using Open_MediaServer.Content;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Open_MediaServer.Database.Schema;

public class DatabaseSchema
{
    public class Media
    {
        [PrimaryKey]
        public string Id { get; init; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public int ContentSize { get; set; }
        public string ThumbnailPath { get; set; }
        public string ContentPath { get; set; }
        public ContentType ContentType { get; set; }
        public bool ContentCompressed { get; set; }
        public bool Public { get; set; }
        public DateTime UploadDate { get; set; }
        public int AuthorId { get; set; }
    }
    
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(string))]
        public string Username { get; set; }
        public string Bio { get; set; }
        public byte[] Salt { get; set; }
        public string Password { get; set; }
        public string SessionKey { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Admin { get; set; }
        [TextBlob(nameof(UploadsBlobbed))]
        public List<MediaSchema.MediaIdentity> Uploads { get; set; }
        //Ignore
        public string UploadsBlobbed { get; set; }
    }
}