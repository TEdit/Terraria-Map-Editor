using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TEdit.Common;
using TEdit.Common.IO;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Loads and saves .twld (tModLoader world) sidecar files.
/// Parses the GZip-compressed NBT format and extracts mod tile/wall/item data.
/// </summary>
public static class TwldFile
{
    /// <summary>
    /// Loads a .twld file associated with the given .wld path.
    /// </summary>
    public static TwldData Load(string wldPath)
    {
        string twldPath = GetTwldPath(wldPath);
        if (!File.Exists(twldPath))
            return null;

        var rootTag = TagIO.FromFile(twldPath);
        return ParseRootTag(rootTag);
    }

    /// <summary>
    /// Saves a .twld file associated with the given .wld path.
    /// </summary>
    public static void Save(string wldPath, TwldData data)
    {
        if (data == null) return;

        string twldPath = GetTwldPath(wldPath);
        var rootTag = BuildRootTag(data);
        TagIO.ToFile(rootTag, twldPath);
    }

    /// <summary>
    /// Applies mod tile/wall overlays from TwldData onto the World's tile grid.
    /// Assigns virtual IDs beyond vanilla range and registers dynamic TileProperty/WallProperty entries.
    /// </summary>
    public static void ApplyToWorld(World world, TwldData data)
    {
        if (data == null) return;

        // Parse binary tile/wall data now that we have world dimensions
        if (!data.TileWallDataParsed && data.RawTileWallData.Length > 0)
        {
            ParseTileWallBinary(data.RawTileWallData, data, world.TilesWide, world.TilesHigh);
            data.TileWallDataParsed = true;
        }

        // Try to load scraped mod colors for better rendering
        var modColors = LoadModColorsFromDefaultPaths();

        ushort virtualTileBase = (ushort)WorldConfiguration.TileCount;
        ushort virtualWallBase = (ushort)WorldConfiguration.WallCount;

        // Assign virtual IDs and register properties for mod tiles
        for (int i = 0; i < data.TileMap.Count; i++)
        {
            var entry = data.TileMap[i];
            ushort virtualId = (ushort)(virtualTileBase + i);
            data.VirtualTileIdToMapIndex[virtualId] = i;
            data.MapIndexToVirtualTileId[i] = virtualId;

            TEditColor color;
            if (modColors.TryGetValue(entry.FullName, out var scraped))
                color = scraped;
            else
                color = GenerateModColor(entry.FullName);

            RegisterModTileProperty(virtualId, entry.FullName, color, entry.FrameImportant);
        }

        // Assign virtual IDs and register properties for mod walls
        for (int i = 0; i < data.WallMap.Count; i++)
        {
            var entry = data.WallMap[i];
            ushort virtualId = (ushort)(virtualWallBase + i);
            data.VirtualWallIdToMapIndex[virtualId] = i;
            data.MapIndexToVirtualWallId[i] = virtualId;

            TEditColor color;
            if (modColors.TryGetValue(entry.FullName, out var scraped))
                color = scraped;
            else
                color = GenerateModColor(entry.FullName);

            RegisterModWallProperty(virtualId, entry.FullName, color);
        }

        // Overlay mod tiles onto the world grid
        foreach (var (pos, modTile) in data.ModTileGrid)
        {
            if (pos.X < 0 || pos.X >= world.TilesWide || pos.Y < 0 || pos.Y >= world.TilesHigh)
                continue;

            var tile = world.Tiles[pos.X, pos.Y];
            if (data.MapIndexToVirtualTileId.TryGetValue(modTile.TileMapIndex, out ushort vTileId))
            {
                tile.IsActive = true;
                tile.Type = vTileId;
                tile.TileColor = modTile.Color;
                if (data.TileMap[modTile.TileMapIndex].FrameImportant)
                {
                    tile.U = modTile.FrameX;
                    tile.V = modTile.FrameY;
                }
            }
        }

        // Overlay mod walls onto the world grid
        foreach (var (pos, modWall) in data.ModWallGrid)
        {
            if (pos.X < 0 || pos.X >= world.TilesWide || pos.Y < 0 || pos.Y >= world.TilesHigh)
                continue;

            var tile = world.Tiles[pos.X, pos.Y];
            if (data.MapIndexToVirtualWallId.TryGetValue(modWall.WallMapIndex, out ushort vWallId))
            {
                tile.Wall = vWallId;
                tile.WallColor = modWall.WallColor;
            }
        }
    }

