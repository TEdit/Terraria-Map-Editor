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
        if (!data.TileWallDataParsed)
        {
            if (data.RawTileData.Length > 0 || data.RawWallData.Length > 0)
            {
                // New format: separate dense tileData/wallData streams
                if (data.RawTileData.Length > 0)
                    ParseTileDataDense(data.RawTileData, data, world.TilesWide, world.TilesHigh);
                if (data.RawWallData.Length > 0)
                    ParseWallDataDense(data.RawWallData, data, world.TilesWide, world.TilesHigh);
            }
            else if (data.RawTileWallData.Length > 0)
            {
                // Legacy format: interleaved skip+flags stream
                ParseTileWallBinary(data.RawTileWallData, data, world.TilesWide, world.TilesHigh);
            }
            data.TileWallDataParsed = true;
        }

        // Build virtual ID mappings (thread-safe, no UI collections touched)
        AssignVirtualIds(data);

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
    /// Assigns virtual IDs for mod tiles/walls without touching any UI-bound collections.
    /// Safe to call from any thread.
    /// </summary>
    private static void AssignVirtualIds(TwldData data)
    {
        ushort virtualTileBase = (ushort)WorldConfiguration.TileCount;
        ushort virtualWallBase = (ushort)WorldConfiguration.WallCount;

        for (int i = 0; i < data.TileMap.Count; i++)
        {
            ushort virtualId = (ushort)(virtualTileBase + i);
            data.VirtualTileIdToMapIndex[virtualId] = i;
            data.MapIndexToVirtualTileId[i] = virtualId;
        }

        for (int i = 0; i < data.WallMap.Count; i++)
        {
            ushort virtualId = (ushort)(virtualWallBase + i);
            data.VirtualWallIdToMapIndex[virtualId] = i;
            data.MapIndexToVirtualWallId[i] = virtualId;
        }
    }

    /// <summary>
    /// Registers mod tile/wall properties into WorldConfiguration's ObservableCollections.
    /// Must be called on the UI (dispatcher) thread since TileProperties/WallProperties are UI-bound.
    /// </summary>
    public static void RegisterModProperties(TwldData data)
    {
        if (data == null) return;

        // Ensure virtual IDs are assigned
        if (data.VirtualTileIdToMapIndex.Count == 0 && data.TileMap.Count > 0)
            AssignVirtualIds(data);

        var modColors = LoadModColorsFromDefaultPaths();

        for (int i = 0; i < data.TileMap.Count; i++)
        {
            var entry = data.TileMap[i];
            if (!data.MapIndexToVirtualTileId.TryGetValue(i, out ushort virtualId))
                continue;

            TEditColor color;
            if (modColors.TryGetValue(entry.FullName, out var scraped))
                color = scraped;
            else
                color = GenerateModColor(entry.FullName);

            RegisterModTileProperty(virtualId, entry.FullName, color, entry.FrameImportant);
        }

        for (int i = 0; i < data.WallMap.Count; i++)
        {
            var entry = data.WallMap[i];
            if (!data.MapIndexToVirtualWallId.TryGetValue(i, out ushort virtualId))
                continue;

            TEditColor color;
            if (modColors.TryGetValue(entry.FullName, out var scraped))
                color = scraped;
            else
                color = GenerateModColor(entry.FullName);

            RegisterModWallProperty(virtualId, entry.FullName, color);
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

        // Rebuild the raw binary data for saving, preserving the original format
        if (data.RawTileData.Length > 0 || data.RawWallData.Length > 0)
        {
            // New format: separate dense tileData/wallData
            data.RawTileData = BuildTileDataDense(data, world.TilesWide, world.TilesHigh);
            data.RawWallData = BuildWallDataDense(data, world.TilesWide, world.TilesHigh);
        }
        else
        {
            // Legacy format: interleaved binary
            data.RawTileWallData = BuildTileWallBinary(data, world.TilesWide, world.TilesHigh);
        }
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

        // Build SaveType → list index lookups from the "value" field
        for (int i = 0; i < data.TileMap.Count; i++)
            data.SaveTypeToTileMapIndex[data.TileMap[i].SaveType] = i;
        for (int i = 0; i < data.WallMap.Count; i++)
            data.SaveTypeToWallMapIndex[data.WallMap[i].SaveType] = i;

        // Store raw binary data — defer parsing until world dimensions are known
        // Newer tModLoader versions use separate "tileData" and "wallData" keys
        // Older versions use a single "data" key with interleaved tile/wall data
        if (tilesTag.ContainsKey("tileData"))
        {
            byte[] tileData = tilesTag.GetByteArray("tileData");
            if (tileData != null && tileData.Length > 0)
                data.RawTileData = tileData;
        }
        if (tilesTag.ContainsKey("wallData"))
        {
            byte[] wallData = tilesTag.GetByteArray("wallData");
            if (wallData != null && wallData.Length > 0)
                data.RawWallData = wallData;
        }
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
        using var ms = new MemoryStream(binaryData);
        using var r = new BinaryReader(ms);

        // Position tracked as linear index: pos = x * tilesHigh + y (column-major)
        int linearPos = 0;
        int totalCells = tilesWide * tilesHigh;

        // tModLoader format: [skip bytes] [record] [skip bytes if !nextMod] [record] ...
        // Each record is preceded by skip bytes (unless the previous record's nextIsModTile flag was set).
        bool expectSkip = true;

        while (ms.Position < ms.Length && linearPos < totalCells)
        {
            // Read skip bytes to advance past vanilla tiles
            if (expectSkip)
            {
                while (ms.Position < ms.Length)
                {
                    byte skipByte = r.ReadByte();
                    linearPos += skipByte;
                    if (skipByte < 255)
                        break;
                }

                if (ms.Position >= ms.Length || linearPos >= totalCells)
                    break;
            }

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
                ushort saveType = r.ReadUInt16();
                short frameX = 0, frameY = 0;

                // Look up the TileMap index from the saveType
                if (data.SaveTypeToTileMapIndex.TryGetValue(saveType, out int mapIndex))
                {
                    if (data.TileMap[mapIndex].FrameImportant)
                    {
                        frameX = frameXLarge ? r.ReadInt16() : (short)r.ReadByte();
                        frameY = frameYLarge ? r.ReadInt16() : (short)r.ReadByte();
                    }

                    byte color = hasColor ? r.ReadByte() : (byte)0;

                    tileData = new ModTileData
                    {
                        TileMapIndex = (ushort)mapIndex,
                        Color = color,
                        FrameX = frameX,
                        FrameY = frameY,
                    };
                }
                else
                {
                    // Unknown saveType — can't determine frameImportant so we can't reliably
                    // skip remaining bytes. Log and break to avoid stream desynchronization.
                    System.Diagnostics.Debug.WriteLine($"TwldFile: Unknown tile saveType {saveType} at pos {linearPos}, aborting legacy parse");
                    return;
                }
            }

            if (hasWall)
            {
                ushort saveType = r.ReadUInt16();
                byte wallColor = hasWallColor ? r.ReadByte() : (byte)0;

                // Look up the WallMap index from the saveType
                if (data.SaveTypeToWallMapIndex.TryGetValue(saveType, out int mapIndex))
                {
                    wallData = new ModWallData
                    {
                        WallMapIndex = (ushort)mapIndex,
                        WallColor = wallColor,
                    };
                }
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

            // If next record is a mod tile, skip bytes are NOT written between them
            expectSkip = !nextIsModTile;
        }
    }

    /// <summary>
    /// Parses the dense tileData stream (new tModLoader format).
    /// Format: for each (x, y) position in column-major order, a ushort saveType.
    /// saveType is a runtime type ID (from the "value" field in tileMap NBT entries).
    /// 0 = vanilla/empty. If saveType > 0: read color byte; if frameImportant, read frameX (short) and frameY (short).
    /// </summary>
    internal static void ParseTileDataDense(byte[] tileData, TwldData data, int tilesWide, int tilesHigh)
    {
        using var ms = new MemoryStream(tileData);
        using var r = new BinaryReader(ms);

        for (int x = 0; x < tilesWide; x++)
        {
            for (int y = 0; y < tilesHigh; y++)
            {
                if (ms.Position + 1 >= ms.Length) return;

                ushort saveType = r.ReadUInt16();
                if (saveType == 0) continue;

                // Look up the TileMap index from the saveType written in the binary stream
                if (!data.SaveTypeToTileMapIndex.TryGetValue(saveType, out int mapIndex))
                {
                    // Unknown saveType — skip the color byte and move on
                    // (can't determine frameImportant without a valid entry)
                    r.ReadByte();
                    continue;
                }

                byte color = r.ReadByte();
                short frameX = 0, frameY = 0;

                if (data.TileMap[mapIndex].FrameImportant)
                {
                    frameX = r.ReadInt16();
                    frameY = r.ReadInt16();
                }

                data.ModTileGrid[(x, y)] = new ModTileData
                {
                    TileMapIndex = (ushort)mapIndex,
                    Color = color,
                    FrameX = frameX,
                    FrameY = frameY,
                };
            }
        }
    }

    /// <summary>
    /// Parses the dense wallData stream (new tModLoader format).
    /// Format: for each (x, y) position in column-major order, a ushort saveType.
    /// saveType is a runtime type ID (from the "value" field in wallMap NBT entries).
    /// 0 = vanilla/empty. If saveType > 0: read wallColor byte.
    /// </summary>
    internal static void ParseWallDataDense(byte[] wallData, TwldData data, int tilesWide, int tilesHigh)
    {
        using var ms = new MemoryStream(wallData);
        using var r = new BinaryReader(ms);

        for (int x = 0; x < tilesWide; x++)
        {
            for (int y = 0; y < tilesHigh; y++)
            {
                if (ms.Position + 1 >= ms.Length) return;

                ushort saveType = r.ReadUInt16();
                if (saveType == 0) continue;

                // Look up the WallMap index from the saveType written in the binary stream
                if (!data.SaveTypeToWallMapIndex.TryGetValue(saveType, out int mapIndex))
                {
                    // Unknown saveType — skip the wallColor byte and move on
                    r.ReadByte();
                    continue;
                }

                byte wallColor = r.ReadByte();

                data.ModWallGrid[(x, y)] = new ModWallData
                {
                    WallMapIndex = (ushort)mapIndex,
                    WallColor = wallColor,
                };
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

        // Preserve format: new format uses separate tileData/wallData, legacy uses combined "data"
        if (data.RawTileData.Length > 0 || data.RawWallData.Length > 0)
        {
            tilesTag.Set("tileData", data.RawTileData);
            tilesTag.Set("wallData", data.RawWallData);
        }
        else
        {
            tilesTag.Set("data", data.RawTileWallData ?? Array.Empty<byte>());
        }

        return tilesTag;
    }

    /// <summary>
    /// Rebuilds the interleaved binary tile/wall data for saving.
    /// Format: [skip bytes] [record] [skip bytes if !nextMod] [record] ...
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
        bool writeSkip = true; // first entry always has skip bytes

        for (int idx = 0; idx < posArray.Count; idx++)
        {
            int linearPos = posArray[idx];
            int x = linearPos / tilesHigh;
            int y = linearPos % tilesHigh;

            bool hasTileData = data.ModTileGrid.TryGetValue((x, y), out var tileInfo);
            bool hasWallData = data.ModWallGrid.TryGetValue((x, y), out var wallInfo);

            // Check for "same" (repeat) count — consecutive cells with identical data
            int sameCount = 0;
            for (int s = 1; s <= 255 && idx + s < posArray.Count; s++)
            {
                int nextLinear = posArray[idx + s];
                if (nextLinear != linearPos + s)
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
            }

            // Write skip bytes before this record (tModLoader format)
            if (writeSkip)
            {
                int skip = linearPos - prevEndPos;
                WriteSkipBytes(w, skip);
            }

            // Determine nextIsModTile: is the position right after this run also a mod tile?
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
                // Write the original SaveType (runtime type ID), not the mapIndex
                ushort tileSaveType = tileInfo.TileMapIndex < data.TileMap.Count
                    ? data.TileMap[tileInfo.TileMapIndex].SaveType
                    : tileInfo.TileMapIndex;
                w.Write(tileSaveType);
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
                // Write the original SaveType (runtime type ID), not the mapIndex
                ushort wallSaveType = wallInfo.WallMapIndex < data.WallMap.Count
                    ? data.WallMap[wallInfo.WallMapIndex].SaveType
                    : wallInfo.WallMapIndex;
                w.Write(wallSaveType);
                if (wallInfo.WallColor != 0) w.Write(wallInfo.WallColor);
            }

            if (sameCount > 0)
                w.Write((byte)sameCount);

            prevEndPos = endPos + 1;
            idx += sameCount;

            // Next iteration: write skip if the next record is NOT immediately adjacent
            writeSkip = !nextAfterSameIsMod;
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

    /// <summary>
    /// Builds the dense tileData stream (new tModLoader format).
    /// Writes a ushort for every tile position; 0 = vanilla/empty.
    /// </summary>
    internal static byte[] BuildTileDataDense(TwldData data, int tilesWide, int tilesHigh)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        for (int x = 0; x < tilesWide; x++)
        {
            for (int y = 0; y < tilesHigh; y++)
            {
                if (data.ModTileGrid.TryGetValue((x, y), out var tile) &&
                    tile.TileMapIndex < data.TileMap.Count)
                {
                    // Write the original SaveType (runtime type ID) from the entry
                    w.Write(data.TileMap[tile.TileMapIndex].SaveType);
                    w.Write(tile.Color);
                    if (data.TileMap[tile.TileMapIndex].FrameImportant)
                    {
                        w.Write(tile.FrameX);
                        w.Write(tile.FrameY);
                    }
                }
                else
                {
                    w.Write((ushort)0);
                }
            }
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Builds the dense wallData stream (new tModLoader format).
    /// Writes a ushort for every tile position; 0 = vanilla/empty.
    /// </summary>
    internal static byte[] BuildWallDataDense(TwldData data, int tilesWide, int tilesHigh)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        for (int x = 0; x < tilesWide; x++)
        {
            for (int y = 0; y < tilesHigh; y++)
            {
                if (data.ModWallGrid.TryGetValue((x, y), out var wall) &&
                    wall.WallMapIndex < data.WallMap.Count)
                {
                    // Write the original SaveType (runtime type ID) from the entry
                    w.Write(data.WallMap[wall.WallMapIndex].SaveType);
                    w.Write(wall.WallColor);
                }
                else
                {
                    w.Write((ushort)0);
                }
            }
        }

        return ms.ToArray();
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

    #region Mod Texture Mapping

    /// <summary>
    /// Returns the list of mod internal names referenced by this world (from the "usedMods" header).
    /// </summary>
    public static List<string> GetUsedModNames(TwldData data)
    {
        if (data?.Header == null)
        {
            System.Diagnostics.Debug.WriteLine("TwldFile.GetUsedModNames: No header data");
            return new List<string>();
        }

        try
        {
            var mods = data.Header.GetList<string>("usedMods");
            System.Diagnostics.Debug.WriteLine($"TwldFile.GetUsedModNames: {mods.Count} mods — {string.Join(", ", mods)}");
            return mods;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TwldFile.GetUsedModNames: Failed to read usedMods — {ex.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// Builds a lookup from (modName, tileName) → virtual tile ID.
    /// Call after RegisterModProperties / AssignVirtualIds.
    /// </summary>
    public static Dictionary<(string ModName, string TileName), ushort> BuildTileNameToVirtualIdMap(TwldData data)
    {
        var map = new Dictionary<(string, string), ushort>(StringTupleComparer.OrdinalIgnoreCase);
        if (data == null) return map;

        for (int i = 0; i < data.TileMap.Count; i++)
        {
            var entry = data.TileMap[i];
            if (data.MapIndexToVirtualTileId.TryGetValue(i, out ushort virtualId))
            {
                map[(entry.ModName, entry.Name)] = virtualId;
            }
        }
        return map;
    }

    /// <summary>
    /// Builds a lookup from (modName, wallName) → virtual wall ID.
    /// Call after RegisterModProperties / AssignVirtualIds.
    /// </summary>
    public static Dictionary<(string ModName, string WallName), ushort> BuildWallNameToVirtualIdMap(TwldData data)
    {
        var map = new Dictionary<(string, string), ushort>(StringTupleComparer.OrdinalIgnoreCase);
        if (data == null) return map;

        for (int i = 0; i < data.WallMap.Count; i++)
        {
            var entry = data.WallMap[i];
            if (data.MapIndexToVirtualWallId.TryGetValue(i, out ushort virtualId))
            {
                map[(entry.ModName, entry.Name)] = virtualId;
            }
        }
        return map;
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
    public static TEditColor GenerateModColor(string fullName)
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

        var prop = new TileProperty
        {
            Id = virtualId,
            Name = displayName,
            Color = color,
            IsFramed = isFramed,
            IsSolid = !isFramed,
        };
        WorldConfiguration.TileProperties[virtualId] = prop;

        // Non-framed mod tiles go into the paint dropdowns (TileBricks/TileBricksMask)
        if (!isFramed)
        {
            WorldConfiguration.TileBricks.Add(prop);
            WorldConfiguration.TileBricksMask.Add(prop);
        }
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

        var prop = new WallProperty
        {
            Id = virtualId,
            Name = displayName,
            Color = color,
        };
        WorldConfiguration.WallProperties[virtualId] = prop;

        // Add to the wall mask dropdown
        WorldConfiguration.WallPropertiesMask.Add(prop);
    }

    #endregion
}

/// <summary>
/// Case-insensitive equality comparer for (string, string) tuples.
/// </summary>
internal class StringTupleComparer : IEqualityComparer<(string, string)>
{
    public static readonly StringTupleComparer OrdinalIgnoreCase = new();

    public bool Equals((string, string) x, (string, string) y)
    {
        return string.Equals(x.Item1, y.Item1, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode((string, string) obj)
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1 ?? ""),
            StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2 ?? ""));
    }
}
