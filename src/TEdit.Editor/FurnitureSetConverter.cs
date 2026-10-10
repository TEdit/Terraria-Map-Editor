#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TEdit.Common;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Editor;

/// <summary>Counts of what a furniture set conversion changed or had to leave alone.</summary>
public readonly record struct FurnitureSetConversion(int Sprites, int Blocks, int Walls, int Unmatched, int Partial, int GemTrees = 0)
{
    public int Changed => Sprites + Blocks + Walls + GemTrees;
}

/// <summary>Which parts of the selection a furniture set conversion changes.</summary>
public sealed record FurnitureSetOptions
{
    public static FurnitureSetOptions Default { get; } = new();

    public bool Furniture { get; init; } = true;
    public bool Blocks { get; init; } = true;
    public bool Walls { get; init; } = true;

    /// <summary>
    /// Replace every solid block and every wall in the selection with the target set's block and wall,
    /// not only the source set's own (e.g. a pumpkin house built from gray brick with stone slab walls).
    /// Natural terrain inside the selection is replaced too, so select the building with the brush or lasso.
    /// </summary>
    public bool AnyBlockOrWall { get; init; }

    /// <summary>Regrow gem trees as the gem whose color is closest to the target set's block (Sunplate to Topaz).</summary>
    public bool GemTrees { get; init; } = true;
}

/// <summary>
/// Converts the furniture, platforms, blocks and walls of one furniture set (Sandstone, Skyware...) into another.
/// Furniture is matched by the frame names in tiles.json ("Sandstone Chair" to "Skyware Chair"), so new sets
/// need no code changes; only the building block and wall of each set come from <see cref="BuildingMaterials"/>.
/// </summary>
public sealed class FurnitureSetConverter
{
    // Tile families that hold set furniture. Other framed tiles (stalactites, crates, saplings...) share set names by coincidence.
    private static readonly HashSet<string> FurnitureFamilies = new(StringComparer.Ordinal)
    {
        "Doors (Closed)", "Doors (Open)", "Tables", "Chairs", "Toilets", "Work Benches", "Platforms", "Beds",
        "Bathtubs", "Lamps", "Candelabras", "Candles", "Chandeliers", "Lanterns", "Bookcases", "Clocks",
        "Sinks", "Pianos", "Dressers", "Benches", "Chests",
    };

    // Every set has a chair, table or work bench; their frame names define the set names.
    private static readonly (string Family, string Suffix)[] SetDefiningItems =
    {
        ("Chairs", " Chair"), ("Tables", " Table"), ("Work Benches", " Work Bench"),
    };

