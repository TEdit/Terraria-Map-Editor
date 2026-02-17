using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TEdit.Common;
using TEdit.Geometry;
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

    // Localization
    public DataModel.LocalizationData? Localization { get; private set; }
    public string CurrentLocale { get; private set; } = "en-US";

    // Version management
    public DataModel.SaveVersionManager? VersionManager { get; private set; }
    public BestiaryNpcConfiguration? Bestiary { get; private set; }
    public DataModel.MorphConfiguration? Morphs { get; private set; }
    public DataModel.BackgroundStyleConfiguration? BackgroundStyles { get; private set; }

    // Prefix data for key resolution during localization
    private List<PrefixData> _prefixDataList = [];

    // Bestiary-derived lookups
    public Dictionary<int, BestiaryNpcData> BestiaryNpcById { get; } = new();
    public Dictionary<string, BestiaryNpcData> BestiaryNpcByBestiaryId { get; } = new();
    public List<string> BestiaryTalkedIDs { get; } = [];
    public List<string> BestiaryNearIDs { get; } = [];
    public List<string> BestiaryKilledIDs { get; } = [];

    // Version-dependent limits
    public int TileCount { get; private set; } = 752;
    public int WallCount { get; private set; } = 366;
    public int MaxNpcId { get; private set; } = 696;
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

        // Load background styles
        try
        {
            using var stream = JsonDataLoader.GetDataStream("backgroundStyles.json", dataPath);
            BackgroundStyles = DataModel.BackgroundStyleConfiguration.Load(stream);
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
            if (item.Name.Contains("Dye")) DyeItems[item.Id] = item;
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

        // Apply item names
        foreach (var kv in data.Items)
            if (itemsByKey.TryGetValue(kv.Key, out var item))
                item.Name = kv.Value;

        // Apply tile names
        foreach (var kv in data.Tiles)
            if (tilesByKey.TryGetValue(kv.Key, out var tile))
                tile.Name = kv.Value;

        // Apply wall names
        foreach (var kv in data.Walls)
            if (wallsByKey.TryGetValue(kv.Key, out var wall))
                wall.Name = kv.Value;

        // Apply NPC names (+ maintain reverse lookups)
        foreach (var kv in data.Npcs)
        {
            if (npcsByKey.TryGetValue(kv.Key, out var npc))
            {
                var oldName = npc.Name;
                npc.Name = kv.Value;
                NpcNameById[npc.Id] = kv.Value;
                if (NpcIdByName.Remove(oldName))
                    NpcIdByName[kv.Value] = npc.Id;
            }
        }

        // Apply prefix names
        foreach (var kv in data.Prefixes)
            if (prefixKeyToId.TryGetValue(kv.Key, out var id))
                PrefixById[id] = kv.Value;
    }

    private void RebuildFrameImportant()
    {
        var count = TileCount + 1;
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
        TileCount = data.MaxTileId;
        WallCount = data.MaxWallId;
        MaxNpcId = data.MaxNpcId;
        RebuildFrameImportant();
    }
}
