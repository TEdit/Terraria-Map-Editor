using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class MetadataApi
{
    public string TileName(int id)
    {
        if (id >= 0 && id < WorldConfiguration.TileProperties.Count)
            return WorldConfiguration.TileProperties[id].Name ?? "";
        return "";
    }

    public string WallName(int id)
    {
        if (id >= 0 && id < WorldConfiguration.WallProperties.Count)
            return WorldConfiguration.WallProperties[id].Name ?? "";
        return "";
    }

    public string ItemName(int id)
    {
        var item = WorldConfiguration.ItemProperties.FirstOrDefault(i => i.Id == id);
        return item?.Name ?? "";
    }

    public int TileId(string name)
    {
        var lower = name.ToLowerInvariant();
        var prop = WorldConfiguration.TileProperties
            .FirstOrDefault(p => p.Name?.ToLowerInvariant() == lower);
        return prop != null ? prop.Id : -1;
    }

    public int WallId(string name)
    {
        var lower = name.ToLowerInvariant();
        var prop = WorldConfiguration.WallProperties
            .FirstOrDefault(p => p.Name?.ToLowerInvariant() == lower);
        return prop != null ? prop.Id : -1;
    }

    public int ItemId(string name)
    {
        var lower = name.ToLowerInvariant();
        var prop = WorldConfiguration.ItemProperties
            .FirstOrDefault(p => p.Name?.ToLowerInvariant() == lower);
        return prop?.Id ?? -1;
    }

    public List<Dictionary<string, object>> AllTiles()
    {
        return WorldConfiguration.TileProperties
            .Select(p => new Dictionary<string, object>
            {
                { "id", p.Id },
                { "name", p.Name ?? "" }
            }).ToList();
    }

    public List<Dictionary<string, object>> AllWalls()
    {
        return WorldConfiguration.WallProperties
            .Select(p => new Dictionary<string, object>
            {
                { "id", p.Id },
                { "name", p.Name ?? "" }
            }).ToList();
    }

    public List<Dictionary<string, object>> AllItems()
    {
        return WorldConfiguration.ItemProperties
            .Select(p => new Dictionary<string, object>
            {
                { "id", p.Id },
                { "name", p.Name ?? "" }
            }).ToList();
    }
}
