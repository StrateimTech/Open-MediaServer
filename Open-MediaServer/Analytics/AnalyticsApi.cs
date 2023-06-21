using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace Open_MediaServer.Analytics;

public class AnalyticsApi
{
    private readonly HttpClient _client;
    private readonly string _apikey; 
    
    private readonly List<Analytics> _analyticsCached = new();
    private readonly Stopwatch _flushWatch = new();
    
    public AnalyticsApi(string apiKey)
    {
        _flushWatch.Start();
        
        _apikey = apiKey;
        _client = new HttpClient(new SocketsHttpHandler());
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    }

    private struct AnalyticsPayload
    {
        public string api_key { get; set; }
        public Analytics[] requests { get; set; }
        public string framework { get; set; }
    }

    public void LogRequest(Analytics analytics)
    {
        _analyticsCached.Add(analytics);
        if (_flushWatch.Elapsed.TotalSeconds > 60)
        {
            AnalyticsPayload payload = new AnalyticsPayload
            {
                api_key = _apikey,
                requests = _analyticsCached.ToArray(),
                framework = "Rocket"
            };
            
            SendAnalytics(payload);
            
            _analyticsCached.Clear();
            _flushWatch.Restart();
        }
    }

    private void SendAnalytics(AnalyticsPayload analyticsPayload)
    {
        try
        {
            _client.PostAsJsonAsync("https://www.apianalytics-server.com/api/log-request", analyticsPayload);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}