using System;
using System.Collections.Generic;
using TEdit.Common.IO;

namespace TEdit.Terraria.TModLoader;

/// <summary>
/// In-memory model for all mod data parsed from a .twld file.
/// </summary>
public class TwldData
{
    /// <summary>Tile type map: save-time index → mod tile entry.</summary>
    public List<ModTileEntry> TileMap { get; set; } = new();

    /// <summary>Wall type map: save-time index → mod wall entry.</summary>
    public List<ModWallEntry> WallMap { get; set; } = new();

    /// <summary>Sparse overlay of mod tile data keyed by (x, y).</summary>
    public Dictionary<(int X, int Y), ModTileData> ModTileGrid { get; set; } = new();

    /// <summary>Sparse overlay of mod wall data keyed by (x, y).</summary>
    public Dictionary<(int X, int Y), ModWallData> ModWallGrid { get; set; } = new();

    /// <summary>Raw interleaved binary tile/wall data from older .twld format (deferred parse).</summary>
    public byte[] RawTileWallData { get; set; } = Array.Empty<byte>();

    /// <summary>Raw binary tile data from newer .twld format (separate "tileData" key).</summary>
    public byte[] RawTileData { get; set; } = Array.Empty<byte>();

    /// <summary>Raw binary wall data from newer .twld format (separate "wallData" key).</summary>
    public byte[] RawWallData { get; set; } = Array.Empty<byte>();

    /// <summary>Whether the raw binary data has been parsed into ModTileGrid/ModWallGrid.</summary>
    public bool TileWallDataParsed { get; set; }

    /// <summary>Mod chest item tags, keyed by chest position (x, y) → list of item slot tags.</summary>
    public List<TagCompound> ModChestItems { get; set; } = new();

    /// <summary>Mod NPC tags (preserved for round-trip).</summary>
    public List<TagCompound> ModNpcs { get; set; } = new();

    /// <summary>Mod tile entity tags (preserved for round-trip).</summary>
    public List<TagCompound> ModTileEntities { get; set; } = new();

    /// <summary>Header info: used mods, mod versions, etc.</summary>
    public TagCompound Header { get; set; } = new();

    /// <summary>Container data tag (preserved for round-trip).</summary>
    public TagCompound ContainerData { get; set; } = new();

    /// <summary>
    /// The full raw root TagCompound, preserved for round-tripping sections we don't interpret
    /// (modData, killCounts, bestiary, anglerQuest, townManager, alteredVanillaFields, etc.).
    /// </summary>
    public TagCompound RawTag { get; set; } = new();

    /// <summary>Maps binary saveType values to TileMap list indices (built from "value" NBT field).</summary>
    public Dictionary<ushort, int> SaveTypeToTileMapIndex { get; set; } = new();

    /// <summary>Maps binary saveType values to WallMap list indices (built from "value" NBT field).</summary>
    public Dictionary<ushort, int> SaveTypeToWallMapIndex { get; set; } = new();

    /// <summary>Maps virtual tile IDs (beyond vanilla range) back to TileMap indices.</summary>
    public Dictionary<ushort, int> VirtualTileIdToMapIndex { get; set; } = new();

    /// <summary>Maps virtual wall IDs (beyond vanilla range) back to WallMap indices.</summary>
    public Dictionary<ushort, int> VirtualWallIdToMapIndex { get; set; } = new();

    /// <summary>Maps TileMap index to virtual tile ID.</summary>
    public Dictionary<int, ushort> MapIndexToVirtualTileId { get; set; } = new();

    /// <summary>Maps WallMap index to virtual wall ID.</summary>
    public Dictionary<int, ushort> MapIndexToVirtualWallId { get; set; } = new();
}
