using System.Collections.Generic;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.Loaders;

public static class TileDataLoader
{
    /// <summary>
    /// Derive chest data from tile frames for tiles that are chest types.
    /// </summary>
    public static List<ChestProperty> DeriveChests(List<TileProperty> tiles)
    {
        var chests = new List<ChestProperty>();
        int chestId = 0;

        foreach (var tile in tiles)
        {
            if (!TileTypes.IsChest(tile.Id)) continue;
            if (tile.Frames == null) continue;

            foreach (var frame in tile.Frames)
            {
                var chest = new ChestProperty
                {
                    ChestId = chestId++,
                    Name = FormatChestName(tile.Name, frame.Name, frame.Variety),
                    UV = frame.UV,
                    Size = frame.Size,
                    TileType = (ushort)tile.Id,
                };
                chests.Add(chest);
            }
        }

        return chests;
    }

    /// <summary>
    /// Derive sign data from tile frames for tiles that are sign types.
    /// </summary>
    public static List<SignProperty> DeriveSigns(List<TileProperty> tiles)
    {
        var signs = new List<SignProperty>();
        int signId = 0;

        foreach (var tile in tiles)
        {
            if (!TileTypes.IsSign(tile.Id)) continue;
            if (tile.Frames == null) continue;

            foreach (var frame in tile.Frames)
            {
                var sign = new SignProperty
                {
                    SignId = signId++,
                    Name = $"{tile.Name} {frame.Variety} {frame.Anchor}",
                    UV = frame.UV,
                    TileType = (ushort)tile.Id,
                };
                signs.Add(sign);
            }
        }

        return signs;
    }

    private static string FormatChestName(string tileName, string frameName, string? variety)
    {
        if (variety == null) return frameName;

        if (tileName == "Dresser")
            return $"{variety} Dresser";

        return $"{frameName} {variety}";
    }
}