    /// <summary>
    /// Strips mod tile/wall data from the World's tile grid back into TwldData
    /// before saving the vanilla .wld. Reverses ApplyToWorld.
    /// </summary>
    public static void StripFromWorld(World world, TwldData data)
    {
        if (data == null) return;

        ushort virtualTileBase = (ushort)WorldConfiguration.TileCount;
        ushort virtualWallBase = (ushort)WorldConfiguration.WallCount;

        data.ModTileGrid.Clear();
        data.ModWallGrid.Clear();

        for (int x = 0; x < world.TilesWide; x++)
        {
            for (int y = 0; y < world.TilesHigh; y++)
            {
                var tile = world.Tiles[x, y];

                if (tile.IsActive && tile.Type >= virtualTileBase &&
                    data.VirtualTileIdToMapIndex.TryGetValue(tile.Type, out int tileMapIdx))
                {
                    data.ModTileGrid[(x, y)] = new ModTileData
                    {
                        TileMapIndex = (ushort)tileMapIdx,
                        Color = tile.TileColor,
                        FrameX = tile.U,
                        FrameY = tile.V,
                    };

                    tile.IsActive = false;
                    tile.Type = 0;
                    tile.TileColor = 0;
                    tile.U = 0;
                    tile.V = 0;
                }

                if (tile.Wall >= virtualWallBase &&
                    data.VirtualWallIdToMapIndex.TryGetValue(tile.Wall, out int wallMapIdx))
                {
                    data.ModWallGrid[(x, y)] = new ModWallData
                    {
                        WallMapIndex = (ushort)wallMapIdx,
                        WallColor = tile.WallColor,
                    };

                    tile.Wall = 0;
                    tile.WallColor = 0;
                }
            }
        }

        // Rebuild the raw binary data for saving
        data.RawTileWallData = BuildTileWallBinary(data, world.TilesWide, world.TilesHigh);
    }

    /// <summary>
    /// Re-applies mod tile/wall data after saving the vanilla .wld.
    /// </summary>
    public static void ReapplyToWorld(World world, TwldData data)
    {
        if (data == null) return;

        foreach (var (pos, modTile) in data.ModTileGrid)
        {
            if (pos.X < 0 || pos.X >= world.TilesWide || pos.Y < 0 || pos.Y >= world.TilesHigh)
                continue;

            var tile = world.Tiles[pos.X, pos.Y];
            if (data.MapIndexToVirtualTileId.TryGetValue(modTile.TileMapIndex, out ushort vTileId))
            {
                tile.IsActive = true;
                tile.Type = vTileId;
                tile.TileColor = modTile.Color;
                if (data.TileMap[modTile.TileMapIndex].FrameImportant)
                {
                    tile.U = modTile.FrameX;
                    tile.V = modTile.FrameY;
                }
            }
        }

        foreach (var (pos, modWall) in data.ModWallGrid)
        {
            if (pos.X < 0 || pos.X >= world.TilesWide || pos.Y < 0 || pos.Y >= world.TilesHigh)
                continue;

            var tile = world.Tiles[pos.X, pos.Y];
            if (data.MapIndexToVirtualWallId.TryGetValue(modWall.WallMapIndex, out ushort vWallId))
            {
                tile.Wall = vWallId;
                tile.WallColor = modWall.WallColor;
            }
        }
    }

    #region Tag Parsing

    private static TwldData ParseRootTag(TagCompound rootTag)
    {
        var data = new TwldData { RawTag = rootTag };

        if (rootTag.ContainsKey("0header"))
            data.Header = rootTag.GetCompound("0header");

        if (rootTag.ContainsKey("tiles"))
        {
            var tilesTag = rootTag.GetCompound("tiles");
            ParseTilesSection(tilesTag, data);
        }

        if (rootTag.ContainsKey("containers"))
            data.ContainerData = rootTag.GetCompound("containers");

        if (rootTag.ContainsKey("chests"))
            data.ModChestItems = rootTag.GetList<TagCompound>("chests");

        if (rootTag.ContainsKey("npcs"))
            data.ModNpcs = rootTag.GetList<TagCompound>("npcs");

        if (rootTag.ContainsKey("tileEntities"))
            data.ModTileEntities = rootTag.GetList<TagCompound>("tileEntities");

        return data;
    }

