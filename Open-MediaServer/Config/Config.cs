#nullable enable
using System;
using K4os.Compression.LZ4;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;

namespace Open_MediaServer.Config;

public class Config
{
    public string FrontendName { get; set; } = String.Empty;

    public string FrontendDomain { get; set; } = String.Empty;

    /// <summary>
    /// Associated instances in the form of URLs
    /// </summary>
    public string[] AssociatedInstances { get; set; } = {};
    
    /// <summary>
    /// Index page stats' container footer
    /// </summary>
    public string StatsFooterText { get; set; } = "Empty Footer!";

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

    public int UploadNameLimit { get; set; } = 64;
    public bool ReplaceUnderscores { get; set; } = true;
    public bool Thumbnails { get; set; } = true;
    public bool PreComputeThumbnails { get; set; } = true;

    // Only the thumbnail's width is needed (Keeps the original content's aspect ratio for clarity)
    public int ThumbnailWidth { get; set; } = 200;

    [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
    public IImageFormat ThumbnailFormat { get; set; } = WebpFormat.Instance;

    public string[] ImageTypes { get; set; } =
    {
        ".png", ".tif", ".tiff", ".bmp", ".jpg", ".jpeg", ".gif", ".webp"
    };

    public string[] VideoTypes { get; set; } =
    {
        ".mp4", ".avi", ".mkv", ".mov", ".webm", ".wmv"
    };

    // If you want to accept all types of content set null
    public string[]? OtherTypes { get; set; } =
    {
        ".txt"
    };

    /// <summary>
    /// ATM it only works with image forms of content because the ImageSharp associated libraries cannot add text to video.
    /// Kinda unstable because of dynamic scaling (may look worse or better on different images)
    /// Lato font needs to be installed on the system for it to work.
    /// </summary>
    public string Watermark { get; set; } = String.Empty;
    
    /// <summary>
    /// Falls back to request provided file extension in the case MimeDetective does not work
    /// </summary>
    public bool FallBackToFileExtension { get; set; } = true;

    // Note: since this uses TrID file signatures' you can only use it personally or for non commercial, you'll have you buy a commercial license if not
    public bool UseMimeDetective { get; set; } = true;

    public bool LosslessCompression { get; set; } = true;
    public LZ4Level LosslessCompressionLevel { get; set; } = LZ4Level.L12_MAX;

    public bool Caching { get; set; } = true;
}