    /// <summary>
    /// Building block and background wall of each set, by their names in tiles.json and walls.json.
    /// Source: the "&lt;Set&gt; furniture" pages on terraria.wiki.gg (the block and wall each set is shown with).
    /// Sets without a block or wall of their own (Banquet) are left out. Martian and Martian Hover share theirs.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (string? Block, string? Wall)> BuildingMaterials =
        new Dictionary<string, (string? Block, string? Wall)>(StringComparer.Ordinal)
        {
            ["Aetherium"] = ("Aetherium Brick", "Aetherium Brick Wall"),
            ["Aquarium"] = ("Aquarium Block", "Aquarium Wall"),
            ["Ash Wood"] = ("Ash Wood", "Ash Wood Wall"),
            ["Balloon"] = ("Silly Pink Balloon Block", "Silly Pink Balloon Wall"),
            ["Bamboo"] = ("Bamboo Block", "Bamboo Wall"),
            ["Blue Dungeon"] = ("Blue Brick", "Blue Brick Wall"),
            ["Bone"] = ("Bone Block", "Bone Block Wall"),
            ["Boreal Wood"] = ("Boreal Wood", "Boreal Wood Wall"),
            ["Boulder"] = ("Boulder Block", "Boulder Wall"),
            ["Cactus"] = ("Cactus Block", "Cactus Wall"),
            ["Cloud"] = ("Cloud Block", "Cloud Wall"),
            ["Crimtane"] = ("Crimtane Brick", "Crimtane Brick Wall"),
            ["Crystal"] = ("Crystal Block", "Crystal Block Wall"),
            ["Demonite"] = ("Demonite Brick", "Demonite Brick Wall"),
            ["Duskware"] = ("Moonplate Block", "Crescent Wall"),
            ["Dynasty"] = ("Dynasty Wood", "White Dynasty Wall"),
            ["Easter"] = ("Easter Block", "Easter Wall"),
            ["Ebonwood"] = ("Ebonwood", "Ebonwood Wall"),
            ["Fallen Star"] = ("Fallen Star Block", "Fallen Star Wall"),
            ["Feywood"] = ("Feywood", "Feywood Wall"),
            ["Flesh"] = ("Flesh Block", "Flesh Block Wall"),
            ["Flinx Fur"] = ("Flinx Fur Block", "Flinx Fur Wall"),
            ["Forbidden"] = ("Forbidden Block", "Forbidden Wall"),
            ["Frozen"] = ("Ice Block", "Ice Wall"),
            ["Glass"] = ("Glass Block", "Glass Wall"),
            ["Golden"] = ("Gold Brick", "Gold Brick Wall"),
            ["Gothic"] = ("Gothic Brick", "Gothic Brick Wall"),
            ["Granite"] = ("Smooth Granite Block", "Smooth Granite Wall"),
            ["Green Dungeon"] = ("Green Brick", "Green Brick Wall"),
            ["Hallowed"] = ("Hallowed Brick", "Hallowed Brick Wall"),
            ["Harpy"] = ("Harpy Block", "Harpy Wall"),
            ["Honey"] = ("Honey Block", "Honeyfall Wall"),
            ["Jellyfish"] = ("Jellyfish Block", "Jellyfish Wall"),
            ["Lesion"] = ("Lesion Block", "Lesion Block Wall"),
            ["Librarian"] = ("Librarian Block", "Librarian Wall"),
            ["Lihzahrd"] = ("Lihzahrd Brick", "Lihzahrd Brick Wall"),
            ["Living Wood"] = ("Living Wood Block", "Living Wood Wall"),
            ["Marble"] = ("Smooth Marble Block", "Smooth Marble Wall"),
            ["Martian"] = ("Martian Conduit Plating", "Martian Conduit Wall"),
            ["Martian Hover"] = ("Martian Conduit Plating", "Martian Conduit Wall"),
            ["Meteorite"] = ("Meteorite Brick", "Meteorite Brick Wall"),
            ["Mushroom"] = ("Glowing Mushroom Block", "Mushroom Wall"),
            ["Nebula"] = ("Nebula Brick", "Nebula Brick Wall"),
            ["Obsidian"] = ("Obsidian Brick", "Obsidian Brick Wall"),
            ["Office"] = ("Office Block", "Office Wall"),
            ["Palm Wood"] = ("Palm Wood", "Palm Wood Wall"),
            ["Pearlwood"] = ("Pearlwood", "Pearlwood Wall"),
            ["Pine"] = ("Pine Wood", "Pine Wood Wall"),
            ["Pink Dungeon"] = ("Pink Brick", "Pink Brick Wall"),
            ["Pumpkin"] = ("Pumpkin Block", "Pumpkin Wall"),
            ["Reef"] = ("Reef Block", "Reef Wall"),
            ["Rich Mahogany"] = ("Rich Mahogany", "Rich Mahogany Wall"),
            ["Sandstone"] = ("Smooth Sandstone Block", "Smooth Sandstone Wall"),
            ["Shadewood"] = ("Shadewood", "Shadewood Wall"),
            ["Skyware"] = ("Sunplate Block", "Disc Wall"),
            ["Slime"] = ("Slime Block", "Slime Block Wall"),
            ["Snow"] = ("Snow Brick", "Snow Brick Wall"),
            ["Solar"] = ("Solar Brick", "Solar Brick Wall"),
            ["Spider"] = ("Spider Nest Block", "Spider Nest Wall"),
            ["Spike"] = ("Spike Block", "Spike Wall"),
            ["Spooky"] = ("Spooky Wood", "Spooky Wood Wall"),
            ["Stardust"] = ("Stardust Brick", "Stardust Brick Wall"),
            ["Steampunk"] = ("Cog Block", "Cog Wall"),
            ["Stone"] = ("Gray Brick", "Gray Brick Wall"),
            ["Vortex"] = ("Vortex Brick", "Vortex Brick Wall"),
            ["Wooden"] = ("Wood", "Wood Wall"),
        };

