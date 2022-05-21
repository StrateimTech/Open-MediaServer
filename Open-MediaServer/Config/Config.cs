using K4os.Compression.LZ4;

namespace Open_MediaServer.Config;

public class Config
{
    public string FrontendName { get; set; } = "";
    
    public string? WorkingDirectory = null;

    // Length of generated uid 
    public int UniqueIdLength { get; set; } = 8;

    // This value is in MBs (Can be null for no limit)
    // Maximum network file upload limit
    public int? FileNetworkUploadMax { get; set; } = null;

    // Developer tools
    public bool ShowSwaggerUi { get; set; } = true;

    public bool AllowSignups { get; set; } = true;
    public bool AllowImages { get; set; } = true;
    public bool AllowVideos { get; set; } = true;
    public bool AllowOther { get; set; } = true;

    public bool Thumbnails { get; set; } = true;
    public bool PreComputeThumbnails { get; set; } = true;

    // null size will result in the video resolution's size being used 
    public (int, int)? ThumbnailSize { get; set; } = null;

    public string ThumbnailType { get; set; } = ".png";

    public string[] ImageTypes { get; set; } =
    {
        ".png", ".tif", ".tiff", ".bmp", ".jpg", ".jpeg", ".gif", ".webp"
    };

    public string[] VideoTypes { get; set; } =
    {
        ".mp4", ".avi", ".mkv", ".mov", ".webm", ".wmv"
    };

    public string[] OtherTypes { get; set; } =
    {
        ".txt"
    };

    public bool LosslessCompression { get; set; } = true;
    public LZ4Level LosslessCompressionLevel { get; set; } = LZ4Level.L12_MAX;
}