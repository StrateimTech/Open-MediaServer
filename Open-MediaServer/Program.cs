using System;
using System.Threading;
using Open_MediaServer.Backend;
using Open_MediaServer.Config;
using Open_MediaServer.Content;
using Open_MediaServer.Database;
using Open_MediaServer.Frontend;

namespace Open_MediaServer;

public class Program
{
    public static ConfigManager ConfigManager;
    private static BackendServer _backend;
    private static FrontendServer _frontend;
    public static SqDatabase Database;
    public static ContentManager ContentManager;

    public static void Main(string[] args)
    {
        ConfigManager = new ConfigManager(Environment.CurrentDirectory);
        ContentManager = new ContentManager(ConfigManager.Config.WorkingDirectory ?? Environment.CurrentDirectory);
        Database = new SqDatabase(ConfigManager.Config.WorkingDirectory ?? Environment.CurrentDirectory);
        new Thread(() => _backend = new BackendServer()).Start();
        new Thread(() => _frontend = new FrontendServer()).Start();
    }
}