    private static readonly Lazy<FurnitureSetConverter> _default = new(
        () => new FurnitureSetConverter(WorldConfiguration.TileProperties, WorldConfiguration.WallProperties));

    /// <summary>Converter built from the loaded game data.</summary>
    public static FurnitureSetConverter Default => _default.Value;

    private readonly record struct ItemKey(string Family, string Item, string Variety, FrameAnchor Anchor, int Ordinal);

    private sealed record SetFrame(ushort Type, Vector2Short UV, Vector2Short Size, string Set, ItemKey Key);

    private readonly Dictionary<ushort, List<SetFrame>> _framesByType = new();
    private readonly Dictionary<ushort, Vector2Short> _intervals = new();
    private readonly Dictionary<string, Dictionary<ItemKey, SetFrame>> _setItems = new(StringComparer.Ordinal);
    private readonly Dictionary<ushort, List<string>> _blockSets = new();
    private readonly Dictionary<ushort, List<string>> _wallSets = new();
    private readonly Dictionary<string, ushort> _setBlocks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ushort> _setWalls = new(StringComparer.Ordinal);
    private readonly HashSet<ushort> _solidBlocks = new();
    private readonly Dictionary<string, ushort> _setGemTrees = new(StringComparer.Ordinal);
    private readonly HashSet<ushort> _gemTrees = new();