    private static void ParseTilesSection(TagCompound tilesTag, TwldData data)
    {
        var tileMapTags = tilesTag.GetList<TagCompound>("tileMap");
        for (int i = 0; i < tileMapTags.Count; i++)
            data.TileMap.Add(ModTileEntry.FromTag(tileMapTags[i], i));

        var wallMapTags = tilesTag.GetList<TagCompound>("wallMap");
        for (int i = 0; i < wallMapTags.Count; i++)
            data.WallMap.Add(ModWallEntry.FromTag(wallMapTags[i], i));

        // Store raw binary data — defer parsing until world dimensions are known
        if (tilesTag.ContainsKey("data"))
        {
            byte[] binaryData = tilesTag.GetByteArray("data");
            if (binaryData != null)
                data.RawTileWallData = binaryData;
        }
    }

    /// <summary>
    /// Parses the interleaved binary tile/wall data stream from .twld.
    /// Format uses flags-based encoding with skip counts, column-major (x outer, y inner).
    /// </summary>
    internal static void ParseTileWallBinary(byte[] binaryData, TwldData data, int tilesWide, int tilesHigh)
    {
        bool[] frameImportant = new bool[Math.Max(data.TileMap.Count, 1)];
        for (int i = 0; i < data.TileMap.Count; i++)
            frameImportant[i] = data.TileMap[i].FrameImportant;

        using var ms = new MemoryStream(binaryData);
        using var r = new BinaryReader(ms);

        // Position tracked as linear index: pos = x * tilesHigh + y
        int linearPos = 0;
        int totalCells = tilesWide * tilesHigh;

        while (ms.Position < ms.Length && linearPos < totalCells)
        {
            byte flags = r.ReadByte();

            bool hasTile = (flags & 0x01) != 0;
            bool frameXLarge = (flags & 0x02) != 0;
            bool frameYLarge = (flags & 0x04) != 0;
            bool hasColor = (flags & 0x08) != 0;
            bool hasWall = (flags & 0x10) != 0;
            bool hasWallColor = (flags & 0x20) != 0;
            bool hasSameCount = (flags & 0x40) != 0;
            bool nextIsModTile = (flags & 0x80) != 0;

            ModTileData? tileData = null;
            ModWallData? wallData = null;

            if (hasTile)
            {
                ushort tileType = r.ReadUInt16();
                short frameX = 0, frameY = 0;

                if (tileType < frameImportant.Length && frameImportant[tileType])
                {
                    frameX = frameXLarge ? r.ReadInt16() : (short)r.ReadByte();
                    frameY = frameYLarge ? r.ReadInt16() : (short)r.ReadByte();
                }

                byte color = hasColor ? r.ReadByte() : (byte)0;

                tileData = new ModTileData
                {
                    TileMapIndex = tileType,
                    Color = color,
                    FrameX = frameX,
                    FrameY = frameY,
                };
            }

            if (hasWall)
            {
                ushort wallType = r.ReadUInt16();
                byte wallColor = hasWallColor ? r.ReadByte() : (byte)0;

                wallData = new ModWallData
                {
                    WallMapIndex = wallType,
                    WallColor = wallColor,
                };
            }

            int sameCount = hasSameCount ? r.ReadByte() : 0;

            // Apply this cell and any repeated (same) cells
            for (int repeat = 0; repeat <= sameCount; repeat++)
            {
                if (linearPos < totalCells)
                {
                    int x = linearPos / tilesHigh;
                    int y = linearPos % tilesHigh;

                    if (tileData.HasValue)
                        data.ModTileGrid[(x, y)] = tileData.Value;
                    if (wallData.HasValue)
                        data.ModWallGrid[(x, y)] = wallData.Value;
                }
                linearPos++;
            }

            // If next byte is NOT a mod tile, read skip count(s)
            if (!nextIsModTile)
            {
                // Read skip bytes (each byte up to 255; if byte == 255, read another)
                while (ms.Position < ms.Length)
                {
                    byte skipByte = r.ReadByte();
                    linearPos += skipByte;
                    if (skipByte < 255)
                        break;
                }
            }
        }
    }

