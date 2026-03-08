using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TEdit.Common.Serialization;

namespace TEdit.Terraria.DataModel;

public class MorphConfiguration
{
    public Dictionary<string, MorphBiomeData> Biomes { get; set; } = new();
    public Dictionary<string, int> MossTypes { get; set; } = new();
    public List<MorphGroup> MorphGroups { get; set; } = [];

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

    /// <summary>
    /// Expand morph groups into per-biome MorphId entries.
    /// Groups define equivalence classes across biomes; this method generates
    /// the concrete MorphId rules that MorphBiomeDataApplier consumes.
    /// </summary>
    public void ExpandGroups()
    {
        foreach (var group in MorphGroups)
        {
            bool isTile = group.Category == "tile";

            foreach (var (biomeName, biomeData) in Biomes)
            {
                if (!group.Variants.TryGetValue(biomeName, out var targetVariant))
                    continue;

                var morphList = isTile ? biomeData.MorphTiles : biomeData.MorphWalls;

                // Build set of source IDs already claimed by hand-authored rules
                var claimedIds = new HashSet<ushort>(morphList.SelectMany(m => m.SourceIds));

                // Collect all other variants as sources
                var otherVariants = group.Variants
                    .Where(kv => kv.Key != biomeName && kv.Value.TileId.HasValue)
                    .ToList();

                if (otherVariants.Count == 0 && !targetVariant.Delete)
                    continue;

                // Partition sources: same tile ID as target (frame-offset) vs different tile ID (swap)
                var sameTileId = otherVariants
                    .Where(kv => kv.Value.TileId == targetVariant.TileId && targetVariant.TileId.HasValue)
                    .ToList();

                var differentTileId = otherVariants
                    .Where(kv => kv.Value.TileId != targetVariant.TileId || !targetVariant.TileId.HasValue)
                    .ToList();

                // Case A: Different tile IDs (tile-ID swap)
                if (differentTileId.Count > 0)
                {
                    var sourceIds = differentTileId
                        .Select(kv => kv.Value.TileId!.Value)
                        .Where(id => !claimedIds.Contains(id))
                        .ToHashSet();

                    if (sourceIds.Count > 0)
                    {
                        var morphId = new MorphId
                        {
                            Name = $"group:{group.Name}",
                            Delete = targetVariant.Delete,
                            SourceIds = sourceIds,
                        };

                        if (!targetVariant.Delete && targetVariant.TileId.HasValue)
                        {
                            morphId.Default = new MorphIdLevels { SkyId = targetVariant.TileId.Value };
                            morphId.SpriteReplacement = new MorphSpriteReplacement
                            {
                                TargetTileId = targetVariant.TileId.Value,
                            };
                        }

                        morphList.Add(morphId);

                        // Track newly claimed IDs
                        foreach (var id in sourceIds)
                            claimedIds.Add(id);
                    }
                }

                // Case B: Same tile ID, different frames (sprite offset)
                if (sameTileId.Count > 0 && targetVariant.TileId.HasValue)
                {
                    var sharedTileId = targetVariant.TileId.Value;

                    // Only add if the shared tile ID is not already fully claimed
                    // (for sprite offsets, the source ID is the same tile, so we check
                    // whether we need to add offset rules)
                    var spriteOffsets = new List<MorphSpriteUVOffset>();

                    foreach (var (_, sourceVariant) in sameTileId)
                    {
                        if (sourceVariant.FrameU == null && targetVariant.FrameU == null)
                            continue;

                        short srcU = sourceVariant.FrameU ?? 0;
                        short srcV = sourceVariant.FrameV ?? 0;
                        short tgtU = targetVariant.FrameU ?? 0;
                        short tgtV = targetVariant.FrameV ?? 0;

                        if (srcU == tgtU && srcV == tgtV)
                            continue;

                        short srcW = sourceVariant.FrameWidth ?? 1;
                        short srcH = sourceVariant.FrameHeight ?? 1;

                        var offset = new MorphSpriteUVOffset
                        {
                            MinU = srcU,
                            MaxU = (short)(srcU + srcW - 1),
                            OffsetU = (short)(tgtU - srcU),
                        };

                        if (sourceVariant.FrameHeight.HasValue || targetVariant.FrameHeight.HasValue)
                        {
                            offset.UseFilterV = true;
                            offset.MinV = srcV;
                            offset.MaxV = (short)(srcV + srcH - 1);
                            offset.OffsetV = (short)(tgtV - srcV);
                        }

                        spriteOffsets.Add(offset);
                    }

                    if (spriteOffsets.Count > 0)
                    {
                        // Check if there is already a rule for this tile ID with sprite offsets
                        var existingRule = morphList
                            .FirstOrDefault(m => m.SourceIds.Contains(sharedTileId) && m.SpriteOffsets.Count > 0);

                        if (existingRule == null)
                        {
                            morphList.Add(new MorphId
                            {
                                Name = $"group:{group.Name}:frames",
                                SourceIds = [sharedTileId],
                                Default = new MorphIdLevels { SkyId = sharedTileId },
                                SpriteOffsets = spriteOffsets,
                            });
                        }
                    }
                }
            }
        }
    }

    public static MorphConfiguration Load(Stream stream)
    {
        var config = JsonSerializer.Deserialize<MorphConfiguration>(stream, TEditJsonSerializer.DefaultOptions)
            ?? throw new InvalidOperationException("Failed to deserialize morph configuration.");
        config.ExpandGroups();
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
