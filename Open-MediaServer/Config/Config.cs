using HeyRed.ImageSharp.AVCodecFormats.Webm;
using K4os.Compression.LZ4;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;

namespace Open_MediaServer.Config;

public class Config
{
    public string FrontendName { get; set; }

    public string FrontendDomain { get; set; }

    public (short http, short https) FrontendPorts { get; set; } = (80, 443);
    public (short http, short https) BackendPorts { get; set; } = (2000, 2001);
    
    public string? WorkingDirectory { get; set; } = null;

    // Length of generated uid 
    public int UniqueIdLength { get; set; } = 8;

    // This value is in MBs (Can be null for no limit)
    // Maximum network file upload limit
    public int? FileNetworkUploadMax { get; set; } = null;

    // Developer tools
    public bool ShowSwaggerUi { get; set; } = true;

    public bool ShowConsoleProviders { get; set; } = true;

    public bool ForceHttpsRedirection { get; set; } = false;

    public bool AllowRegistering { get; set; } = true;
    public bool AllowImages { get; set; } = true;
    public bool AllowVideos { get; set; } = true;
    public bool AllowOther { get; set; } = true;

    public bool Thumbnails { get; set; } = true;
    public bool PreComputeThumbnails { get; set; } = true;

    // null size will result in the video resolution's size being used 
    public (int width, int height)? ThumbnailSize { get; set; } = (200, 113);

    [JsonProperty( TypeNameHandling = TypeNameHandling.Objects )]
    public IImageFormat ThumbnailFormat { get; set; } = WebpFormat.Instance;

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