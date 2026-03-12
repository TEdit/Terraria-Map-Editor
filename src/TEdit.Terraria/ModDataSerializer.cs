using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Common.IO;
using TEdit.Geometry;
using TEdit.Terraria.TModLoader;

namespace TEdit.Terraria;

/// <summary>
/// Per-tile mod overlay entry for serialization. Captures mod tile identity
/// stripped from the vanilla tile stream (Type >= TileCount).
/// </summary>
public struct ModTileOverlayEntry
{
    public int X;
    public int Y;
    public ushort Type;
    public byte Color;
    public short U;
    public short V;
}

/// <summary>
/// Per-tile mod wall overlay entry for serialization. Captures mod wall identity
/// stripped from the vanilla tile stream (Wall >= WallCount).
/// </summary>
public struct ModWallOverlayEntry
{
    public int X;
    public int Y;
    public ushort Wall;
    public byte WallColor;
}

/// <summary>
/// Serializes/deserializes mod NBT data for undo buffers and clipboard files.
/// Appended after vanilla chest/sign/tile-entity data with a magic marker.
///
/// Format versions:
///   v1 — chest mod items + tile entity mod data
///   v2 — adds modTiles/modWalls overlay (mod tile/wall identity preserved via NBT)
///   v3 — adds tileIdMap/wallIdMap (full virtual ID → "ModName:TileName" mapping table)
/// </summary>
public static class ModDataSerializer
{
    /// <summary>Magic marker "TNBT" identifying the start of a mod NBT payload.</summary>
    public static readonly byte[] Magic = { 0x54, 0x4E, 0x42, 0x54 }; // "TNBT"

    /// <summary>Current NBT payload format version.</summary>
    public const int CurrentVersion = 3;

    /// <summary>
    /// Writes mod NBT payload to the stream. Includes chest/entity mod data,
    /// tile/wall overlay for mod tiles beyond vanilla MaxTileId, and the full
    /// virtual ID mapping table from TwldData (v3+).
    /// </summary>
    public static void SaveModPayload(
        BinaryWriter bw,
        IList<Chest> chests,
        IList<TileEntity> entities,
        IList<ModTileOverlayEntry> modTiles = null,
        IList<ModWallOverlayEntry> modWalls = null,
        TwldData twldData = null)
    {
        var payload = BuildPayload(chests, entities, modTiles, modWalls, twldData);
        if (payload == null) return; // nothing to write

        bw.Write(Magic);
        bw.Write(CurrentVersion);
        TagIO.ToStream(payload, bw.BaseStream, compressed: false);
    }

    /// <summary>
    /// Reads mod NBT payload from the stream if present.
    /// Applies mod data back to chests/entities and returns tile/wall overlays.
    /// If targetTwldData is provided and the payload contains an ID map (v3+),
    /// overlay entries are remapped from source virtual IDs to target virtual IDs.
    /// </summary>
    public static bool LoadModPayload(
        BinaryReader br,
        IList<Chest> chests,
        IList<TileEntity> entities,
        out List<ModTileOverlayEntry> modTiles,
        out List<ModWallOverlayEntry> modWalls,
        TwldData targetTwldData = null)
    {
        modTiles = null;
        modWalls = null;

        if (!TryReadMagic(br)) return false;

        int version = br.ReadInt32();
        if (version < 1) return false;

        var payload = TagIO.FromStream(br.BaseStream, compressed: false);
        ApplyPayload(payload, chests, entities);

        if (version >= 2)
        {
            modTiles = ReadTileOverlay(payload);
            modWalls = ReadWallOverlay(payload);
        }

        // v3+: read ID maps and remap overlay entries if target world has different IDs
        if (version >= 3 && targetTwldData != null)
        {
            var sourceTileMap = ReadIdMap(payload, "tileIdMap");
            var sourceWallMap = ReadIdMap(payload, "wallIdMap");

            if (sourceTileMap.Count > 0 && modTiles != null)
            {
                var tileRemap = BuildRemap(sourceTileMap, targetTwldData, isTile: true);
                if (tileRemap.Count > 0)
                    modTiles = RemapTileOverlays(modTiles, tileRemap);
            }

            if (sourceWallMap.Count > 0 && modWalls != null)
            {
                var wallRemap = BuildRemap(sourceWallMap, targetTwldData, isTile: false);
                if (wallRemap.Count > 0)
                    modWalls = RemapWallOverlays(modWalls, wallRemap);
            }
        }

        return true;
    }

