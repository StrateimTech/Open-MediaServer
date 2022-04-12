namespace Open_MediaServer.Config;

public class Config
{
    // Length of generated uid 
    public int UniqueIdLength { get; set; } = 8;
    
    // This value is in MBs (Can be null for no limit)
    // Maximum network file upload limit
    public int? FileNetworkUploadMax { get; set; } = null;
    
    // Developer tools
    public bool ShowSwaggerUi { get; set; } = true;
}