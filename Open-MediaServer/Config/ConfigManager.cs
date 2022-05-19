using System;
using System.IO;
using System.Text.Json;

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
            var deserializedConfig = JsonSerializer.Deserialize<Config>(fileData);
            if (deserializedConfig == null)
            {
                Console.WriteLine($"Failed to deserialize config ({file})");
                return;
            }

            Config = deserializedConfig;
            return;
        }

        string serializedJson = JsonSerializer.Serialize(Config, new JsonSerializerOptions()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });
        File.WriteAllText(file, serializedJson);
    }
}