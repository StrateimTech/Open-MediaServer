namespace Open_MediaServer.Analytics;

public struct Analytics
{
    public string hostname { get; set; }
    public string ip_address { get; set; }
    public string path { get; set; }
    public string user_agent { get; set; }
    public string method { get; set; }
    public int  response_time { get; set; }
    public int status { get; set; }
    public string created_at { get; set; }
}