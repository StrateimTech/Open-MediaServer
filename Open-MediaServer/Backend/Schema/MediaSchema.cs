using System;

namespace Open_MediaServer.Backend.Schema;

public class MediaSchema
{
    // ID 8 long abc..123 (3vwtjiUg)
    // Name (Hello_World_abc)
    // Extension (mp4)
    // Author (Admin)
    // UploadDate (4/11/22)
    // Size (Bytes)
    
    public class Media
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Author { get; set; }
        
        public DateTime UploadDate { get; set; }
        public int Size { get; set; }
    }
}