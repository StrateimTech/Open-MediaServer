using System;
using System.IO;
using Newtonsoft.Json;

namespace Open_MediaServer.Config;

public class ConfigManager
{
    public readonly Config Config = new();

    public ConfigManager(string path)
    {
        string file = $"{path}/config.json";
        if (File.Exists(file))
        {
            string fileData = File.ReadAllText(file);
            var deserializedConfig = JsonConvert.DeserializeObject<Config>(fileData);
            if (deserializedConfig == null)
            {
                Console.WriteLine($"Failed to deserialize config ({file})");
                return;
            }

            Config = deserializedConfig;
            return;
        }

        string serializedJson = JsonConvert.SerializeObject(Config, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include
        });
        File.WriteAllText(file, serializedJson);
    }
}