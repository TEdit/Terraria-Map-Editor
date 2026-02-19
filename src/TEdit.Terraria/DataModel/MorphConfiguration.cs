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

    /// <summary>
    /// Column width for moss plant sprites (tile 184). Each moss type occupies one 22px column.
    /// </summary>
    public const short MossPlantColumnWidth = 22;

    [System.Text.Json.Serialization.JsonIgnore]
    private readonly HashSet<int> _mossTypes = [];

    [System.Text.Json.Serialization.JsonIgnore]
    private readonly Dictionary<int, int> _mossColumnIndex = new();

    public bool IsMoss(ushort type) => _mossTypes.Contains(type);

    /// <summary>
    /// Get the sprite column index (0-based) for a moss tile type.
    /// Returns -1 if the type is not a known moss.
    /// </summary>
    public int GetMossColumnIndex(int mossTileId)
    {
        return _mossColumnIndex.TryGetValue(mossTileId, out var index) ? index : -1;
    }

    public void InitCache()
    {
        _mossTypes.Clear();
        _mossColumnIndex.Clear();
        int column = 0;
        foreach (var id in MossTypes.Values)
        {
            _mossTypes.Add(id);
            _mossColumnIndex[id] = column++;
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
