using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.DataModel;

public class MorphConfiguration
{
    public Dictionary<string, MorphBiomeData> Biomes { get; set; } = new();
    public Dictionary<string, int> MossTypes { get; set; } = new();

    /// <summary>
    /// Static set of tile IDs that are not solid (passable tiles like platforms, furniture, etc.).
    /// Populated by WorldConfiguration after loading tile properties.
    /// </summary>
    public static HashSet<ushort> NotSolidTiles { get; } = new();

    [System.Text.Json.Serialization.JsonIgnore]
    private readonly HashSet<int> _mossTypes = [];

    public bool IsMoss(ushort type) => _mossTypes.Contains(type);

    public void InitCache()
    {
        _mossTypes.Clear();
        foreach (var id in MossTypes.Values)
        {
            _mossTypes.Add(id);
        }
    }

    public static MorphConfiguration Load(Stream stream)
    {
        var config = JsonSerializer.Deserialize<MorphConfiguration>(stream, TEditJsonSerializer.DefaultOptions)
            ?? throw new InvalidOperationException("Failed to deserialize morph configuration.");
        config.InitCache();
        return config;
    }

    public static MorphConfiguration LoadFile(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        return Load(stream);
    }

    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);
        JsonSerializer.Serialize(stream, this, TEditJsonSerializer.DefaultOptions);
    }
}
