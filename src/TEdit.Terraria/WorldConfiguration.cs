using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TEdit.Common;
using TEdit.Terraria.DataModel;
using TEdit.Geometry;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria;

public class WorldConfiguration
{
    // Baseline fallbacks ONLY (used if config files are missing/broken).
    // Baselines use v1.4.5.4 (latest of 02Feb26).
    private const uint  DefaultCompatibleVersion = 317;
    private const short DefaultTileCount         = 752;
    private const short DefaultWallCount         = 366;
    private const short DefaultMaxNpcId          = 696;
    private const int   DefaultMaxChests         = 8000;
    private const int   DefaultMaxSigns          = 32767;
    private const int   DefaultCavernToBottom    = 478;
    private const byte  DefaultMaxMoons          = 9;

    public static uint  CompatibleVersion          { get; private set; } = DefaultCompatibleVersion;

    public static short TileCount                  { get; private set; } = DefaultTileCount;
    public static short WallCount                  { get; private set; } = DefaultWallCount;
    public static short MaxNpcID                   { get; private set; } = DefaultMaxNpcId;

    public static int   MaxChests                  { get; private set; } = DefaultMaxChests;
    public static int   MaxSigns                   { get; private set; } = DefaultMaxSigns;
    public static int   CavernLevelToBottomOfWorld { get; private set; } = DefaultCavernToBottom;
    public static byte  MaxMoons                   { get; private set; } = DefaultMaxMoons; // property, not field

    public static bool[] SettingsTileFrameImportant { get; private set; }

    public static SaveVersionManager SaveConfiguration { get; private set; }
    public static BestiaryConfiguration BestiaryData { get; private set; }
    public static MorphConfiguration MorphSettings { get; private set; }

    // Tracks what we actually applied (handy for UI / warnings).
    public static uint ActiveWorldVersion  { get; private set; }
    public static uint ActiveConfigVersion { get; private set; }

    private static bool _initialized;
    private static readonly object _initLock = new();

    public const string DesktopHeader = "relogic";
    public const string ChineseHeader = "xindong";

    public static List<string>            Biomes    => MorphSettings.Biomes.Keys.ToList();
    public static Dictionary<string, int> MossTypes => MorphSettings.MossTypes;

    private static readonly Dictionary<string, TEditColor> _globalColors = new Dictionary<string, TEditColor>();
    private static readonly Dictionary<string, int> _npcIds = new Dictionary<string, int>();
    private static readonly Dictionary<int, Vector2Short> _npcFrames = new Dictionary<int, Vector2Short>();
    private static readonly Dictionary<byte, string> _prefix = new Dictionary<byte, string>();
    private static readonly Dictionary<int, ItemProperty> _itemLookup = new Dictionary<int, ItemProperty>();
    private static readonly Dictionary<int, string> _tallynames = new Dictionary<int, string>();
    private static readonly Dictionary<string, string> _frameNames = new Dictionary<string, string>();
    private static readonly Dictionary<int, string> _armorHeadNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _foodNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _kiteNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _critterNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _accessoryNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _dyeNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _armorBodyNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _armorLegsNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _rackable = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _mountNames = new Dictionary<int, string>();


    private static readonly ObservableCollection<ItemProperty> _itemProperties = new ObservableCollection<ItemProperty>();
    private static readonly ObservableCollection<ChestProperty> _chestProperties = new ObservableCollection<ChestProperty>();
    private static readonly ObservableCollection<SignProperty> _signProperties = new ObservableCollection<SignProperty>();
    private static readonly ObservableCollection<TileProperty> _tileProperties = new ObservableCollection<TileProperty>();
    private static readonly ObservableCollection<TileProperty> _tileBricks = new ObservableCollection<TileProperty>();
    private static readonly ObservableCollection<TileProperty> _tileBricksMask = new ObservableCollection<TileProperty>();
    private static readonly ObservableCollection<WallProperty> _wallProperties = new ObservableCollection<WallProperty>();
    private static readonly ObservableCollection<WallProperty> _wallPropertiesMask = new ObservableCollection<WallProperty>();
    private static readonly ObservableCollection<PaintProperty> _paintProperties = new ObservableCollection<PaintProperty>();

    private static TerrariaDataStore _store;

    static WorldConfiguration()
    {
        // Keep static ctor, but route everything through Initialize()
        // so you can also call it explicitly from App startup.
        Initialize();
    }

    /// <summary>
    /// Initialize WorldConfiguration from embedded JSON resources via TerrariaDataStore.
    /// Safe to call multiple times.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;

            try
            {
                // Load data from TerrariaDataStore
                _store = TerrariaDataStore.Initialize();
                PopulateFromStore(_store);

                // Apply MAX config version at startup
                ApplyForConfigMax();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WorldConfiguration.Initialize failed: {ex}");
                System.Diagnostics.Trace.WriteLine($"WorldConfiguration.Initialize failed: {ex}");
                // Rethrow so initialization failures are visible
                throw;
            }

