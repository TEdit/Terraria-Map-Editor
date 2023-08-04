using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TEdit.Configuration;

public class SaveVersionManager
{
    public Dictionary<string, uint> GameVersionToSaveVersion { get; set; }

    public Dictionary<int, SaveVersionData> SaveVersions { get; set; }

    public int GetMaxVersion() => SaveVersions.Keys.Max();

    public SaveVersionData GetData(uint version) => GetData((int)version);

    public SaveVersionData GetData(int version)
    {
        int useVersion = (version > GetMaxVersion()) ? GetMaxVersion() : version;
        if (SaveVersions.TryGetValue(useVersion, out var data))
        {
            return data;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(version), $"Missing settings for world file version: {version}");
        }
    }

    /// <summary>
    /// Get a <see cref="bool"/> array of framed tiles (sprites) for saving to world header.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Throws when configuration data is not found.</exception>
    public bool[] GetTileFramesForVersion(int version)
    {
        if (SaveVersions.TryGetValue(version, out var data))
        {
            return data.GetFrames();
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(version), version, $"Error saving world version {version}: save configuration not found.");
        }
    }

    public static SaveVersionManager LoadJson(string fileName)
    {
        using (StreamReader file = File.OpenText(fileName))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<SaveVersionManager>(reader);
        }
    }
}