    #endregion

    #region Tag Building (Save)

    private static TagCompound BuildRootTag(TwldData data)
    {
        // Start with the raw tag to preserve all unmodified sections
        var rootTag = data.RawTag ?? new TagCompound();

        // Rebuild tiles section
        rootTag.Set("tiles", BuildTilesSection(data));

        if (data.Header.Count > 0)
            rootTag.Set("0header", data.Header);

        if (data.ContainerData.Count > 0)
            rootTag.Set("containers", data.ContainerData);

        if (data.ModChestItems.Count > 0)
            rootTag.Set("chests", data.ModChestItems);

        if (data.ModNpcs.Count > 0)
            rootTag.Set("npcs", data.ModNpcs);

        if (data.ModTileEntities.Count > 0)
            rootTag.Set("tileEntities", data.ModTileEntities);

        return rootTag;
    }

    private static TagCompound BuildTilesSection(TwldData data)
    {
        var tilesTag = new TagCompound();

        var tileMapTags = new List<TagCompound>();
        foreach (var entry in data.TileMap)
            tileMapTags.Add(entry.ToTag());
        tilesTag.Set("tileMap", tileMapTags);

        var wallMapTags = new List<TagCompound>();
        foreach (var entry in data.WallMap)
            wallMapTags.Add(entry.ToTag());
        tilesTag.Set("wallMap", wallMapTags);

        // Use the rebuilt binary data if available, otherwise use raw
        tilesTag.Set("data", data.RawTileWallData ?? Array.Empty<byte>());

        return tilesTag;
    }