    public FurnitureSetConverter(IEnumerable<TileProperty> tiles, IEnumerable<WallProperty> walls)
    {
        var tileList = tiles.Where(t => t != null).ToList();
        var aliases = BuildSetAliases(tileList);
        // Longest alias first so "Martian Hover Chair" is not read as a "Martian" item.
        var prefixes = aliases.Keys.OrderByDescending(a => a.Length).ToList();

        foreach (var tile in tileList)
        {
            string family = Family(tile.Name);
            if (!tile.IsFramed || tile.Frames == null || !FurnitureFamilies.Contains(family) || tile.Id is < 0 or > ushort.MaxValue)
                continue;

            var type = (ushort)tile.Id;
            _intervals[type] = SpritePlacer.GetTileStep(tile);
            var ordinals = new Dictionary<(string Set, ItemKey Key), int>();

            foreach (var frame in tile.Frames)
            {
                string name = frame.Name?.Trim() ?? string.Empty;
                string? prefix = prefixes.FirstOrDefault(p => name.StartsWith(p + " ", StringComparison.Ordinal));
                if (prefix == null)
                    continue;

                string set = aliases[prefix];
                var key = new ItemKey(family, name[(prefix.Length + 1)..], frame.Variety?.Trim() ?? string.Empty, frame.Anchor, 0);
                ordinals.TryGetValue((set, key), out int ordinal);
                ordinals[(set, key)] = ordinal + 1;
                key = key with { Ordinal = ordinal };

                var size = frame.Size.X > 0 && frame.Size.Y > 0 ? frame.Size : tile.GetFrameSize(frame.UV.Y);
                var setFrame = new SetFrame(type, frame.UV, size, set, key);

                if (!_framesByType.TryGetValue(type, out var list))
                    _framesByType[type] = list = new List<SetFrame>();
                list.Add(setFrame);

                if (!_setItems.TryGetValue(set, out var items))
                    _setItems[set] = items = new Dictionary<ItemKey, SetFrame>();
                items.TryAdd(key, setFrame);
            }
        }

        foreach (var tile in tileList.Where(t => !t.IsFramed && t.IsSolid && t.Id is >= 0 and <= ushort.MaxValue))
            _solidBlocks.Add((ushort)tile.Id);

        var blockIds = tileList.Where(t => !t.IsFramed && t.Id is >= 0 and <= ushort.MaxValue)
            .GroupBy(t => t.Name).ToDictionary(g => g.Key, g => (ushort)g.First().Id, StringComparer.Ordinal);
        var wallIds = walls.Where(w => w != null && w.Id is > 0 and <= ushort.MaxValue)
            .GroupBy(w => w.Name).ToDictionary(g => g.Key, g => (ushort)g.First().Id, StringComparer.Ordinal);

        foreach (var (set, (block, wall)) in BuildingMaterials)
        {
            if (!_setItems.ContainsKey(set))
                continue;
            if (block != null && blockIds.TryGetValue(block, out var blockId))
            {
                _setBlocks[set] = blockId;
                AddSet(_blockSets, blockId, set);
            }
            if (wall != null && wallIds.TryGetValue(wall, out var wallId))
            {
                _setWalls[set] = wallId;
                AddSet(_wallSets, wallId, set);
            }
        }

        // Gem trees pair with their gem stone by name ("Topaz Tree", "Topaz Stone Block"); the stone carries the gem's color.
        var gems = tileList
            .Where(t => t.IsFramed && t.Name.EndsWith(" Tree", StringComparison.Ordinal) && t.Id is >= 0 and <= ushort.MaxValue)
            .Select(t => (Tree: (ushort)t.Id, Stone: tileList.FirstOrDefault(s => !s.IsFramed && s.Name == t.Name[..^5] + " Stone Block")))
            .Where(g => g.Stone != null)
            .ToList();
        foreach (var (tree, _) in gems)
            _gemTrees.Add(tree);
        var colors = tileList.Where(t => t.Id is >= 0 and <= ushort.MaxValue).GroupBy(t => (ushort)t.Id).ToDictionary(g => g.Key, g => g.First().Color);
        var diamond = gems.FirstOrDefault(g => g.Stone!.Name.StartsWith("Diamond", StringComparison.Ordinal));
        foreach (var (set, block) in _setBlocks)
        {
            if (gems.Count == 0 || !colors.TryGetValue(block, out var color))
                continue;
            var (_, saturation, _) = ToHsv(color);
            // Gray blocks (stone, obsidian) have no hue to match; diamond is the colorless gem.
            _setGemTrees[set] = saturation < 0.15 && diamond.Stone != null
                ? diamond.Tree
                : gems.MinBy(g => ColorDistance(g.Stone!.Color, color)).Tree;
        }

        Sets = _setItems.Keys.OrderBy(s => s, StringComparer.CurrentCulture).ToList();
    }

    /// <summary>Furniture set names, sorted.</summary>
    public IReadOnlyList<string> Sets { get; }

    /// <summary>The set a tile belongs to (its furniture, block or wall), or null.</summary>
    public string? GetSet(Tile tile)
    {
        if (tile.IsActive && FindFrame(tile) is { } frame)
            return frame.Set;
        if (tile.IsActive && _blockSets.TryGetValue(tile.Type, out var blockSets))
            return blockSets[0];
        return tile.Wall != 0 && _wallSets.TryGetValue(tile.Wall, out var wallSets) ? wallSets[0] : null;
    }

    /// <summary>Set, furniture family and item of a furniture tile, e.g. ("Pumpkin", "Chairs", "Chair"); null otherwise.</summary>
    public (string Set, string Family, string Item)? GetFurniture(Tile tile) =>
        tile.IsActive && FindFrame(tile) is { } frame ? (frame.Set, frame.Key.Family, frame.Key.Item) : null;

