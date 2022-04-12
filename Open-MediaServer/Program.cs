using System;
using Microsoft.Extensions.Configuration;
using Open_MediaServer.Backend;
using Open_MediaServer.Config;
using Open_MediaServer.Database;
using Open_MediaServer.Frontend;

namespace Open_MediaServer;
public class Program
{
    public static ConfigManager ConfigManager;
    public static BackendServer Backend;
    public static FrontendServer Frontend;
    public static SqLite Database;
    
    public static void Main(string[] args)
    {
        ConfigManager = new ConfigManager(Environment.CurrentDirectory);
        
        Backend = new BackendServer();
        // Frontend = new FrontendServer();
    }
}