    /// <summary>
    /// Rebuilds the interleaved binary tile/wall data for saving.
    /// </summary>
    internal static byte[] BuildTileWallBinary(TwldData data, int tilesWide, int tilesHigh)
    {
        if (data.ModTileGrid.Count == 0 && data.ModWallGrid.Count == 0)
            return Array.Empty<byte>();

        // Build a set of all linear positions with mod data
        var modPositions = new SortedSet<int>();
        foreach (var (pos, _) in data.ModTileGrid)
            modPositions.Add(pos.X * tilesHigh + pos.Y);
        foreach (var (pos, _) in data.ModWallGrid)
            modPositions.Add(pos.X * tilesHigh + pos.Y);

        bool[] frameImportant = new bool[Math.Max(data.TileMap.Count, 1)];
        for (int i = 0; i < data.TileMap.Count; i++)
            frameImportant[i] = data.TileMap[i].FrameImportant;

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        var posArray = new List<int>(modPositions);
        int prevEndPos = 0; // position after the last written cell

        for (int idx = 0; idx < posArray.Count; idx++)
        {
            int linearPos = posArray[idx];
            int x = linearPos / tilesHigh;
            int y = linearPos % tilesHigh;

            bool hasTileData = data.ModTileGrid.TryGetValue((x, y), out var tileInfo);
            bool hasWallData = data.ModWallGrid.TryGetValue((x, y), out var wallInfo);

            // Determine if next position is also mod (adjacent in linear order)
            bool nextIsMod = idx + 1 < posArray.Count && posArray[idx + 1] == linearPos + 1;

            // Check for "same" (repeat) count — consecutive cells with identical data
            int sameCount = 0;
            if (!nextIsMod)
            {
                // No same-count optimization for non-adjacent cells
            }
            else
            {
                // Count how many consecutive cells have the exact same tile+wall data
                while (idx + 1 + sameCount < posArray.Count)
                {
                    int nextLinear = posArray[idx + 1 + sameCount];
                    if (nextLinear != linearPos + 1 + sameCount)
                        break;

                    int nx = nextLinear / tilesHigh;
                    int ny = nextLinear % tilesHigh;
                    bool nextHasTile = data.ModTileGrid.TryGetValue((nx, ny), out var nextTile);
                    bool nextHasWall = data.ModWallGrid.TryGetValue((nx, ny), out var nextWall);

                    if (nextHasTile != hasTileData || nextHasWall != hasWallData)
                        break;
                    if (hasTileData && !nextTile.Equals(tileInfo))
                        break;
                    if (hasWallData && !nextWall.Equals(wallInfo))
                        break;

                    sameCount++;
                    if (sameCount >= 255)
                        break;
                }
            }

            // Write skip count from previous position
            int skip = linearPos - prevEndPos;
            if (skip > 0 || (idx == 0 && linearPos > 0))
            {
                // If this is NOT the first entry, the previous entry's "nextIsMod" flag was false,
                // so we already have a context where skip bytes are expected.
                // For the first entry, we need special handling.
                if (idx == 0)
                {
                    // Before the first mod entry, write skip bytes
                    WriteSkipBytes(w, linearPos);
                }
                else
                {
                    WriteSkipBytes(w, skip);
                }
            }

            // Recalculate nextIsMod accounting for sameCount
            int endPos = linearPos + sameCount;
            bool nextAfterSameIsMod = idx + 1 + sameCount < posArray.Count &&
                                       posArray[idx + 1 + sameCount] == endPos + 1;

            // Build flags byte
            byte flags = 0;
            if (hasTileData) flags |= 0x01;
            if (hasWallData) flags |= 0x10;

            bool isFrameImportant = hasTileData && tileInfo.TileMapIndex < frameImportant.Length &&
                                    frameImportant[tileInfo.TileMapIndex];

            if (hasTileData && isFrameImportant)
            {
                if (tileInfo.FrameX >= 256) flags |= 0x02;
                if (tileInfo.FrameY >= 256) flags |= 0x04;
            }
            if (hasTileData && tileInfo.Color != 0) flags |= 0x08;
            if (hasWallData && wallInfo.WallColor != 0) flags |= 0x20;
            if (sameCount > 0) flags |= 0x40;
            if (nextAfterSameIsMod) flags |= 0x80;

            w.Write(flags);

            if (hasTileData)
            {
                w.Write(tileInfo.TileMapIndex);
                if (isFrameImportant)
                {
                    if (tileInfo.FrameX >= 256) w.Write(tileInfo.FrameX);
                    else w.Write((byte)tileInfo.FrameX);
                    if (tileInfo.FrameY >= 256) w.Write(tileInfo.FrameY);
                    else w.Write((byte)tileInfo.FrameY);
                }
                if (tileInfo.Color != 0) w.Write(tileInfo.Color);
            }

            if (hasWallData)
            {
                w.Write(wallInfo.WallMapIndex);
                if (wallInfo.WallColor != 0) w.Write(wallInfo.WallColor);
            }

            if (sameCount > 0)
                w.Write((byte)sameCount);

            prevEndPos = endPos + 1;

            // Skip over the same-counted entries
            idx += sameCount;

            // If next is not mod, the reader will expect skip bytes.
            // They'll be written at the start of the next iteration.
        }

        return ms.ToArray();
    }

    private static void WriteSkipBytes(BinaryWriter w, int skip)
    {
        while (skip >= 255)
        {
            w.Write((byte)255);
            skip -= 255;
        }
        w.Write((byte)skip);
    }

    #endregion

    #region Mod Colors

    private static Dictionary<string, TEditColor> _cachedModColors;

    /// <summary>
    /// Loads mod colors from default search paths (next to executable, app data).
    /// Results are cached after first load.
    /// </summary>
    private static Dictionary<string, TEditColor> LoadModColorsFromDefaultPaths()
    {
        if (_cachedModColors != null)
            return _cachedModColors;

        // Search paths for modColors.json
        string[] searchPaths =
        {
            Path.Combine(AppContext.BaseDirectory, "modColors.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TEdit", "modColors.json"),
        };

        foreach (var path in searchPaths)
        {
            var colors = LoadModColors(path);
            if (colors.Count > 0)
            {
                _cachedModColors = colors;
                return colors;
            }
        }

        _cachedModColors = new Dictionary<string, TEditColor>();
        return _cachedModColors;
    }