    /// <summary>True when the set has that item in that furniture family.</summary>
    public bool HasItem(string set, string family, string item) =>
        _setItems.TryGetValue(set, out var items) && items.Keys.Any(k => k.Family == family && k.Item == item);

    /// <summary>Gem tree tile id whose gem color is closest to the set's block; null when the set has no block.</summary>
    public ushort? GetGemTree(string set) => _setGemTrees.TryGetValue(set, out var id) ? id : null;

    /// <summary>Tile id of the block a set is built from; null if none.</summary>
    public ushort? GetBlock(string set) => _setBlocks.TryGetValue(set, out var id) ? id : null;

    /// <summary>Wall id of a set; null if none.</summary>
    public ushort? GetWall(string set) => _setWalls.TryGetValue(set, out var id) ? id : null;

    /// <summary>
    /// Converts every selected tile of <paramref name="fromSet"/> (any set when null) to <paramref name="toSet"/>.
    /// Multi-tile furniture converts only when the whole sprite is selected, so no half-converted sprites are left.
    /// Furniture with no equivalent in the target set, or of a different size, is left unchanged and counted as unmatched.
    /// <paramref name="options"/> picks the parts to change; by default furniture plus the source set's own blocks and walls.
    /// </summary>
    public FurnitureSetConversion Convert(
        World world,
        RectangleInt32 area,
        Func<int, int, bool>? include,
        string? fromSet,
        string toSet,
        IUndoManager? undo,
        FurnitureSetOptions? options = null)
    {
        options ??= FurnitureSetOptions.Default;
        if (world == null || !_setItems.ContainsKey(toSet) || (toSet == fromSet && !options.AnyBlockOrWall))
            return default;

        area = RectangleInt32.Intersect(area, new RectangleInt32(0, 0, world.TilesWide, world.TilesHigh));
        bool Selected(int x, int y) => area.Contains(x, y) && (include == null || include(x, y));
        bool FromSet(string set) => set != toSet && (fromSet == null || set == fromSet);
        // A block or wall shared with the target set (Martian Conduit Plating for both Martian sets) is already right.
        bool FromSets(List<string> sets) => !sets.Contains(toSet) && sets.Exists(FromSet);
        bool IsSourceBlock(ushort type) => options.AnyBlockOrWall
            ? _solidBlocks.Contains(type)
            : _blockSets.TryGetValue(type, out var sets) && FromSets(sets);
        bool IsSourceWall(ushort wall) => options.AnyBlockOrWall || (_wallSets.TryGetValue(wall, out var sets) && FromSets(sets));

        int sprites = 0, blocks = 0, walls = 0, unmatched = 0, partial = 0, gemTrees = 0;
        var targetGemTree = options.GemTrees ? GetGemTree(toSet) : null;
        var visited = new HashSet<Vector2Int32>();
        var targetBlock = GetBlock(toSet);
        var targetWall = GetWall(toSet);

        for (int x = area.Left; x < area.Right; x++)
        {
            for (int y = area.Top; y < area.Bottom; y++)
            {
                if (!Selected(x, y))
                    continue;

                ref var tile = ref world.Tiles[x, y];
                bool saved = false;

                var frame = tile.IsActive ? FindFrame(tile) : null;
                if (tile.IsActive && targetGemTree is { } newTree && tile.Type != newTree && _gemTrees.Contains(tile.Type))
                {
                    // Every gem tree uses the same frame layout, so only the type changes.
                    undo?.SaveTile(world, x, y);
                    saved = true;
                    tile.Type = newTree;
                    gemTrees++;
                }
                else if (frame != null)
                {
                    if (options.Furniture && FromSet(frame.Set))
                    {
                        var interval = _intervals[frame.Type];
                        var origin = new Vector2Int32(x - (tile.U - frame.UV.X) / interval.X, y - (tile.V - frame.UV.Y) / interval.Y);
                        if (visited.Add(origin))
                        {
                            var parts = CollectSprite(world, origin, frame, toSet, Selected, out bool isPartial);
                            if (isPartial)
                                partial++;
                            else if (parts == null)
                                unmatched++;
                            else
                            {
                                ReplaceSprite(world, parts, undo);
                                sprites++;
                                saved = true;
                            }
                        }
                    }
                }
                else if (options.Blocks && tile.IsActive && targetBlock is { } newBlock &&
                         tile.Type != newBlock && IsSourceBlock(tile.Type))
                {
                    undo?.SaveTile(world, x, y);
                    saved = true;
                    tile.Type = newBlock;
                    blocks++;
                }

                if (options.Walls && tile.Wall != 0 && targetWall is { } newWall &&
                    tile.Wall != newWall && IsSourceWall(tile.Wall))
                {
                    if (!saved)
                        undo?.SaveTile(world, x, y);
                    tile.Wall = newWall;
                    walls++;
                }
            }
        }

        return new FurnitureSetConversion(sprites, blocks, walls, unmatched, partial, gemTrees);
    }

