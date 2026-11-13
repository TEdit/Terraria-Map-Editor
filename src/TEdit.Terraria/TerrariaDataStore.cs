using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TEdit.Common;
using TEdit.Terraria.DataModel;
using TEdit.Terraria.Loaders;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria;

public class TerrariaDataStore
{
    private static TerrariaDataStore? _instance;
    private static readonly object _initLock = new();

    public static TerrariaDataStore Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("TerrariaDataStore has not been initialized. Call Initialize() first.");
            return _instance;
        }
    }

    // Collections (for UI binding) - using *Property types directly
    public ObservableCollection<TileProperty> Tiles { get; } = [];
    public ObservableCollection<TileProperty> TileBricks { get; } = [];
    public ObservableCollection<WallProperty> Walls { get; } = [];
    public ObservableCollection<ItemProperty> Items { get; } = [];
    public ObservableCollection<PaintProperty> Paints { get; } = [];
    public ObservableCollection<ChestProperty> Chests { get; } = [];
    public ObservableCollection<SignProperty> Signs { get; } = [];

    // Indexed lookups
    public Dictionary<int, TileProperty> TileById { get; } = new();
    public Dictionary<int, ItemProperty> ItemById { get; } = new();
    public Dictionary<string, int> NpcIdByName { get; } = new();
    public Dictionary<int, string> NpcNameById { get; } = new();
    public Dictionary<int, NpcData> NpcById { get; } = new();
    public Dictionary<byte, string> PrefixById { get; } = new();
    public Dictionary<string, TEditColor> GlobalColors { get; } = new();

    // Category dictionaries (for UI dropdowns)
    public Dictionary<int, ItemProperty> ArmorHeadItems { get; } = new();
    public Dictionary<int, ItemProperty> ArmorBodyItems { get; } = new();
    public Dictionary<int, ItemProperty> ArmorLegsItems { get; } = new();
    public Dictionary<int, ItemProperty> FoodItems { get; } = new();
    public Dictionary<int, ItemProperty> CritterItems { get; } = new();
    public Dictionary<int, ItemProperty> KiteItems { get; } = new();
    public Dictionary<int, ItemProperty> AccessoryItems { get; } = new();
    public Dictionary<int, ItemProperty> MountItems { get; } = new();
    public Dictionary<int, ItemProperty> DyeItems { get; } = new();
    public Dictionary<int, ItemProperty> RackableItems { get; } = new();
    public Dictionary<int, string> TallyNames { get; } = new();

    // Reverse lookups: tile/wall ID → first item ID that places it
    public Dictionary<int, int> TileItemLookup { get; } = new();
    public Dictionary<int, int> WallItemLookup { get; } = new();

    // Dye color lookup (item ID → static tint color)
    public Dictionary<int, DyeProperty> DyeColorById { get; } = new();

    // Angler Quest fish
    public List<AnglerQuestProperty> AnglerQuests { get; } = new();

    // Localization
    public DataModel.LocalizationData? Localization { get; private set; }
    public string CurrentLocale { get; private set; } = "en-US";

    // Version management
    public DataModel.SaveVersionManager? VersionManager { get; private set; }
    public BestiaryNpcConfiguration? Bestiary { get; private set; }
    public DataModel.MorphConfiguration? Morphs { get; private set; }
    public DataModel.MorphConfiguration? DestructiveMorphs { get; private set; }
    public DataModel.BackgroundStyleConfiguration? BackgroundStyles { get; private set; }

    // Prefix data for key resolution during localization
    private List<PrefixData> _prefixDataList = [];
    private readonly Dictionary<string, string> _itemKeyByName = new(StringComparer.OrdinalIgnoreCase);

    // Bestiary-derived lookups
    public Dictionary<int, BestiaryNpcData> BestiaryNpcById { get; } = new();
    public Dictionary<string, BestiaryNpcData> BestiaryNpcByBestiaryId { get; } = new();
    public List<string> BestiaryTalkedIDs { get; } = [];
    public List<string> BestiaryNearIDs { get; } = [];
    public List<string> BestiaryKilledIDs { get; } = [];

    // Version-dependent limits
    public int TileCount { get; private set; } = 753;  // MaxTileId (752) + 1
    public int WallCount { get; private set; } = 367;  // MaxWallId (366) + 1
    public int MaxNpcId { get; private set; } = 697;   // MaxNpcId  (696) + 1
    public bool[] TileFrameImportant { get; private set; } = [];

    /// <summary>
    /// Initialize the data store from embedded resources or a filesystem data path.
    /// </summary>
    public static TerrariaDataStore Initialize(string? dataPath = null, string locale = "en-US")
    {
        lock (_initLock)
        {
            var store = new TerrariaDataStore();
            store.LoadAll(dataPath);
            store.LoadLocalization(locale, dataPath);
            _instance = store;
            return store;
        }
    }

    /// <summary>
    /// Initialize the data store from pre-loaded data (for testing).
    /// </summary>
    public static TerrariaDataStore InitializeFrom(
        List<TileProperty>? tiles = null,
        List<WallProperty>? walls = null,
        List<ItemProperty>? items = null,
        List<NpcData>? npcs = null,
        List<PaintProperty>? paints = null,
        List<PrefixData>? prefixes = null,
        List<GlobalColorEntry>? globalColors = null)
    {
        lock (_initLock)
        {
            var store = new TerrariaDataStore();
            if (tiles != null) store.PopulateTiles(tiles);
            if (walls != null) store.PopulateWalls(walls);
            if (items != null) store.PopulateItems(items);
            if (npcs != null) store.PopulateNpcs(npcs);
            if (paints != null) store.PopulatePaints(paints);
            if (prefixes != null) store.PopulatePrefixes(prefixes);
            if (globalColors != null) store.PopulateGlobalColors(globalColors);
            store.RebuildFrameImportant();
            _instance = store;
            return store;
        }
    }

    /// <summary>
    /// Reset the singleton (primarily for testing).
    /// </summary>
    public static void Reset()
    {
        lock (_initLock)
        {
            _instance = null;
        }
    }

    private void LoadAll(string? dataPath)
    {
        // Load tiles directly to TileProperty
        try
        {
            var tiles = JsonDataLoader.LoadListFromResource<TileProperty>("tiles.json", dataPath);
            PopulateTiles(tiles);
        }
        catch (FileNotFoundException) { /* optional file */ }

        // Load walls directly to WallProperty
        try
        {
            var walls = JsonDataLoader.LoadListFromResource<WallProperty>("walls.json", dataPath);
            PopulateWalls(walls);
        }
        catch (FileNotFoundException) { }

        // Load items directly to ItemProperty
        try
        {
            var items = JsonDataLoader.LoadListFromResource<ItemProperty>("items.json", dataPath);
            PopulateItems(items);
        }
        catch (FileNotFoundException) { }

        // Load NPCs (still uses NpcData as it's simple)
        try
        {
            var npcs = JsonDataLoader.LoadListFromResource<NpcData>("npcs.json", dataPath);
            PopulateNpcs(npcs);
        }
        catch (FileNotFoundException) { }

        // Load paints directly to PaintProperty
        try
        {
            var paints = JsonDataLoader.LoadListFromResource<PaintProperty>("paints.json", dataPath);
            PopulatePaints(paints);
        }
        catch (FileNotFoundException) { }

        // Load prefixes
        try
        {
            var prefixes = JsonDataLoader.LoadListFromResource<PrefixData>("prefixes.json", dataPath);
            PopulatePrefixes(prefixes);
        }
        catch (FileNotFoundException) { }

        // Load global colors
        try
        {
            var colors = JsonDataLoader.LoadListFromResource<GlobalColorEntry>("globalColors.json", dataPath);
            PopulateGlobalColors(colors);
        }
        catch (FileNotFoundException) { }

        // Load version manager
        try
        {
            using var stream = JsonDataLoader.GetDataStream("versions.json", dataPath);
            VersionManager = DataModel.SaveVersionManager.Load(stream);
        }
        catch (FileNotFoundException) { }

        // Load bestiary
        try
        {
            var bestiary = JsonDataLoader.LoadFromResource<BestiaryNpcConfiguration>("bestiaryNpcs.json", dataPath);
            PopulateBestiary(bestiary);
        }
        catch (FileNotFoundException) { }

        // Load morph configuration
        try
        {
            using var stream = JsonDataLoader.GetDataStream("morphBiomes.json", dataPath);
            Morphs = DataModel.MorphConfiguration.Load(stream);
        }
        catch (FileNotFoundException) { }

        // Keep the original broad profile intact for explicit biome generation.
        // Safe conversion uses the independently validated morphBiomes.json profile.
        try
        {
            using var stream = JsonDataLoader.GetDataStream("morphBiomes.generate.json", dataPath);
            DestructiveMorphs = DataModel.MorphConfiguration.Load(stream);
        }
        catch (FileNotFoundException)
        {
            DestructiveMorphs = Morphs;
        }

        // Load background styles
        try
        {
            using var stream = JsonDataLoader.GetDataStream("backgroundStyles.json", dataPath);
            BackgroundStyles = DataModel.BackgroundStyleConfiguration.Load(stream);
        }
        catch (FileNotFoundException) { }

        // Load dye color data
        try
        {
            var dyes = JsonDataLoader.LoadListFromResource<DyeProperty>("dyes.json", dataPath);
            PopulateDyes(dyes);
        }
        catch (FileNotFoundException) { }

        ResolveRarityColors();
        RebuildFrameImportant();
    }

    private void ResolveRarityColors()
    {
        foreach (var item in Items)
        {
            if (GlobalColors.TryGetValue("Rarity_" + item.Rarity, out var color))
                item.RarityColor = color;
        }
    }

    internal void PopulateTiles(List<TileProperty> tiles)
    {
        Tiles.Clear();
        TileBricks.Clear();
        TileById.Clear();

        // Add air brick first
        TileBricks.Add(new TileProperty { Id = -1, Name = "Air", Color = TEditColor.Transparent });

        foreach (var tile in tiles)
        {
            Tiles.Add(tile);
            TileById[tile.Id] = tile;

            if (!tile.IsFramed)
            {
                TileBricks.Add(tile);
            }
        }

        // Derive chests and signs from tile frame data
        var chests = TileDataLoader.DeriveChests(tiles);
        Chests.Clear();
        foreach (var chest in chests)
            Chests.Add(chest);

        var signs = TileDataLoader.DeriveSigns(tiles);
        Signs.Clear();
        foreach (var sign in signs)
            Signs.Add(sign);
    }

    internal void PopulateWalls(List<WallProperty> walls)
    {
        Walls.Clear();
        foreach (var wall in walls)
            Walls.Add(wall);
    }

    internal void PopulateItems(List<ItemProperty> items)
    {
        Items.Clear();
        ItemById.Clear();
        ArmorHeadItems.Clear();
        ArmorBodyItems.Clear();
        ArmorLegsItems.Clear();
        FoodItems.Clear();
        CritterItems.Clear();
        KiteItems.Clear();
        AccessoryItems.Clear();
        MountItems.Clear();
        DyeItems.Clear();
        RackableItems.Clear();
        TallyNames.Clear();
        TileItemLookup.Clear();
        WallItemLookup.Clear();

        foreach (var item in items)
        {
            Items.Add(item);
            ItemById[item.Id] = item;

            if (item.Tally > 0) TallyNames[item.Tally] = item.Name;
            if (item.Head.HasValue) ArmorHeadItems[item.Id] = item;
            if (item.Body.HasValue) ArmorBodyItems[item.Id] = item;
            if (item.Legs.HasValue) ArmorLegsItems[item.Id] = item;
            if (item.IsRackable) RackableItems[item.Id] = item;
            if (item.IsFood) FoodItems[item.Id] = item;
            if (item.IsCritter) CritterItems[item.Id] = item;
            if (item.IsKite) KiteItems[item.Id] = item;
            if (item.IsAccessory) AccessoryItems[item.Id] = item;
            if (item.IsMount) MountItems[item.Id] = item;
            if (DyeColorById.ContainsKey(item.Id)) DyeItems[item.Id] = item;

            // Build reverse lookups: tile/wall → first item that places it
            if (item.CreateTile.HasValue && !TileItemLookup.ContainsKey(item.CreateTile.Value))
                TileItemLookup[item.CreateTile.Value] = item.Id;
            if (item.CreateWall.HasValue && !WallItemLookup.ContainsKey(item.CreateWall.Value))
                WallItemLookup[item.CreateWall.Value] = item.Id;
        }
        PopulateAnglerQuests();
    }

    internal void PopulateDyes(List<DyeProperty> dyes)
    {
        DyeColorById.Clear();
        foreach (var dye in dyes)
            DyeColorById[dye.ItemId] = dye;

        DyeItems.Clear();
        foreach (var item in Items)
            if (DyeColorById.ContainsKey(item.Id)) DyeItems[item.Id] = item;
    }

    /// <summary>
    /// Item IDs for Angler quests, matching Terraria.Main.anglerQuestItemNetIDs (0..38: 2450..2488, 39: 4393, 40: 4394).
    /// </summary>
    private static readonly int[] AnglerQuestItemIds = [.. Enumerable.Range(2450, 39), 4393, 4394];

    internal void PopulateAnglerQuests()
    {
        AnglerQuests.Clear();
        for (int i = 0; i < AnglerQuestItemIds.Length; i++)
        {
            int itemId = AnglerQuestItemIds[i];
            ItemById.TryGetValue(itemId, out var item);
            AnglerQuests.Add(new AnglerQuestProperty
            {
                QuestId = i,
                ItemId = itemId,
                Item = item
            });
        }
    }

    internal void PopulateNpcs(List<NpcData> npcs)
    {
        NpcIdByName.Clear();
        NpcNameById.Clear();
        NpcById.Clear();

        foreach (var npc in npcs)
        {
            NpcIdByName[npc.Name] = npc.Id;
            NpcNameById[npc.Id] = npc.Name;
            NpcById[npc.Id] = npc;
        }
    }

    internal void PopulatePaints(List<PaintProperty> paints)
    {
        Paints.Clear();
        foreach (var paint in paints)
            Paints.Add(paint);
    }

    internal void PopulatePrefixes(List<PrefixData> prefixes)
    {
        PrefixById.Clear();
        _prefixDataList = prefixes;
        foreach (var prefix in prefixes)
            PrefixById[(byte)prefix.Id] = prefix.Name;
    }

    internal void PopulateGlobalColors(List<GlobalColorEntry> colors)
    {
        GlobalColors.Clear();
        foreach (var entry in colors)
            GlobalColors[entry.Name] = entry.Color;
    }

    internal void PopulateBestiary(BestiaryNpcConfiguration bestiary)
    {
        Bestiary = bestiary;
        BestiaryNpcById.Clear();
        BestiaryNpcByBestiaryId.Clear();
        BestiaryTalkedIDs.Clear();
        BestiaryNearIDs.Clear();
        BestiaryKilledIDs.Clear();

        foreach (var npc in bestiary.NpcData)
        {
            BestiaryNpcById[npc.Id] = npc;
            BestiaryNpcByBestiaryId[npc.BestiaryId] = npc;

            if (npc.CanTalk) BestiaryTalkedIDs.Add(npc.BestiaryId);
            if (npc.IsCritter) BestiaryNearIDs.Add(npc.BestiaryId);
            if (npc.IsKillCredit) BestiaryKilledIDs.Add(npc.BestiaryId);
        }
    }

    public void LoadLocalization(string locale, string? dataPath = null)
    {
        CurrentLocale = locale;

        if (locale == "en-US")
        {
            Localization = null;
            return;
        }

        var data = Loaders.LocalizationLoader.LoadLocalization(locale, dataPath);
        Localization = data;
        ApplyLocalization(data);
    }

    private void ApplyLocalization(DataModel.LocalizationData data)
    {
        // Frame style / variant names arrive as display names; index item name -> key so they can resolve back.
        _itemKeyByName.Clear();
        foreach (var item in Items)
            if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Key))
                _itemKeyByName.TryAdd(item.Name, item.Key);

        // Build key→object lookups from the Key property on each model
        var itemsByKey = Items
            .Where(i => i.Key != null)
            .ToDictionary(i => i.Key!);
        var tilesByKey = Tiles
            .Where(t => t.Key != null)
            .ToDictionary(t => t.Key!);
        var wallsByKey = Walls
            .Where(w => w.Key != null)
            .ToDictionary(w => w.Key!);
        var npcsByKey = NpcById.Values
            .Where(n => n.Key != null)
            .ToDictionary(n => n.Key!);
        var prefixKeyToId = _prefixDataList
            .Where(p => p.Key != null)
            .ToDictionary(p => p.Key!, p => (byte)p.Id);

        // Display only: Name stays the English identity.
        foreach (var kv in data.Items)
            if (itemsByKey.TryGetValue(kv.Key, out var item))
                item.LocalizedName = kv.Value;

        // Apply tile names
        foreach (var kv in data.Tiles)
            if (tilesByKey.TryGetValue(kv.Key, out var tile))
                tile.LocalizedName = kv.Value;

        // Fallback for tiles not covered by data.Tiles
        foreach (var tile in Tiles)
        {
            if (tile.Key != null && data.Tiles.ContainsKey(tile.Key))
                continue;

            if (tile.Key != null && data.Items.TryGetValue(tile.Key, out var tileName))
                tile.LocalizedName = tileName;
            else if (tile.Key != null && data.SupplementalNames.TryGetValue(tile.Key, out var frameTileName))
                tile.LocalizedName = frameTileName;
            else if (GetPlacingItemName(TileItemLookup, tile.Id) is { } itemName)
                tile.LocalizedName = itemName;
        }

        // Apply wall names
        foreach (var kv in data.Walls)
            if (wallsByKey.TryGetValue(kv.Key, out var wall))
                wall.LocalizedName = kv.Value;

        // Fallback for walls not covered by data.Walls
        foreach (var wall in Walls)
        {
            if (wall.Key == null || data.Walls.ContainsKey(wall.Key))
                continue;

            if (data.Items.TryGetValue(wall.Key, out var wallName))
                wall.LocalizedName = wallName;
            else if (data.Items.TryGetValue(wall.Key + "Wall", out var wallNameWall))
                wall.LocalizedName = wallNameWall;
            else if (GetPlacingItemName(WallItemLookup, wall.Id) is { } itemName)
                wall.LocalizedName = itemName;
        }

        // Natural/unused variants have no game name; reuse the base wall's name + marker.
        if (data.SupplementalNames.TryGetValue("Wall.Natural", out var naturalMarker)
            && data.SupplementalNames.TryGetValue("Wall.Unused", out var unusedMarker))
        {
            var baseLocalizedByName = new Dictionary<string, string>();
            foreach (var wall in Walls)
                baseLocalizedByName.TryAdd(wall.Name, wall.LocalizedName);

            foreach (var wall in Walls)
            {
                string marker;
                if (wall.Name.EndsWith(" (natural)", StringComparison.Ordinal))
                    marker = naturalMarker;
                else if (wall.Name.EndsWith(" (unused)", StringComparison.Ordinal))
                    marker = unusedMarker;
                else
                    continue;

                var baseEnglish = wall.Name[..wall.Name.LastIndexOf('(')].TrimEnd();
                if (baseLocalizedByName.TryGetValue(baseEnglish, out var baseLocalized) && baseLocalized != baseEnglish)
                    wall.LocalizedName = baseLocalized + marker;
            }
        }

        // Apply paint names
        foreach (var paint in Paints)
            if (paint.Key != null && data.Items.TryGetValue(paint.Key, out var paintName))
                paint.LocalizedName = paintName;

        // Apply bestiary NPC names
        if (Bestiary != null)
        {
            foreach (var npc in Bestiary.NpcData)
            {
                if (data.Npcs.TryGetValue(npc.BestiaryId, out var localizedName))
                {
                    npc.LocalizedName = localizedName;
                }
            }
        }

        // Display only: NpcIdByName / NpcNameById stay English for saves and scripts.
        foreach (var kv in data.Npcs)
            if (npcsByKey.TryGetValue(kv.Key, out var npc))
                npc.LocalizedName = kv.Value;

        // Apply prefix names
        foreach (var kv in data.Prefixes)
            if (prefixKeyToId.TryGetValue(kv.Key, out var id))
                PrefixById[id] = kv.Value;
    }

    /// <summary>Gets the display name of the item that places a tile or wall, if any.</summary>
    private string? GetPlacingItemName(Dictionary<int, int> lookup, int tileOrWallId) =>
        lookup.TryGetValue(tileOrWallId, out var itemId) && ItemById.TryGetValue(itemId, out var item)
            ? item.LocalizedName
            : null;

    private void RebuildFrameImportant()
    {
        var count = TileCount;
        var arr = new bool[count];
        var limit = Math.Min(count, Tiles.Count);
        for (int i = 0; i < limit; i++)
            arr[Tiles[i].Id] = Tiles[i].IsFramed;

        TileFrameImportant = arr;
    }

    /// <summary>
    /// Update limits for a specific world version.
    /// </summary>
    public void ApplyWorldVersion(uint worldVersion)
    {
        if (VersionManager == null) return;

        var data = VersionManager.GetData(worldVersion);
        TileCount = data.MaxTileId + 1;
        WallCount = data.MaxWallId + 1;
        MaxNpcId = data.MaxNpcId + 1;
        RebuildFrameImportant();
    }

    /// <summary>Gets the localized name of a frame anchor (e.g. "Left").</summary>
    public string GetLocalizedAnchor(FrameAnchor anchor)
    {
        var key = $"Anchor.{anchor}";
        if (Localization?.SupplementalNames.TryGetValue(key, out var label) == true)
            return label;

        return anchor.ToString();
    }

    /// <summary>
    /// Resolves a supplemental sprite-style / biome-variant label: item name, then tile name, then
    /// supplementalNames.{locale}.json, falling back to the original name when unmatched.
    /// </summary>
    public string GetLocalizedSpriteStyle(string name) =>
        Localization is { } loc ? ResolveStyleName(loc, name) : name;

    private string ResolveStyleName(LocalizationData loc, string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        if (ResolveItemName(loc, name) is { } itemName)
            return itemName;
        if (loc.Tiles.TryGetValue(name, out var tileName))
            return tileName;
        if (loc.SupplementalNames.TryGetValue(name, out var styleName))
            return styleName;

        return name;
    }

    /// <summary>Builds the full localized name for a tile frame style (base name, variant and anchor suffix).</summary>
    public string GetLocalizedFrameName(TileProperty tile, FrameProperty frame)
    {
        if (Localization is not { } loc)
            return frame.ToString();

        // 1. Base name from the style lookup, falling back to the tile's own name for default frames.
        string baseName = ResolveStyleName(loc, frame.Name);
        if (baseName == frame.Name
            && (string.IsNullOrWhiteSpace(frame.Name) || frame.Name == "Default")
            && !string.IsNullOrWhiteSpace(tile.LocalizedName) && tile.LocalizedName != "Default")
            baseName = tile.LocalizedName;

        // 2. Resolve the variant/style name (banners, paintings, trophies, direct style lookup and suffix variants).
        string? localizedVariety = null;
        if (!string.IsNullOrWhiteSpace(frame.Variety) && frame.Variety != "None")
        {
            // Boss trophies: the variant + " Trophy" is the item name (e.g. Eye of Cthulhu).
            if (frame.Name == "Boss Trophy" && ResolveItemName(loc, frame.Variety + " Trophy") is { } trophyItem)
                return trophyItem;

            // Banners / paintings: the variant is the item name (e.g. Blue Slime Banner).
            if (ResolveItemName(loc, frame.Variety) is { } directItem)
            {
                // Painting tiles (keys Painting3X3/4X3/6X4/2X3/3X2) use the variant's item name as the full display
                // name. Tile 240 also hosts boss trophies, which must keep the composed name.
                bool isPaintingTile = tile.Key?.StartsWith("Painting", StringComparison.Ordinal) == true;
                if (frame.Name.EndsWith("Banner", StringComparison.OrdinalIgnoreCase) ||
                    frame.Name == "Painting" || (isPaintingTile && frame.Name != "Boss Trophy"))
                {
                    return directItem;
                }
                localizedVariety = directItem;
            }
            // Look up the style name directly in the supplementalNames dictionary.
            else if (loc.SupplementalNames.TryGetValue(frame.Variety, out var styleVal))
            {
                localizedVariety = styleVal;
            }
            // Compound suffix pattern (e.g. split "Flat A" into "Flat" + " A").
            else if (ResolveVarietySuffix(loc, frame.Variety) is { } suffixVal)
            {
                localizedVariety = suffixVal;
            }
            else
            {
                localizedVariety = frame.Variety;
            }
        }

        // 3. Compose the full name and append the anchor suffix.
        var name = baseName;
        if (!string.IsNullOrWhiteSpace(localizedVariety))
            name += ": " + localizedVariety;
        if (frame.Anchor != FrameAnchor.None)
            name += $" [{GetLocalizedAnchor(frame.Anchor)}]";
        return name;
    }

    /// <summary>Resolves a style/variant name to a localized item name (by item key, then directly by name).</summary>
    private string? ResolveItemName(LocalizationData loc, string name) =>
        _itemKeyByName.TryGetValue(name, out var key) && loc.Items.TryGetValue(key, out var value) ? value
        : loc.Items.TryGetValue(name, out var directValue) ? directValue
        : null;

    /// <summary>Resolves by base name + variant suffix (e.g. split 'Flat A' into 'Flat' + ' A').</summary>
    private string? ResolveVarietySuffix(LocalizationData loc, string variety)
    {
        int lastSpace = variety.LastIndexOf(' ');
        if (lastSpace <= 0)
            return null;

        string basePart = variety[..lastSpace];
        var resolved = loc.SupplementalNames.TryGetValue(basePart, out var style) ? style : ResolveItemName(loc, basePart);
        return resolved is null ? null : resolved + variety[lastSpace..];
    }
}