    /// <summary>
    /// Loads modColors.json from a specific path and returns a flat dictionary
    /// mapping "ModName:TileName" → TEditColor for both tiles and walls.
    /// Returns empty dictionary if file doesn't exist or fails to parse.
    /// </summary>
    internal static Dictionary<string, TEditColor> LoadModColors(string path)
    {
        var result = new Dictionary<string, TEditColor>();

        if (!File.Exists(path))
            return result;

        try
        {
            string json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);

            foreach (var modProp in doc.RootElement.EnumerateObject())
            {
                string modName = modProp.Name;

                if (modProp.Value.TryGetProperty("tiles", out var tilesObj))
                {
                    foreach (var tileProp in tilesObj.EnumerateObject())
                    {
                        string tileName = tileProp.Name;
                        if (tileProp.Value.TryGetProperty("color", out var colorEl))
                        {
                            var color = ParseHexColor(colorEl.GetString());
                            if (color.HasValue)
                                result[$"{modName}:{tileName}"] = color.Value;
                        }
                    }
                }

                if (modProp.Value.TryGetProperty("walls", out var wallsObj))
                {
                    foreach (var wallProp in wallsObj.EnumerateObject())
                    {
                        string wallName = wallProp.Name;
                        if (wallProp.Value.TryGetProperty("color", out var colorEl))
                        {
                            var color = ParseHexColor(colorEl.GetString());
                            if (color.HasValue)
                                result[$"{modName}:{wallName}"] = color.Value;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load modColors.json from {path}: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Parses a hex color string in #RRGGBBAA format.
    /// </summary>
    private static TEditColor? ParseHexColor(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 7 || hex[0] != '#')
            return null;

        try
        {
            byte r = Convert.ToByte(hex.Substring(1, 2), 16);
            byte g = Convert.ToByte(hex.Substring(3, 2), 16);
            byte b = Convert.ToByte(hex.Substring(5, 2), 16);
            byte a = hex.Length >= 9 ? Convert.ToByte(hex.Substring(7, 2), 16) : (byte)255;
            return new TEditColor(r, g, b, a);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clears cached mod colors (useful when modColors.json is updated).
    /// </summary>
    public static void ClearModColorCache()
    {
        _cachedModColors = null;
    }

    #endregion

    #region Helpers

    internal static string GetTwldPath(string wldPath)
    {
        return Path.Combine(
            Path.GetDirectoryName(wldPath),
            Path.GetFileNameWithoutExtension(wldPath) + ".twld");
    }

    /// <summary>
    /// Generates a stable color from a mod:name string using a hash.
    /// Different mod tiles will get visually distinct placeholder colors.
    /// </summary>
    internal static TEditColor GenerateModColor(string fullName)
    {
        // Use a simple but stable hash for color generation
        uint hash = 0;
        foreach (char c in fullName)
        {
            hash = hash * 31 + c;
        }
        // Ensure moderate brightness by clamping components to 64-223 range
        byte r = (byte)(64 + (hash % 160));
        byte g = (byte)(64 + ((hash >> 8) % 160));
        byte b = (byte)(64 + ((hash >> 16) % 160));
        return new TEditColor(r, g, b, (byte)255);
    }

    private static void RegisterModTileProperty(ushort virtualId, string displayName, TEditColor color, bool isFramed)
    {
        while (WorldConfiguration.TileProperties.Count <= virtualId)
        {
            WorldConfiguration.TileProperties.Add(new TileProperty
            {
                Id = WorldConfiguration.TileProperties.Count,
                Name = "UNKNOWN",
                Color = TEditColor.Magenta,
                IsFramed = true,
            });
        }

        WorldConfiguration.TileProperties[virtualId] = new TileProperty
        {
            Id = virtualId,
            Name = displayName,
            Color = color,
            IsFramed = isFramed,
            IsSolid = !isFramed,
        };
    }

    private static void RegisterModWallProperty(ushort virtualId, string displayName, TEditColor color)
    {
        while (WorldConfiguration.WallProperties.Count <= virtualId)
        {
            WorldConfiguration.WallProperties.Add(new WallProperty
            {
                Id = WorldConfiguration.WallProperties.Count,
                Name = "UNKNOWN",
                Color = TEditColor.Magenta,
            });
        }

        WorldConfiguration.WallProperties[virtualId] = new WallProperty
        {
            Id = virtualId,
            Name = displayName,
            Color = color,
        };
    }

    #endregion
}