    // Hue matters most (blue set, blue gem); saturation separates topaz from amber. Brightness is ignored,
    // because map colors of dark blocks would otherwise all match the darkest gem.
    private static double ColorDistance(TEditColor a, TEditColor b)
    {
        var (hueA, satA, _) = ToHsv(a);
        var (hueB, satB, _) = ToHsv(b);
        double hue = Math.Abs(hueA - hueB);
        hue = Math.Min(hue, 1 - hue) * 2;
        return hue * 2 + Math.Abs(satA - satB);
    }

    private static (double Hue, double Saturation, double Value) ToHsv(TEditColor color)
    {
        double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), delta = max - min;
        double hue = delta == 0 ? 0
            : max == r ? ((g - b) / delta % 6 + 6) % 6
            : max == g ? (b - r) / delta + 2
            : (r - g) / delta + 4;
        return (hue / 6, max == 0 ? 0 : delta / max, max);
    }

    private static void AddSet(Dictionary<ushort, List<string>> sets, ushort id, string set)
    {
        if (!sets.TryGetValue(id, out var list))
            sets[id] = list = new List<string>();
        list.Add(set);
    }

    private SetFrame? FindFrame(Tile tile)
    {
        if (!_framesByType.TryGetValue(tile.Type, out var frames))
            return null;

        var interval = _intervals[tile.Type];
        foreach (var frame in frames)
        {
            if (tile.U >= frame.UV.X && tile.V >= frame.UV.Y &&
                tile.U < frame.UV.X + interval.X * frame.Size.X &&
                tile.V < frame.UV.Y + interval.Y * frame.Size.Y)
                return frame;
        }
        return null;
    }

    private SetFrame? FindTarget(SetFrame source, string toSet)
    {
        var items = _setItems[toSet];
        bool SameSize(SetFrame f) => f.Size == source.Size;

        // Same item in the same family, e.g. Sandstone Chair to Skyware Chair.
        if (items.TryGetValue(source.Key, out var match) && SameSize(match))
            return match;

        // Same item filed under another family, e.g. a toilet listed with chairs.
        match = items.Values.FirstOrDefault(f => SameSize(f) && (f.Key with { Family = source.Key.Family }) == source.Key);
        if (match != null)
            return match;

        // Renamed item when both sets have a single item in the family, e.g. Martian Holobookcase to Skyware Bookcase.
        string? sourceItem = SingleItem(_setItems[source.Set], source.Key.Family);
        string? targetItem = SingleItem(items, source.Key.Family);
        if (sourceItem != null && targetItem != null &&
            items.TryGetValue(source.Key with { Item = targetItem }, out match) && SameSize(match))
            return match;

        return null;
    }

    private static string? SingleItem(Dictionary<ItemKey, SetFrame> items, string family)
    {
        var names = items.Keys.Where(k => k.Family == family).Select(k => k.Item).Distinct().Take(2).ToList();
        return names.Count == 1 ? names[0] : null;
    }

    private readonly record struct SpritePart(int X, int Y, SetFrame Source, SetFrame Target);

    /// <summary>
    /// Every tile of the sprite at <paramref name="origin"/> with its target frame, or null when one has no equivalent.
    /// Tiles are matched one by one because a sprite can mix frames: a closed door picks its A/B/C variant per row.
    /// </summary>
    private List<SpritePart>? CollectSprite(
        World world, Vector2Int32 origin, SetFrame frame, string toSet, Func<int, int, bool> selected, out bool isPartial)
    {
        isPartial = false;
        var parts = new List<SpritePart>(frame.Size.X * frame.Size.Y);
        bool unmatched = false;
        var step = _intervals[frame.Type];

        for (int dx = 0; dx < frame.Size.X; dx++)
        {
            for (int dy = 0; dy < frame.Size.Y; dy++)
            {
                int x = origin.X + dx, y = origin.Y + dy;
                var tile = selected(x, y) ? world.Tiles[x, y] : default;
                var source = tile.IsActive && tile.Type == frame.Type ? FindFrame(tile) : null;
                if (source == null || source.Set != frame.Set || source.Key.Family != frame.Key.Family || source.Key.Item != frame.Key.Item ||
                    (tile.U - source.UV.X) / step.X != dx || (tile.V - source.UV.Y) / step.Y != dy)
                {
                    // Not selected, or not part of this sprite: leave the whole sprite alone.
                    isPartial = true;
                    return null;
                }

                var target = FindTarget(source, toSet);
                if (target == null)
                    unmatched = true;
                else
                    parts.Add(new SpritePart(x, y, source, target));
            }
        }

        return unmatched ? null : parts;
    }

    private static void ReplaceSprite(World world, List<SpritePart> parts, IUndoManager? undo)
    {
        foreach (var (x, y, source, target) in parts)
        {
            undo?.SaveTile(world, x, y);
            ref var tile = ref world.Tiles[x, y];
            // Keep the tile's own offset inside its frame, whichever spacing placed it.
            tile.Type = target.Type;
            tile.U = (short)(target.UV.X + tile.U - source.UV.X);
            tile.V = (short)(target.UV.Y + tile.V - source.UV.Y);
        }
    }

    // "Chests (Group 2)" holds the same furniture as "Chests".
    private static string Family(string name) => Regex.Replace(name ?? string.Empty, @"\s*\(Group \d+\)$", string.Empty);

    // Maps every spelling of a set name to one canonical name: "Boreal" and "Boreal Wood" are the same set.
    private static Dictionary<string, string> BuildSetAliases(List<TileProperty> tiles)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var tile in tiles)
        {
            if (tile.Frames == null)
                continue;
            foreach (var (family, suffix) in SetDefiningItems)
            {
                if (Family(tile.Name) != family)
                    continue;
                foreach (var frame in tile.Frames)
                {
                    string name = frame.Name?.Trim() ?? string.Empty;
                    if (name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.Ordinal))
                        names.Add(name[..^suffix.Length]);
                }
            }
        }

        static string Stem(string set) => set.EndsWith(" Wood", StringComparison.Ordinal) ? set[..^5] : set;

        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in names.GroupBy(Stem))
        {
            string canonical = group.OrderByDescending(n => n.Length).First();
            aliases[group.Key] = canonical;
            aliases[group.Key + " Wood"] = canonical;
        }

        // Plain wood furniture is named both "Wooden Chair" and "Wood Platform".
        if (aliases.TryGetValue("Wooden", out var wooden))
            aliases["Wood"] = wooden;

        return aliases;
    }
}