    /// <summary>
    /// Backward-compatible overload without overlay output.
    /// </summary>
    public static bool LoadModPayload(BinaryReader br, IList<Chest> chests, IList<TileEntity> entities)
    {
        return LoadModPayload(br, chests, entities, out _, out _);
    }

    /// <summary>
    /// Scans a tile grid and collects mod tile/wall overlay entries
    /// for tiles/walls beyond vanilla range.
    /// </summary>
    public static void CollectOverlays(
        Tile[,] tiles, int sizeX, int sizeY,
        out List<ModTileOverlayEntry> modTiles,
        out List<ModWallOverlayEntry> modWalls)
    {
        modTiles = new List<ModTileOverlayEntry>();
        modWalls = new List<ModWallOverlayEntry>();
        ushort tileBase = (ushort)WorldConfiguration.TileCount;
        ushort wallBase = (ushort)WorldConfiguration.WallCount;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                ref var tile = ref tiles[x, y];
                if (tile.IsActive && tile.Type >= tileBase)
                {
                    modTiles.Add(new ModTileOverlayEntry
                    {
                        X = x, Y = y,
                        Type = tile.Type,
                        Color = tile.TileColor,
                        U = tile.U, V = tile.V,
                    });
                }
                if (tile.Wall >= wallBase)
                {
                    modWalls.Add(new ModWallOverlayEntry
                    {
                        X = x, Y = y,
                        Wall = tile.Wall,
                        WallColor = tile.WallColor,
                    });
                }
            }
        }
    }

    /// <summary>
    /// Applies tile/wall overlay entries back to a tile grid.
    /// </summary>
    public static void ApplyOverlays(
        Tile[,] tiles, int sizeX, int sizeY,
        IList<ModTileOverlayEntry> modTiles,
        IList<ModWallOverlayEntry> modWalls)
    {
        if (modTiles != null)
        {
            foreach (var entry in modTiles)
            {
                if (entry.X < 0 || entry.X >= sizeX || entry.Y < 0 || entry.Y >= sizeY) continue;
                ref var tile = ref tiles[entry.X, entry.Y];
                tile.IsActive = true;
                tile.Type = entry.Type;
                tile.TileColor = entry.Color;
                tile.U = entry.U;
                tile.V = entry.V;
            }
        }

        if (modWalls != null)
        {
            foreach (var entry in modWalls)
            {
                if (entry.X < 0 || entry.X >= sizeX || entry.Y < 0 || entry.Y >= sizeY) continue;
                ref var tile = ref tiles[entry.X, entry.Y];
                tile.Wall = entry.Wall;
                tile.WallColor = entry.WallColor;
            }
        }
    }

    #region ID Map (v3)

    /// <summary>
    /// Builds the full virtual ID → "ModName:TileName" mapping table from TwldData.
    /// </summary>
    internal static TagCompound BuildTileIdMap(TwldData data)
    {
        var map = new TagCompound();
        if (data == null) return map;

        foreach (var kvp in data.MapIndexToVirtualTileId)
        {
            int mapIndex = kvp.Key;
            ushort virtualId = kvp.Value;
            if (mapIndex < data.TileMap.Count)
            {
                map.Set(virtualId.ToString(), data.TileMap[mapIndex].FullName);
            }
        }
        return map;
    }

    /// <summary>
    /// Builds the full virtual ID → "ModName:WallName" mapping table from TwldData.
    /// </summary>
    internal static TagCompound BuildWallIdMap(TwldData data)
    {
        var map = new TagCompound();
        if (data == null) return map;

        foreach (var kvp in data.MapIndexToVirtualWallId)
        {
            int mapIndex = kvp.Key;
            ushort virtualId = kvp.Value;
            if (mapIndex < data.WallMap.Count)
            {
                map.Set(virtualId.ToString(), data.WallMap[mapIndex].FullName);
            }
        }
        return map;
    }

    /// <summary>
    /// Reads a virtual ID → name mapping table from the payload.
    /// </summary>
    private static Dictionary<ushort, string> ReadIdMap(TagCompound payload, string key)
    {
        var result = new Dictionary<ushort, string>();
        if (!payload.ContainsKey(key)) return result;

        var mapTag = payload.GetCompound(key);
        if (mapTag == null) return result;

        foreach (var kvp in mapTag)
        {
            if (ushort.TryParse(kvp.Key, out ushort id) && kvp.Value is string name)
            {
                result[id] = name;
            }
        }
        return result;
    }

    /// <summary>
    /// Builds a remap dictionary: source virtual ID → target virtual ID.
    /// Uses the source ID map names to look up target IDs in the target TwldData.
    /// Returns only entries where the source and target IDs differ.
    /// </summary>
    private static Dictionary<ushort, ushort> BuildRemap(
        Dictionary<ushort, string> sourceMap,
        TwldData targetData,
        bool isTile)
    {
        var remap = new Dictionary<ushort, ushort>();

        // Build target name → virtualId lookup
        Dictionary<string, ushort> targetNameToId;
        if (isTile)
        {
            targetNameToId = new Dictionary<string, ushort>();
            foreach (var kvp in targetData.MapIndexToVirtualTileId)
            {
                if (kvp.Key < targetData.TileMap.Count)
                    targetNameToId[targetData.TileMap[kvp.Key].FullName] = kvp.Value;
            }
        }
        else
        {
            targetNameToId = new Dictionary<string, ushort>();
            foreach (var kvp in targetData.MapIndexToVirtualWallId)
            {
                if (kvp.Key < targetData.WallMap.Count)
                    targetNameToId[targetData.WallMap[kvp.Key].FullName] = kvp.Value;
            }
        }

        foreach (var kvp in sourceMap)
        {
            ushort sourceId = kvp.Key;
            string name = kvp.Value;

            if (targetNameToId.TryGetValue(name, out ushort targetId))
            {
                if (sourceId != targetId)
                    remap[sourceId] = targetId;
            }
            // If the target world doesn't have this mod tile, the entry stays as-is.
            // The overlay will apply the source ID, which won't map to a valid tile
            // in the target world — this is the expected "missing mod" behavior.
        }

        return remap;
    }

    /// <summary>
    /// Remaps tile overlay entries from source virtual IDs to target virtual IDs.
    /// </summary>
    private static List<ModTileOverlayEntry> RemapTileOverlays(
        List<ModTileOverlayEntry> entries,
        Dictionary<ushort, ushort> remap)
    {
        var result = new List<ModTileOverlayEntry>(entries.Count);
        foreach (var entry in entries)
        {
            var remapped = entry;
            if (remap.TryGetValue(entry.Type, out ushort newType))
                remapped.Type = newType;
            result.Add(remapped);
        }
        return result;
    }

    /// <summary>
    /// Remaps wall overlay entries from source virtual IDs to target virtual IDs.
    /// </summary>
    private static List<ModWallOverlayEntry> RemapWallOverlays(
        List<ModWallOverlayEntry> entries,
        Dictionary<ushort, ushort> remap)
    {
        var result = new List<ModWallOverlayEntry>(entries.Count);
        foreach (var entry in entries)
        {
            var remapped = entry;
            if (remap.TryGetValue(entry.Wall, out ushort newWall))
                remapped.Wall = newWall;
            result.Add(remapped);
        }
        return result;
    }

    #endregion

    private static bool TryReadMagic(BinaryReader br)
    {
        try
        {
            var stream = br.BaseStream;
            if (stream.Position + 4 > stream.Length) return false;

            byte[] marker = br.ReadBytes(4);
            if (marker.Length == 4 &&
                marker[0] == Magic[0] &&
                marker[1] == Magic[1] &&
                marker[2] == Magic[2] &&
                marker[3] == Magic[3])
            {
                return true;
            }

            // Not our marker — seek back
            stream.Position -= marker.Length;
            return false;
        }
        catch
        {
            return false;
        }
    }

    internal static TagCompound BuildPayload(
        IList<Chest> chests,
        IList<TileEntity> entities,
        IList<ModTileOverlayEntry> modTiles = null,
        IList<ModWallOverlayEntry> modWalls = null,
        TwldData twldData = null)
    {
        var chestMods = BuildChestMods(chests);
        var entityMods = BuildEntityMods(entities);
        var tileOverlay = BuildTileOverlay(modTiles);
        var wallOverlay = BuildWallOverlay(modWalls);
        var tileIdMap = BuildTileIdMap(twldData);
        var wallIdMap = BuildWallIdMap(twldData);

        if (chestMods.Count == 0 && entityMods.Count == 0 &&
            tileOverlay.Count == 0 && wallOverlay.Count == 0 &&
            tileIdMap.Count == 0 && wallIdMap.Count == 0)
            return null;

        var payload = new TagCompound();
        payload.Set("v", CurrentVersion);
        if (chestMods.Count > 0) payload.Set("chestMods", chestMods);
        if (entityMods.Count > 0) payload.Set("entityMods", entityMods);
        if (tileOverlay.Count > 0) payload.Set("modTiles", tileOverlay);
        if (wallOverlay.Count > 0) payload.Set("modWalls", wallOverlay);
        if (tileIdMap.Count > 0) payload.Set("tileIdMap", tileIdMap);
        if (wallIdMap.Count > 0) payload.Set("wallIdMap", wallIdMap);
        return payload;
    }

    private static List<TagCompound> BuildTileOverlay(IList<ModTileOverlayEntry> entries)
    {
        var result = new List<TagCompound>();
        if (entries == null) return result;

        foreach (var e in entries)
        {
            var tag = new TagCompound();
            tag.Set("x", e.X);
            tag.Set("y", e.Y);
            tag.Set("t", (int)e.Type);
            if (e.Color != 0) tag.Set("c", (int)e.Color);
            tag.Set("u", (int)e.U);
            tag.Set("v", (int)e.V);
            result.Add(tag);
        }
        return result;
    }

    private static List<TagCompound> BuildWallOverlay(IList<ModWallOverlayEntry> entries)
    {
        var result = new List<TagCompound>();
        if (entries == null) return result;

        foreach (var e in entries)
        {
            var tag = new TagCompound();
            tag.Set("x", e.X);
            tag.Set("y", e.Y);
            tag.Set("w", (int)e.Wall);
            if (e.WallColor != 0) tag.Set("c", (int)e.WallColor);
            result.Add(tag);
        }
        return result;
    }

    private static List<ModTileOverlayEntry> ReadTileOverlay(TagCompound payload)
    {
        var result = new List<ModTileOverlayEntry>();
        var list = payload.GetList<TagCompound>("modTiles");
        if (list == null) return result;

        foreach (var tag in list)
        {
            result.Add(new ModTileOverlayEntry
            {
                X = tag.GetInt("x"),
                Y = tag.GetInt("y"),
                Type = (ushort)tag.GetInt("t"),
                Color = (byte)tag.GetInt("c"),
                U = (short)tag.GetInt("u"),
                V = (short)tag.GetInt("v"),
            });
        }
        return result;
    }

    private static List<ModWallOverlayEntry> ReadWallOverlay(TagCompound payload)
    {
        var result = new List<ModWallOverlayEntry>();
        var list = payload.GetList<TagCompound>("modWalls");
        if (list == null) return result;

        foreach (var tag in list)
        {
            result.Add(new ModWallOverlayEntry
            {
                X = tag.GetInt("x"),
                Y = tag.GetInt("y"),
                Wall = (ushort)tag.GetInt("w"),
                WallColor = (byte)tag.GetInt("c"),
            });
        }
        return result;
    }

    private static List<TagCompound> BuildChestMods(IList<Chest> chests)
    {
        var result = new List<TagCompound>();
        for (int i = 0; i < chests.Count; i++)
        {
            var chest = chests[i];
            var modItems = BuildModItems(chest.Items);
            if (modItems.Count == 0) continue;

            var entry = new TagCompound();
            entry.Set("idx", i);
            entry.Set("items", modItems);
            result.Add(entry);
        }
        return result;
    }

    private static List<TagCompound> BuildEntityMods(IList<TileEntity> entities)
    {
        var result = new List<TagCompound>();
        for (int i = 0; i < entities.Count; i++)
        {
            var te = entities[i];
            bool hasEntityMod = te.ModItemData != null || te.ModGlobalData != null ||
                                !string.IsNullOrEmpty(te.ModName);
            var modItems = BuildModItems(te.Items);

            if (!hasEntityMod && modItems.Count == 0) continue;

            var entry = new TagCompound();
            entry.Set("idx", i);
            if (!string.IsNullOrEmpty(te.ModName)) entry.Set("mn", te.ModName);
            if (!string.IsNullOrEmpty(te.ModItemName)) entry.Set("mi", te.ModItemName);
            if (!string.IsNullOrEmpty(te.ModPrefixMod)) entry.Set("pm", te.ModPrefixMod);
            if (!string.IsNullOrEmpty(te.ModPrefixName)) entry.Set("pn", te.ModPrefixName);
            if (te.ModItemData != null) entry.Set("id", te.ModItemData.Clone());
            if (te.ModGlobalData != null)
                entry.Set("gd", te.ModGlobalData.Select(t => t?.Clone()).Where(t => t != null).ToList());
            if (modItems.Count > 0) entry.Set("items", modItems);
            result.Add(entry);
        }
        return result;
    }

    private static List<TagCompound> BuildModItems(IList<Item> items)
    {
        var result = new List<TagCompound>();
        if (items == null) return result;

        for (int s = 0; s < items.Count; s++)
        {
            var item = items[s];
            if (item == null) continue;
            bool hasMod = item.ModItemData != null || item.ModGlobalData != null ||
                          !string.IsNullOrEmpty(item.ModName);
            if (!hasMod) continue;

            var tag = new TagCompound();
            tag.Set("s", s);
            if (!string.IsNullOrEmpty(item.ModName)) tag.Set("mn", item.ModName);
            if (!string.IsNullOrEmpty(item.ModItemName)) tag.Set("mi", item.ModItemName);
            if (!string.IsNullOrEmpty(item.ModPrefixMod)) tag.Set("pm", item.ModPrefixMod);
            if (!string.IsNullOrEmpty(item.ModPrefixName)) tag.Set("pn", item.ModPrefixName);
            if (item.ModItemData != null) tag.Set("id", item.ModItemData.Clone());
            if (item.ModGlobalData != null)
                tag.Set("gd", item.ModGlobalData.Select(t => t?.Clone()).Where(t => t != null).ToList());
            result.Add(tag);
        }
        return result;
    }

    private static List<TagCompound> BuildModItems(IList<TileEntityItem> items)
    {
        var result = new List<TagCompound>();
        if (items == null) return result;

        for (int s = 0; s < items.Count; s++)
        {
            var item = items[s];
            if (item == null) continue;
            bool hasMod = item.ModItemData != null || item.ModGlobalData != null ||
                          !string.IsNullOrEmpty(item.ModName);
            if (!hasMod) continue;

            var tag = new TagCompound();
            tag.Set("s", s);
            if (!string.IsNullOrEmpty(item.ModName)) tag.Set("mn", item.ModName);
            if (!string.IsNullOrEmpty(item.ModItemName)) tag.Set("mi", item.ModItemName);
            if (!string.IsNullOrEmpty(item.ModPrefixMod)) tag.Set("pm", item.ModPrefixMod);
            if (!string.IsNullOrEmpty(item.ModPrefixName)) tag.Set("pn", item.ModPrefixName);
            if (item.ModItemData != null) tag.Set("id", item.ModItemData.Clone());
            if (item.ModGlobalData != null)
                tag.Set("gd", item.ModGlobalData.Select(t => t?.Clone()).Where(t => t != null).ToList());
            result.Add(tag);
        }
        return result;
    }

    private static void ApplyPayload(TagCompound payload, IList<Chest> chests, IList<TileEntity> entities)
    {
        var chestMods = payload.GetList<TagCompound>("chestMods");
        foreach (var cm in chestMods)
        {
            int idx = cm.GetInt("idx");
            if (idx < 0 || idx >= chests.Count) continue;
            ApplyModItems(cm.GetList<TagCompound>("items"), chests[idx].Items);
        }

        var entityMods = payload.GetList<TagCompound>("entityMods");
        foreach (var em in entityMods)
        {
            int idx = em.GetInt("idx");
            if (idx < 0 || idx >= entities.Count) continue;

            var te = entities[idx];
            if (em.ContainsKey("mn")) te.ModName = em.GetString("mn");
            if (em.ContainsKey("mi")) te.ModItemName = em.GetString("mi");
            if (em.ContainsKey("pm")) te.ModPrefixMod = em.GetString("pm");
            if (em.ContainsKey("pn")) te.ModPrefixName = em.GetString("pn");
            if (em.ContainsKey("id")) te.ModItemData = em.GetCompound("id");
            if (em.ContainsKey("gd")) te.ModGlobalData = em.GetList<TagCompound>("gd").ToList();
            ApplyModItems(em.GetList<TagCompound>("items"), te.Items);
        }
    }

    private static void ApplyModItems(List<TagCompound> modItems, IList<Item> items)
    {
        if (modItems == null || items == null) return;
        foreach (var tag in modItems)
        {
            int slot = tag.GetInt("s");
            if (slot < 0 || slot >= items.Count) continue;

            var item = items[slot];
            if (tag.ContainsKey("mn")) item.ModName = tag.GetString("mn");
            if (tag.ContainsKey("mi")) item.ModItemName = tag.GetString("mi");
            if (tag.ContainsKey("pm")) item.ModPrefixMod = tag.GetString("pm");
            if (tag.ContainsKey("pn")) item.ModPrefixName = tag.GetString("pn");
            if (tag.ContainsKey("id")) item.ModItemData = tag.GetCompound("id");
            if (tag.ContainsKey("gd")) item.ModGlobalData = tag.GetList<TagCompound>("gd").ToList();

            // Mod items must have at least stack size 1 to be visible
            if (item.IsModItem && item.StackSize < 1)
                item.StackSize = 1;
        }
    }

    private static void ApplyModItems(List<TagCompound> modItems, IList<TileEntityItem> items)
    {
        if (modItems == null || items == null) return;
        foreach (var tag in modItems)
        {
            int slot = tag.GetInt("s");
            if (slot < 0 || slot >= items.Count) continue;

            var item = items[slot];
            if (tag.ContainsKey("mn")) item.ModName = tag.GetString("mn");
            if (tag.ContainsKey("mi")) item.ModItemName = tag.GetString("mi");
            if (tag.ContainsKey("pm")) item.ModPrefixMod = tag.GetString("pm");
            if (tag.ContainsKey("pn")) item.ModPrefixName = tag.GetString("pn");
            if (tag.ContainsKey("id")) item.ModItemData = tag.GetCompound("id");
            if (tag.ContainsKey("gd")) item.ModGlobalData = tag.GetList<TagCompound>("gd").ToList();

            // Mod items must have at least stack size 1 to be visible
            if (!string.IsNullOrEmpty(item.ModName) && item.ModName != "Terraria" && item.StackSize < 1)
                item.StackSize = 1;
        }
    }
}
