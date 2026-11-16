using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
    public Dictionary<int, string> NpcDisplayNameById { get; } = new();
    public Dictionary<int, NpcData> NpcById { get; } = new();
    public Dictionary<byte, string> PrefixById { get; } = new();
    public Dictionary<byte, string> PrefixDisplayById { get; } = new();
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

    // Localization
    public DataModel.LocalizationData? Localization { get; private set; }
    public string CurrentLocale { get; private set; } = "en-US";

    // Runtime indexes rebuilt whenever a locale is applied.
    private readonly Dictionary<string, string> _localizedItemNamesBySourceName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _localizedTileNamesBySourceName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _localizedWallNamesBySourceName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TileProperty> _tilesByKey = new(StringComparer.Ordinal);
    private readonly Dictionary<string, WallProperty> _wallsByKey = new(StringComparer.Ordinal);

    // Version management
    public DataModel.SaveVersionManager? VersionManager { get; private set; }
    public BestiaryNpcConfiguration? Bestiary { get; private set; }
    public DataModel.MorphConfiguration? Morphs { get; private set; }
    public DataModel.MorphConfiguration? DestructiveMorphs { get; private set; }
    public DataModel.BackgroundStyleConfiguration? BackgroundStyles { get; private set; }

    // Prefix data for key resolution during localization
    private List<PrefixData> _prefixDataList = [];
    public IReadOnlyList<PrefixData> Prefixes => _prefixDataList;

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
        TileBricks.Add(new TileProperty { Id = -1, Name = "Air", Key = "Air", Color = TEditColor.Transparent });

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

    internal void PopulateNpcs(List<NpcData> npcs)
    {
        NpcIdByName.Clear();
        NpcNameById.Clear();
        NpcDisplayNameById.Clear();
        NpcById.Clear();

        foreach (var npc in npcs)
        {
            NpcIdByName[npc.Name] = npc.Id;
            NpcNameById[npc.Id] = npc.Name;
            NpcDisplayNameById[npc.Id] = npc.DisplayName;
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
        PrefixDisplayById.Clear();
        _prefixDataList = prefixes;
        foreach (var prefix in prefixes)
        {
            var id = (byte)prefix.Id;
            PrefixById[id] = prefix.Name;
            PrefixDisplayById[id] = prefix.DisplayName;
        }
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

        var hasEnglishOverride = locale == "en-US"
            && dataPath != null
            && File.Exists(Path.Combine(dataPath, "Localization", "en-US.json"));
        var hasOverrides = LocalizationLoader.HasOverrides(locale, dataPath);
        if (locale == "en-US" && !hasEnglishOverride && !hasOverrides)
        {
            Localization = null;
            ApplyLocalization(new DataModel.LocalizationData());
            return;
        }

        var data = LocalizationLoader.LoadLocalization(locale, dataPath);
        Localization = data;
        ApplyLocalization(data);
    }

    private void ApplyLocalization(DataModel.LocalizationData data)
    {
        ResetLocalizationState();

        // Build key→object lookups from the Key property on each model.
        var itemsByKey = Items
            .Where(i => i.Key != null)
            .ToDictionary(i => i.Key!);
        var tilesByKey = Tiles
            .Where(t => t.Key != null)
            .ToDictionary(t => t.Key!);
        _tilesByKey.Clear();
        foreach (var (key, tile) in tilesByKey)
            _tilesByKey[key] = tile;
        foreach (var air in TileBricks.Where(tile => tile.Id == -1 && tile.Key != null))
        {
            tilesByKey[air.Key!] = air;
            _tilesByKey[air.Key!] = air;
        }
        var wallsByKey = Walls
            .Where(w => w.Key != null)
            .ToDictionary(w => w.Key!);
        _wallsByKey.Clear();
        foreach (var (key, wall) in wallsByKey)
            _wallsByKey[key] = wall;
        var npcsByKey = NpcById.Values
            .Where(n => n.Key != null)
            .ToDictionary(n => n.Key!);
        var prefixesByKey = _prefixDataList
            .Where(p => p.Key != null)
            .ToDictionary(p => p.Key!);

        // Keep source names stable; localized values are used only for display.
        foreach (var kv in data.Items)
            if (!string.IsNullOrWhiteSpace(kv.Value) && itemsByKey.TryGetValue(kv.Key, out var item))
                item.LocalizedName = kv.Value;

        foreach (var kv in data.Tiles)
            if (!string.IsNullOrWhiteSpace(kv.Value) && tilesByKey.TryGetValue(kv.Key, out var tile))
                tile.LocalizedName = kv.Value;

        foreach (var kv in data.Walls)
            if (!string.IsNullOrWhiteSpace(kv.Value) && wallsByKey.TryGetValue(kv.Key, out var wall))
                wall.LocalizedName = kv.Value;

        foreach (var kv in data.Npcs)
            if (!string.IsNullOrWhiteSpace(kv.Value) && npcsByKey.TryGetValue(kv.Key, out var npc))
                npc.LocalizedName = kv.Value;

        foreach (var kv in data.Prefixes)
            if (!string.IsNullOrWhiteSpace(kv.Value) && prefixesByKey.TryGetValue(kv.Key, out var prefix))
                prefix.LocalizedName = kv.Value;

        ApplyPlacedNameFallbacks(data);

        // Refresh the flattened dictionaries used by the UI and scripting.
        NpcDisplayNameById.Clear();
        foreach (var npc in NpcById.Values)
            NpcDisplayNameById[npc.Id] = npc.DisplayName;
        PrefixDisplayById.Clear();
        foreach (var prefix in _prefixDataList)
            PrefixDisplayById[(byte)prefix.Id] = prefix.DisplayName;

        BuildFrameNameIndexes();

        foreach (var tile in Tiles)
        {
            if (tile.Key == null || tile.Frames == null)
                continue;

            Dictionary<string, string>? frameNames = null;
            if (data.Frames != null)
                data.Frames.TryGetValue(tile.Key, out frameNames);
            foreach (var frame in tile.Frames)
            {
                frame.LocalizedDisplayName = ResolveFrameDisplayName(tile, frame, data, frameNames);
            }
        }
    }

    private void ResetLocalizationState()
    {
        foreach (var item in Items)
            item.LocalizedName = null;
        foreach (var tile in Tiles)
        {
            tile.LocalizedName = null;
            if (tile.Frames == null) continue;
            foreach (var frame in tile.Frames)
                frame.LocalizedDisplayName = null;
        }
        foreach (var air in TileBricks.Where(tile => tile.Id == -1))
            air.LocalizedName = null;
        foreach (var wall in Walls)
            wall.LocalizedName = null;
        foreach (var npc in NpcById.Values)
            npc.LocalizedName = null;
        foreach (var prefix in _prefixDataList)
            prefix.LocalizedName = null;

        _localizedItemNamesBySourceName.Clear();
        _localizedTileNamesBySourceName.Clear();
        _localizedWallNamesBySourceName.Clear();
        _tilesByKey.Clear();
        _wallsByKey.Clear();
        NpcDisplayNameById.Clear();
        PrefixDisplayById.Clear();
    }

    /// <summary>
    /// Localized names are not always present in the official tile/wall sections
    /// (the game derives them from item names). Fill the residual gaps with the
    /// placing item and supplemental names without touching valid official names.
    /// </summary>
    private void ApplyPlacedNameFallbacks(DataModel.LocalizationData data)
    {
        foreach (var air in TileBricks.Where(tile => tile.Id == -1))
        {
            if (air.Key != null
                && data.Tiles.TryGetValue(air.Key, out var airName)
                && !string.IsNullOrWhiteSpace(airName))
            {
                air.LocalizedName = airName;
            }
        }

        foreach (var tile in Tiles)
        {
            if (!string.IsNullOrWhiteSpace(tile.LocalizedName) || tile.Key == null)
                continue;

            if (data.Items.TryGetValue(tile.Key, out var itemName) && !string.IsNullOrWhiteSpace(itemName))
            {
                tile.LocalizedName = itemName;
                continue;
            }

            if (data.SupplementalNames.TryGetValue(tile.Key, out var supplemental)
                && !string.IsNullOrWhiteSpace(supplemental))
            {
                tile.LocalizedName = supplemental;
                continue;
            }

            if (GetPlacingItemName(TileItemLookup, tile.Id) is { } placingItem)
                tile.LocalizedName = placingItem;
        }

        foreach (var wall in Walls)
        {
            if (!string.IsNullOrWhiteSpace(wall.LocalizedName) || wall.Key == null)
                continue;

            if (data.Items.TryGetValue(wall.Key, out var itemName) && !string.IsNullOrWhiteSpace(itemName))
            {
                wall.LocalizedName = itemName;
                continue;
            }

            if (data.Items.TryGetValue(wall.Key + "Wall", out var wallItemName) && !string.IsNullOrWhiteSpace(wallItemName))
            {
                wall.LocalizedName = wallItemName;
                continue;
            }

            if (GetPlacingItemName(WallItemLookup, wall.Id) is { } placingItem)
                wall.LocalizedName = placingItem;
        }

        ApplyWallVariantMarkers(data);
    }

    /// <summary>
    /// Wall data contains synthetic " (natural)" / " (unused)" variants that have no
    /// game name of their own. Reuse the base wall's translation plus a localized marker.
    /// </summary>
    private void ApplyWallVariantMarkers(DataModel.LocalizationData data)
    {
        if (!data.SupplementalNames.TryGetValue("Wall.Natural", out var naturalMarker)
            || string.IsNullOrWhiteSpace(naturalMarker)
            || !data.SupplementalNames.TryGetValue("Wall.Unused", out var unusedMarker)
            || string.IsNullOrWhiteSpace(unusedMarker))
        {
            return;
        }

        var baseLocalizedByName = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var wall in Walls)
        {
            if (!string.IsNullOrWhiteSpace(wall.LocalizedName))
                baseLocalizedByName.TryAdd(wall.Name, wall.LocalizedName);
        }

        foreach (var wall in Walls)
        {
            string marker;
            if (wall.Name.EndsWith(" (natural)", StringComparison.Ordinal))
                marker = naturalMarker;
            else if (wall.Name.EndsWith(" (unused)", StringComparison.Ordinal))
                marker = unusedMarker;
            else
                continue;

            if (string.IsNullOrWhiteSpace(wall.LocalizedName)
                || wall.LocalizedName.Contains(marker, StringComparison.Ordinal))
            {
                continue;
            }

            var baseEnglish = wall.Name[..wall.Name.LastIndexOf(" (", StringComparison.Ordinal)].TrimEnd();
            if (baseLocalizedByName.TryGetValue(baseEnglish, out var baseLocalized)
                && !string.IsNullOrWhiteSpace(baseLocalized)
                && !string.Equals(baseLocalized, baseEnglish, StringComparison.Ordinal))
            {
                wall.LocalizedName = baseLocalized + marker;
            }
        }
    }

    private string? GetPlacingItemName(Dictionary<int, int> lookup, int tileOrWallId)
        => lookup.TryGetValue(tileOrWallId, out var itemId)
            && ItemById.TryGetValue(itemId, out var item)
            && !string.IsNullOrWhiteSpace(item.LocalizedName)
                ? item.LocalizedName
                : null;

    private void BuildFrameNameIndexes()
    {
        _localizedItemNamesBySourceName.Clear();
        foreach (var group in Items
                     .Where(item => !string.IsNullOrWhiteSpace(item.LocalizedName))
                     .GroupBy(item => item.Name, StringComparer.Ordinal))
        {
            var translations = group
                .Select(item => item.LocalizedName!)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (translations.Count == 1)
                _localizedItemNamesBySourceName[group.Key] = translations[0];
        }

        _localizedTileNamesBySourceName.Clear();
        foreach (var group in Tiles
                     .Where(tile => !string.IsNullOrWhiteSpace(tile.LocalizedName))
                     .GroupBy(tile => tile.Name, StringComparer.Ordinal))
        {
            var translations = group
                .Select(tile => tile.LocalizedName!)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (translations.Count == 1)
                _localizedTileNamesBySourceName[group.Key] = translations[0];
        }

        _localizedWallNamesBySourceName.Clear();
        foreach (var group in Walls
                     .Where(wall => !string.IsNullOrWhiteSpace(wall.LocalizedName))
                     .GroupBy(wall => wall.Name, StringComparer.Ordinal))
        {
            var translations = group
                .Select(wall => wall.LocalizedName!)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (translations.Count == 1)
                _localizedWallNamesBySourceName[group.Key] = translations[0];
        }
    }

    /// <summary>
    /// Builds the complete localized label for a frame. Exact per-tile overrides win;
    /// otherwise the label is composed from item/tile translations and supplemental
    /// style/anchor names. When the base name has no reliable translation the raw
    /// source label is preserved as a whole.
    /// </summary>
    private string? ResolveFrameDisplayName(
        TileProperty tile,
        FrameProperty frame,
        DataModel.LocalizationData data,
        Dictionary<string, string>? frameOverrides)
    {
        if (frameOverrides != null
            && frameOverrides.TryGetValue(frame.ToString(), out var exact)
            && !string.IsNullOrWhiteSpace(exact))
        {
            return exact;
        }

        // Banners, paintings and boss trophies already have complete item names;
        // composing "Name: Variety" would duplicate the noun.
        if (!string.IsNullOrWhiteSpace(frame.Variety))
        {
            if (frame.Name == "Boss Trophy"
                && TryResolveItemName(frame.Variety + " Trophy", out var trophyName))
            {
                return trophyName;
            }

            var isPaintingTile = tile.Key?.StartsWith("Painting", StringComparison.Ordinal) == true;
            if (TryResolveItemName(frame.Variety, out var varietyItem)
                && (frame.Name.EndsWith("Banner", StringComparison.OrdinalIgnoreCase)
                    || frame.Name == "Painting"
                    || (isPaintingTile && frame.Name != "Boss Trophy")))
            {
                return varietyItem;
            }
        }

        var baseName = ResolveFrameBaseName(tile, frame.Name, data);
        if (baseName == null)
            return null;

        var result = new StringBuilder(baseName);
        if (!string.IsNullOrWhiteSpace(frame.Variety))
        {
            var variety = ResolveFrameVariety(frame.Variety, data);
            if (variety == null)
            {
                // Do not produce a half-translated label: keep the whole source label.
                return null;
            }

            result.Append(": ");
            result.Append(variety);
        }

        if (frame.Anchor != FrameAnchor.None)
        {
            result.Append(" [");
            result.Append(ResolveAnchorName(frame.Anchor, data));
            result.Append(']');
        }

        return result.ToString();
    }

    private string? ResolveFrameBaseName(
        TileProperty tile,
        string name,
        DataModel.LocalizationData data)
    {
        if (string.IsNullOrWhiteSpace(name)
            || name == "Default"
            || name == tile.Name
            || name == tile.Key
            || name.All(char.IsDigit))
        {
            return string.IsNullOrWhiteSpace(tile.LocalizedName) ? null : tile.LocalizedName;
        }

        if (_localizedItemNamesBySourceName.TryGetValue(name, out var itemName))
            return itemName;
        if (_localizedTileNamesBySourceName.TryGetValue(name, out var tileName))
            return tileName;
        if (data.SupplementalNames.TryGetValue(name, out var supplemental)
            && !string.IsNullOrWhiteSpace(supplemental))
        {
            return supplemental;
        }

        return null;
    }

    private string? ResolveFrameVariety(string variety, DataModel.LocalizationData data)
    {
        if (_localizedItemNamesBySourceName.TryGetValue(variety, out var itemName))
            return itemName;
        if (data.SupplementalNames.TryGetValue(variety, out var supplemental)
            && !string.IsNullOrWhiteSpace(supplemental))
        {
            return supplemental;
        }

        // Compound style names such as "Flat A" reuse the localized base plus the suffix.
        var lastSpace = variety.LastIndexOf(' ');
        if (lastSpace > 0)
        {
            var basePart = variety[..lastSpace];
            var suffix = variety[lastSpace..];
            if (_localizedItemNamesBySourceName.TryGetValue(basePart, out var baseItem))
                return baseItem + suffix;
            if (data.SupplementalNames.TryGetValue(basePart, out var baseStyle)
                && !string.IsNullOrWhiteSpace(baseStyle))
            {
                return baseStyle + suffix;
            }
        }

        return null;
    }

    private bool TryResolveItemName(string sourceName, out string localizedName)
    {
        if (_localizedItemNamesBySourceName.TryGetValue(sourceName, out var resolved))
        {
            localizedName = resolved;
            return true;
        }

        localizedName = sourceName;
        return false;
    }

    private string ResolveAnchorName(FrameAnchor anchor, DataModel.LocalizationData data)
    {
        var key = $"Anchor.{anchor}";
        return data.SupplementalNames.TryGetValue(key, out var label) && !string.IsNullOrWhiteSpace(label)
            ? label
            : anchor.ToString();
    }

    /// <summary>Gets the localized display label for a frame, falling back to the raw label.</summary>
    public string GetLocalizedFrameName(TileProperty tile, FrameProperty frame)
    {
        if (Localization is not { } data)
            return frame.ToString();

        Dictionary<string, string>? frameOverrides = null;
        if (tile.Key != null && data.Frames != null)
            data.Frames.TryGetValue(tile.Key, out frameOverrides);

        return ResolveFrameDisplayName(tile, frame, data, frameOverrides) ?? frame.ToString();
    }

    /// <summary>Resolves a supplemental sprite-style / biome-variant label.</summary>
    public string GetLocalizedSpriteStyle(string name)
    {
        if (Localization is not { } data || string.IsNullOrWhiteSpace(name))
            return name;

        if (_localizedItemNamesBySourceName.TryGetValue(name, out var itemName))
            return itemName;
        if (_tilesByKey.TryGetValue(name, out var tile) && !string.IsNullOrWhiteSpace(tile.LocalizedName))
            return tile.LocalizedName;
        if (_wallsByKey.TryGetValue(name, out var wall) && !string.IsNullOrWhiteSpace(wall.LocalizedName))
            return wall.LocalizedName;
        if (_localizedTileNamesBySourceName.TryGetValue(name, out var tileName))
            return tileName;
        if (_localizedWallNamesBySourceName.TryGetValue(name, out var wallName))
            return wallName;
        if (data.SupplementalNames.TryGetValue(name, out var supplemental)
            && !string.IsNullOrWhiteSpace(supplemental))
        {
            return supplemental;
        }

        return name;
    }

    /// <summary>Gets the localized name of a frame anchor (e.g. "Left").</summary>
    public string GetLocalizedAnchor(FrameAnchor anchor)
        => Localization is { } data ? ResolveAnchorName(anchor, data) : anchor.ToString();

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
}