            _initialized = true;
        }
    }

    /// <summary>
    /// Populate WorldConfiguration from TerrariaDataStore data.
    /// </summary>
    private static void PopulateFromStore(TerrariaDataStore store)
    {
        ClearCollections();

        SaveConfiguration = store.VersionManager;
        MorphSettings = store.Morphs;

        // Build BestiaryConfiguration from store data
        if (store.Bestiary != null)
        {
            var npcById = new Dictionary<int, BestiaryNpcData>();
            var npcData = new Dictionary<string, BestiaryNpcData>();
            foreach (var npc in store.Bestiary.NpcData)
            {
                npcById[npc.Id] = npc;
                npcData[npc.BestiaryId] = npc;
            }
            BestiaryData = BestiaryConfiguration.FromBridge(
                configuration: store.Bestiary,
                npcById: npcById,
                npcData: npcData,
                bestiaryTalkedIDs: store.BestiaryTalkedIDs.ToList(),
                bestiaryNearIDs: store.BestiaryNearIDs.ToList(),
                bestiaryKilledIDs: store.BestiaryKilledIDs.ToList());
        }

        // Add tiles directly (no conversion needed - already TileProperty)
        var airBrick = new TileProperty { Id = -1, Name = "空气", Color = TEditColor.Transparent };
        _tileBricks.Add(airBrick);
        _tileBricksMask.Add(airBrick);

        foreach (var tile in store.Tiles)
        {
            _tileProperties.Add(tile);
            if (!tile.IsFramed)
            {
                _tileBricks.Add(tile);
                _tileBricksMask.Add(tile);
            }
        }

        // Pad tiles up to 255 with magenta framed placeholders
        for (int i = _tileProperties.Count; i < 255; i++)
        {
            _tileProperties.Add(new TileProperty { Id = i, Color = TEditColor.Magenta, IsFramed = true });
        }

        // Populate NotSolidTiles for morph operations
        MorphConfiguration.NotSolidTiles.Clear();
        foreach (var tile in _tileProperties)
        {
            if (!tile.IsSolid)
            {
                MorphConfiguration.NotSolidTiles.Add((ushort)tile.Id);
            }
        }

        // Add walls directly (no conversion needed - already WallProperty)
        foreach (var wall in store.Walls)
        {
            _wallProperties.Add(wall);
            _wallPropertiesMask.Add(wall);
        }

        // Add items directly and build lookups (no conversion needed - already ItemProperty)
        foreach (var item in store.Items)
        {
            _itemProperties.Add(item);
            _itemLookup[item.Id] = item;

            if (item.Tally > 0) _tallynames[item.Tally] = item.Name;
            if (item.Head >= 0) _armorHeadNames[item.Id] = item.Name;
            if (item.Body >= 0) _armorBodyNames[item.Id] = item.Name;
            if (item.Legs >= 0) _armorLegsNames[item.Id] = item.Name;
            if (item.IsRackable) _rackable[item.Id] = item.Name;
            if (item.IsFood) _foodNames[item.Id] = item.Name;
            if (item.IsCritter) _critterNames[item.Id] = item.Name;
            if (item.IsKite) _kiteNames[item.Id] = item.Name;
            if (item.IsAccessory) _accessoryNames[item.Id] = item.Name;
            if (item.IsMount) _mountNames[item.Id] = item.Name;
            if (item.Name.Contains("Dye")) _dyeNames[item.Id] = item.Name;
        }

        // Add paints directly (already PaintProperty)
        foreach (var paint in store.Paints)
            _paintProperties.Add(paint);

        // Add chests directly (already ChestProperty)
        foreach (var chest in store.Chests)
            _chestProperties.Add(chest);

        // Add signs directly (already SignProperty)
        foreach (var sign in store.Signs)
            _signProperties.Add(sign);

        // Copy dictionaries from store
        foreach (var kv in store.GlobalColors) _globalColors[kv.Key] = kv.Value;
        foreach (var kv in store.NpcIdByName) _npcIds[kv.Key] = kv.Value;
        foreach (var kv in store.NpcNameById) _npcNames[kv.Key] = kv.Value;
        foreach (var kv in store.NpcFrames) _npcFrames[kv.Key] = kv.Value;
        foreach (var kv in store.PrefixById) _prefix[kv.Key] = kv.Value;
    }

    private static void ClearCollections()
    {
        _tileProperties.Clear();
        _tileBricks.Clear();
        _tileBricksMask.Clear();
        _wallProperties.Clear();
        _wallPropertiesMask.Clear();
        _itemProperties.Clear();
        _paintProperties.Clear();
        _chestProperties.Clear();
        _signProperties.Clear();
        _globalColors.Clear();
        _npcIds.Clear();
        _npcNames.Clear();
        _npcFrames.Clear();
        _prefix.Clear();
        _itemLookup.Clear();
        _tallynames.Clear();
        _frameNames.Clear();
        _armorHeadNames.Clear();
        _armorBodyNames.Clear();
        _armorLegsNames.Clear();
        _foodNames.Clear();
        _critterNames.Clear();
        _kiteNames.Clear();
        _accessoryNames.Clear();
        _mountNames.Clear();
        _dyeNames.Clear();
        _rackable.Clear();
    }

    /// <summary>
    /// Reset initialization state (primarily for testing).
    /// </summary>
    public static void Reset()
    {
        lock (_initLock)
        {
            ClearCollections();
            SaveConfiguration = null;
            BestiaryData = null;
            MorphSettings = null;
            SettingsTileFrameImportant = null;
            _initialized = false;
        }
    }

    private static void ApplyForConfigMax()
    {
        if (SaveConfiguration == null)
            return;

        var maxCfg = SaveConfiguration.GetMaxVersion();
        if (maxCfg > 0)
        {
            // Apply using the config's max version (startup baseline).
            ApplyConfigVersion((uint)maxCfg, worldVersion: (uint)maxCfg);
        }
    }

    /// <summary>
    /// Update limits for a specific WORLD version, clamping to the max config version if needed.
    /// Call this when a world is being loaded (optional).
    /// </summary>
    public static bool ApplyForWorldVersion(uint worldVersion, out uint configVersionUsed)
    {
        Initialize();

        configVersionUsed = 0;
        if (SaveConfiguration == null)
            return false;

        var maxCfg = SaveConfiguration.GetMaxVersion();
        if (maxCfg <= 0)
            return false;

        var maxCfgU = (uint)maxCfg;

        // This is the true "max supported by config right now".
        CompatibleVersion = maxCfgU;

        var chosen = worldVersion > maxCfgU ? maxCfgU : worldVersion;

        // If GetData throws for some versions, fall back to max config.
        try
        {
            SaveConfiguration.GetData((int)chosen);
        }
        catch
        {
            chosen = maxCfgU;
        }

        ApplyConfigVersion(chosen, worldVersion);
        configVersionUsed = chosen;
        return true;
    }

    /// <summary>
    /// Applies a specific CONFIG version to the static limits.
    /// </summary>
    private static void ApplyConfigVersion(uint configVersion, uint worldVersion)
    {
        var data = SaveConfiguration.GetData((int)configVersion);

        ActiveWorldVersion  = worldVersion;
        ActiveConfigVersion = configVersion;

        CompatibleVersion = (uint)SaveConfiguration.GetMaxVersion();

        TileCount = (short)data.MaxTileId;
        WallCount = (short)data.MaxWallId;
        MaxNpcID  = (short)data.MaxNpcId;

        // OPTIONAL: Only if your JSON/version data actually contains these fields.
        // If it doesn't, keep the defaults.
        TryApplyOptionalInts(data);

        RebuildFrameImportant();
    }

    private static void RebuildFrameImportant()
    {
        // Rebuild to match TileCount and avoid out-of-range.
        var arr = new bool[TileCount + 1];

        // TileProperties must already be loaded from settings.xml for this to be useful.
        // Clamp to what we actually have.
        var limit = Math.Min(TileCount, (short)(TileProperties.Count - 1));
        for (int i = 0; i <= limit; i++)
            arr[i] = TileProperties[i].IsFramed;

        SettingsTileFrameImportant = arr;
    }

    private static void TryApplyOptionalInts(object data)
    {
        // If these properties exist on your version-data model, apply them.
        // If not, just keep existing defaults.

        if (TryGetInt(data, out var maxChests, "MaxChests", "MaxChestCount") && maxChests > 0)
            MaxChests = maxChests;

        if (TryGetInt(data, out var maxSigns, "MaxSigns", "MaxSignCount") && maxSigns > 0)
            MaxSigns = maxSigns;

        if (TryGetInt(data, out var cavern, "CavernLevelToBottomOfWorld", "CavernToBottom") && cavern > 0)
            CavernLevelToBottomOfWorld = cavern;

        if (TryGetInt(data, out var moons, "MaxMoons") && moons > 0 && moons <= byte.MaxValue)
            MaxMoons = (byte)moons;
    }

    private static bool TryGetInt(object obj, out int value, params string[] names)
    {
        value = 0;
        if (obj == null) return false;

        var t = obj.GetType();
        foreach (var n in names)
        {
            var p = t.GetProperty(n);
            if (p == null || !p.CanRead) continue;

            var v = p.GetValue(obj);
            if (v == null) continue;

            try
            {
                value = Convert.ToInt32(v);
                return true;
            }
            catch { }
        }
        return false;
    }

    public static TileProperty GetBrickFromColor(byte a, byte r, byte g, byte b)
    {
        for (int i = 0; i < TileBricks.Count; i++)
        {
            var curBrick = TileBricks[i];
            var aB = curBrick.Color.A;
            var rB = curBrick.Color.R;
            var gB = curBrick.Color.G;
            var bB = curBrick.Color.B;
            if (r == rB && g == gB && b == bB)
                return curBrick;
        }

        return null;
    }

    public static WallProperty GetWallFromColor(byte a, byte r, byte g, byte b)
    {
        // if it is a global color, skip
        foreach (var global in GlobalColors)
        {
            var aB = global.Value.A;
            var rB = global.Value.R;
            var gB = global.Value.G;
            var bB = global.Value.B;
            if (r == rB && g == gB && b == bB)
                return null;
        }
        for (int i = 0; i < WallProperties.Count; i++)
        {
            var curBrick = WallProperties[i];
            var aB = curBrick.Color.A;
            var rB = curBrick.Color.R;
            var gB = curBrick.Color.G;
            var bB = curBrick.Color.B;
            if (r == rB && g == gB && b == bB)
                return curBrick;
        }
        return null;
    }

    public static Dictionary<string, TEditColor> GlobalColors
    {
        get { return _globalColors; }
    }

    public static Dictionary<string, int> NpcIds
    {
        get { return _npcIds; }
    }

    private static Dictionary<int, string> _npcNames = new Dictionary<int, string>();
    public static Dictionary<int, string> NpcNames
    {
        get { return _npcNames; }
    }

    public static Dictionary<int, string> ArmorHeadNames
    {
        get { return _armorHeadNames; }
    }


    public static Dictionary<int, string> MountNames
    {
        get { return _mountNames; }
    }

    public static Dictionary<int, string> AccessoryNames
    {
        get { return _accessoryNames; }
    }

    public static Dictionary<int, string> FoodNames
    {
        get { return _foodNames; }
    }

    public static Dictionary<int, string> CritterNames
    {
        get { return _critterNames; }
    }

    public static Dictionary<int, string> KiteNames
    {
        get { return _kiteNames; }
    }

    public static Dictionary<int, string> ArmorBodyNames
    {
        get { return _armorBodyNames; }
    }
    public static Dictionary<int, string> ArmorLegsNames
    {
        get { return _armorLegsNames; }
    }

    public static Dictionary<int, string> DyeNames
    {
        get { return _dyeNames; }
    }
    public static Dictionary<int, string> Rackable
    {
        get { return _rackable; }
    }

    public static Dictionary<int, Vector2Short> NpcFrames
    {
        get { return _npcFrames; }
    }

    public static Dictionary<byte, string> ItemPrefix
    {
        get { return _prefix; }
    }



    public static ObservableCollection<TileProperty> TileProperties
    {
        get { return _tileProperties; }
    }

    public static TileProperty GetTileProperties(int type)
    {
        if (_tileProperties.Count > type)
        {
            return _tileProperties[type];
        }
        return _tileProperties[0];
    }

    public static ObservableCollection<TileProperty> TileBricks
    {
        get { return _tileBricks; }
    }

    public static ObservableCollection<WallProperty> WallProperties
    {
        get { return _wallProperties; }
    }

    public static ObservableCollection<TileProperty> TileBricksMask
    {
        get { return _tileBricksMask; }
    }

    public static ObservableCollection<WallProperty> WallPropertiesMask
    {
        get { return _wallPropertiesMask; }
    }

    public static ObservableCollection<PaintProperty> PaintProperties
    {
        get { return _paintProperties; }
    }

    public static ObservableCollection<ItemProperty> ItemProperties
    {
        get { return _itemProperties; }
    }

    public static ObservableCollection<ChestProperty> ChestProperties
    {
        get { return _chestProperties; }
    }

    public static ObservableCollection<SignProperty> SignProperties
    {
        get { return _signProperties; }
    }

    public static Dictionary<int, ItemProperty> ItemLookupTable
    {
        get { return _itemLookup; }
    }

    public static ObservableCollection<SpriteSheet> Sprites2 { get; } = new ObservableCollection<SpriteSheet>();

    /// <summary>
    /// Lock object for synchronizing access to Sprites2 across threads.
    /// Use BindingOperations.EnableCollectionSynchronization with this lock in WPF apps.
    /// </summary>
    public static readonly object Sprites2Lock = new();

    public static Dictionary<int, string> TallyNames
    {
        get { return _tallynames; }
    }

    public static Dictionary<string, string> FrameNames
    {
        get { return _frameNames; }
    }


    public static string GetFrameNameKey(int id, short u, short v)
    {
        return id + ":" + u + "," + v;
    }
}
