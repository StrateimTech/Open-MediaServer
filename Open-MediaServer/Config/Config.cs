namespace Open_MediaServer.Config;

public class Config
{
    public string? WorkingDirectory = null;
    
    // Length of generated uid 
    public int UniqueIdLength { get; set; } = 8;
    
    // This value is in MBs (Can be null for no limit)
    // Maximum network file upload limit
    public int? FileNetworkUploadMax { get; set; } = null;
    
    // Developer tools
    public bool ShowSwaggerUi { get; set; } = true;

    public bool AllowImages { get; set; } = true;
    public bool AllowVideos { get; set; } = true;
    public bool AllowOther { get; set; } = true;
    
    public string ThumbnailType = "png";

    public string[] ImageTypes = new[]
    {
        "png"
    };
    
    public string[] VideoTypes = new[]
    {
        "mp4"
    };
    
    public string[] OtherTypes = new[]
    {
        "txt"
    };
    
    public bool LosslessCompression { get; set; } = true;
}