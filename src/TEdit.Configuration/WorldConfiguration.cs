using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using TEdit.Terraria.Objects;
using System.Linq;
using TEdit.Geometry;
using TEdit.Common;
using TEdit.Configuration.BiomeMorph;

namespace TEdit.Configuration;

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

    public static List<KeyValuePair<string, string>> Biomes =>
        MorphSettings.Biomes.Keys
            .Select(k => new KeyValuePair<string, string>(k, BiomeDisplayName.Get(k)))
            .ToList();

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

    static WorldConfiguration()
    {
        // Keep static ctor, but route everything through Initialize()
        // so you can also call it explicitly from App startup.
        Initialize();
    }

    /// <summary>
    /// Loads configuration files and applies the MAX config version so all static limits
    /// (CompatibleVersion/TileCount/WallCount/etc.) are correct at app startup.
    /// Safe to call multiple times.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;

            // ---- Load configs (same as you already do) ----
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var saveVersionPath = Path.Combine(baseDir, "TerrariaVersionTileData.json");
            if (File.Exists(saveVersionPath))
                SaveConfiguration = SaveVersionManager.LoadJson(saveVersionPath);

            var settingspath = Path.Combine(baseDir, "settings.xml");
            if (File.Exists(settingspath))
                LoadObjectDbXml(settingspath);

            var bestiaryDataPath = Path.Combine(baseDir, "npcData.json");
            if (File.Exists(bestiaryDataPath))
                BestiaryData = BestiaryConfiguration.LoadJson(bestiaryDataPath);

            try
            {
                var morphPath = Path.Combine(baseDir, "morphSettings.json");
                if (File.Exists(morphPath))
                    MorphSettings = MorphConfiguration.LoadJson(morphPath);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Invalid morphSettings.json", ex);
            }

            // ---- Apply MAX config version at startup ----
            try
            {
                ApplyForConfigMax();
            }
            catch (Exception ex)
            {
                // IMPORTANT: do NOT swallow silently or you’ll never know why defaults “stick”.
                // Use your project’s logger here:
                // ErrorLogging.LogException(ex);
                System.Diagnostics.Debug.WriteLine(ex);
            }

            _initialized = true;
        }
    }

    private static void ApplyForConfigMax()
    {
        if (SaveConfiguration == null)
            return;

        var maxCfg = SaveConfiguration.GetMaxVersion();
        if (maxCfg > 0)
        {
            // Apply using the config’s max version (startup baseline).
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

    private static IEnumerable<TOut> StringToList<TOut>(string xmlcsv)
    {
        if (!string.IsNullOrWhiteSpace(xmlcsv))
        {
            string[] split = xmlcsv.Split(',');
            foreach (var s in split)
            {
                yield return (TOut)Convert.ChangeType(s, typeof(TOut));
            }
        }
    }

    private static T InLineEnumTryParse<T>(string str) where T : struct
    {
        T result;

        Enum.TryParse(str, true, out result);
        return result;
    }

    private static Vector2Short[] StringToVector2ShortArray(string v, short defaultX = 0, short defaultY = 0)
    {

        if (!string.IsNullOrWhiteSpace(v))
        {
            if (!v.StartsWith("["))
            {
                return new Vector2Short[] { StringToVector2Short(v, defaultX, defaultY) };
            }

            v = v.Trim(new[] { '[', ']' });
            var items = v.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var result = new Vector2Short[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                result[i] = StringToVector2Short(items[i], defaultX, defaultY);
            }

            if (result.Length > 0)
            {
                return result;
            }
        }

        return new Vector2Short[] { new Vector2Short(defaultX, defaultY) };
    }

    private static Vector2Short StringToVector2Short(string v, short defaultX = 0, short defaultY = 0)
    {
        if (!string.IsNullOrWhiteSpace(v))
        {

            short x = 0;
            short y = 0;
            var split = v.Split(',');
            if (split.Length == 2)
            {
                if (short.TryParse(split[0], out x) && short.TryParse(split[1], out y))
                    return new Vector2Short(x, y);
            }
        }

        return new Vector2Short(defaultX, defaultY);
    }


    public static void LoadObjectDbXml(string file)
    {
        TileBricks.Add(new TileProperty
        {
            Id = -1,
            Name = "空气",
            Color = TEditColor.Transparent
        });

        TileBricksMask.Add(new TileProperty
        {
            Id = -1,
            Name = "空气",
            Color = TEditColor.Transparent
        });

        var xmlSettings = XElement.Load(file);

        // Load Colors
        foreach (var xElement in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
        {
            string name = (string)xElement.Attribute("Name");
            var color = TEditColor.FromString((string)xElement.Attribute("Color"));
            GlobalColors.Add(name, color);
        }

        foreach (var xElement in xmlSettings.Elements("Tiles").Elements("Tile"))
        {

            var curTile = new TileProperty();

            // Read XML attributes
            curTile.Color = TEditColor.FromString((string)xElement.Attribute("Color"));
            curTile.Name = (string)xElement.Attribute("Name");
            curTile.Id = (int?)xElement.Attribute("Id") ?? 0;
            curTile.IsFramed = (bool?)xElement.Attribute("Framed") ?? false;
            curTile.IsSolid = (bool?)xElement.Attribute("Solid") ?? false;
            curTile.SaveSlope = (bool?)xElement.Attribute("SaveSlope") ?? false;
            curTile.IsSolidTop = (bool?)xElement.Attribute("SolidTop") ?? false;
            curTile.IsLight = (bool?)xElement.Attribute("Light") ?? false;
            curTile.IsAnimated = (bool?)xElement.Attribute("IsAnimated") ?? false;
            curTile.FrameSize = StringToVector2ShortArray((string)xElement.Attribute("Size"), 1, 1);
            // curTile.FrameSize = StringToVector2Short((string)xElement.Attribute("Size"), 1, 1);
            curTile.Placement = InLineEnumTryParse<FramePlacement>((string)xElement.Attribute("Placement"));
            curTile.TextureGrid = StringToVector2Short((string)xElement.Attribute("TextureGrid"), 16, 16);
            curTile.FrameGap = StringToVector2Short((string)xElement.Attribute("FrameGap"), 2, 2);
            curTile.IsGrass = "Grass".Equals((string)xElement.Attribute("Special")); /* Heathtech */
            curTile.IsPlatform = "Platform".Equals((string)xElement.Attribute("Special")); /* Heathtech */
            curTile.IsCactus = "Cactus".Equals((string)xElement.Attribute("Special")); /* Heathtech */
            curTile.IsStone = (bool?)xElement.Attribute("Stone") ?? false; /* Heathtech */
            curTile.CanBlend = (bool?)xElement.Attribute("Blends") ?? false; /* Heathtech */
            curTile.MergeWith = (int?)xElement.Attribute("MergeWith") ?? null; /* Heathtech */
            curTile.FrameNameSuffix = (string)xElement.Attribute("FrameNameSuffix") ?? null;

            foreach (var elementFrame in xElement.Elements("Frames").Elements("Frame"))
            {
                var curFrame = new FrameProperty();
                // Read XML attributes
                curFrame.Name = (string)elementFrame.Attribute("Name");
                curFrame.Variety = (string)elementFrame.Attribute("Variety");
                curFrame.UV = StringToVector2Short((string)elementFrame.Attribute("UV"), 0, 0);
                curFrame.Anchor = InLineEnumTryParse<FrameAnchor>((string)elementFrame.Attribute("Anchor"));
                var frameSize = StringToVector2Short((string)elementFrame.Attribute("Size"), curTile.FrameSize[0].X, curTile.FrameSize[0].Y);
                curFrame.Size = frameSize;
                // Assign a default name if none existed
                if (string.IsNullOrWhiteSpace(curFrame.Name))
                    curFrame.Name = curTile.Name;

                curTile.Frames.Add(curFrame);
                string spriteName = null;
                if (curFrame.Name == curTile.Name)
                {
                    if (!string.IsNullOrWhiteSpace(curFrame.Variety))
                        spriteName += curFrame.Variety;
                }
                else
                {
                    spriteName += curFrame.Name;
                    if (!string.IsNullOrWhiteSpace(curFrame.Variety))
                        spriteName += " - " + curFrame.Variety;
                }
                //Sprites.Add(new Sprite
                //{
                //    Anchor = curFrame.Anchor,
                //    IsPreviewTexture = false,
                //    Name = spriteName,
                //    Origin = curFrame.UV,
                //    Size = frameSize,
                //    Tile = (ushort)curTile.Id, /* SBlogic */
                //    TileName = curTile.Name
                //});

            }
            if (curTile.IsFramed &&curTile.Frames.Count == 0)
            {
                var curFrame = new FrameProperty();
                // Read XML attributes
                curFrame.Name = curTile.Name;
                curFrame.Variety = string.Empty;
                curFrame.UV = new Vector2Short(0, 0);
                curFrame.Size = curTile.FrameSize[0];
                //curFrame.Anchor = InLineEnumTryParse<FrameAnchor>((string)xElement.Attribute("Anchor"));

                curTile.Frames.Add(curFrame);
                //Sprites.Add(new Sprite
                //{
                //    Anchor = curFrame.Anchor,
                //    IsPreviewTexture = false,
                //    Name = null,
                //    Origin = curFrame.UV,
                //    Size = curTile.FrameSize[0],
                //    Tile = (ushort)curTile.Id,
                //    TileName = curTile.Name
                //});
            }
            TileProperties.Add(curTile);
            if (!curTile.IsFramed)
            {
                TileBricks.Add(curTile);
                TileBricksMask.Add(curTile);
            }
        }

        for (int i = TileProperties.Count; i < 255; i++)
        {
            TileProperties.Add(new TileProperty
            {
                Id = i,
                Color = TEditColor.Magenta,
                IsFramed = true
            });
        }

        foreach (var xElement in xmlSettings.Elements("Walls").Elements("Wall"))
        {
            var curWall = new WallProperty();
            curWall.Color = TEditColor.FromString((string)xElement.Attribute("Color"));
            curWall.Name = (string)xElement.Attribute("Name");
            curWall.Id = (int?)xElement.Attribute("Id") ?? -1;
            WallProperties.Add(curWall);
            WallPropertiesMask.Add(curWall);
        }



        foreach (var xElement in xmlSettings.Elements("Items").Elements("Item"))
        {
            var curItem = new ItemProperty();
            curItem.Id = (int?)xElement.Attribute("Id") ?? -1;
            curItem.Name = (string)xElement.Attribute("Name");
            curItem.Scale = (float?)xElement.Attribute("Scale") ?? 1f;
            curItem.MaxStackSize = (int?)xElement.Attribute("MaxStackSize") ?? 0;

            ItemProperties.Add(curItem);
            _itemLookup.Add(curItem.Id, curItem);
            int tally = (int?)xElement.Attribute("Tally") ?? 0;
            if (tally > 0)
                _tallynames.Add(tally, curItem.Name);
            int head = (int?)xElement.Attribute("Head") ?? -1;
            if (head >= 0)
                _armorHeadNames.Add(curItem.Id, curItem.Name);
            int body = (int?)xElement.Attribute("Body") ?? -1;
            if (body >= 0)
                _armorBodyNames.Add(curItem.Id, curItem.Name);
            int legs = (int?)xElement.Attribute("Legs") ?? -1;
            if (legs >= 0)
                _armorLegsNames.Add(curItem.Id, curItem.Name);
            bool rack = (bool?)xElement.Attribute("Rack") ?? false;
            if (rack)
                _rackable.Add(curItem.Id, curItem.Name);

            bool food = (bool?)xElement.Attribute("IsFood") ?? false;
            if (food)
            {
                _foodNames.Add(curItem.Id, curItem.Name);
                curItem.IsFood = true;
            }

            bool critter = (bool?)xElement.Attribute("IsCritter") ?? false;
            if (critter)
            {
                _critterNames.Add(curItem.Id, curItem.Name);
            }

            bool kite = (bool?)xElement.Attribute("IsKite") ?? false;
            if (kite)
            {
                _kiteNames.Add(curItem.Id, curItem.Name);
            }

            bool acc = (bool?)xElement.Attribute("Accessory") ?? false;
            if (acc)
                _accessoryNames.Add(curItem.Id, curItem.Name);

            bool mount = (bool?)xElement.Attribute("Mount") ?? false;
            if (mount)
            {
                _mountNames.Add(curItem.Id, curItem.Name);
            }

            if (curItem.Name.Contains("Dye"))
            {
                _dyeNames.Add(curItem.Id, curItem.Name);
            }
        }

        foreach (var xElement in xmlSettings.Elements("Paints").Elements("Paint"))
        {
            var curPaint = new PaintProperty();
            curPaint.Id = (int?)xElement.Attribute("Id") ?? -1;
            curPaint.Name = (string)xElement.Attribute("Name");
            curPaint.Color = TEditColor.FromString((string)xElement.Attribute("Color"));
            PaintProperties.Add(curPaint);
        }

        int chestId = 0;
        foreach (var tileElement in xmlSettings.Elements("Tiles").Elements("Tile"))
        {
            string tileName = (string)tileElement.Attribute("Name");
            int type = (int)tileElement.Attribute("Id");
            if (TileTypes.IsChest(type))
            {
                foreach (var xElement in tileElement.Elements("Frames").Elements("Frame"))
                {
                    var curItem = new ChestProperty();
                    curItem.Name = (string)xElement.Attribute("Name");
                    string variety = (string)xElement.Attribute("Variety");
                    if (variety != null)
                    {
                        if (tileName == "Dresser")
                        {
                            curItem.Name = variety + " " + "Dresser";
                        }
                        else
                        {
                            curItem.Name = curItem.Name + " " + variety;
                        }
                    }
                    curItem.ChestId = chestId++;
                    curItem.UV = StringToVector2Short((string)xElement.Attribute("UV"), 0, 0);
                    curItem.TileType = (ushort)type;
                    ChestProperties.Add(curItem);
                }
            }
        }

        int signId = 0;
        foreach (var tileElement in xmlSettings.Elements("Tiles").Elements("Tile"))
        {
            var tileId = (int?)tileElement.Attribute("Id") ?? 0;
            string tileName = (string)tileElement.Attribute("Name");
            if (TileTypes.IsSign(tileId))
            {
                ushort type = (ushort)((int?)tileElement.Attribute("Id") ?? 55);
                foreach (var xElement in tileElement.Elements("Frames").Elements("Frame"))
                {
                    var curItem = new SignProperty();
                    string variety = (string)xElement.Attribute("Variety");
                    string anchor = (string)xElement.Attribute("Anchor");
                    curItem.Name = $"{tileName} {variety} {anchor}";
                    curItem.SignId = signId++;
                    curItem.UV = StringToVector2Short((string)xElement.Attribute("UV"), 0, 0);
                    curItem.TileType = type;
                    SignProperties.Add(curItem);
                }
            }
        }

        foreach (var xElement in xmlSettings.Elements("Npcs").Elements("Npc"))
        {
            int id = (int?)xElement.Attribute("Id") ?? -1;
            string name = (string)xElement.Attribute("Name");
            NpcIds[name] = id;
            NpcNames[id] = name;
            var frames = StringToVector2Short((string)xElement.Attribute("Size"), 0, 0);
            NpcFrames[id] = frames;
        }

        foreach (var xElement in xmlSettings.Elements("ItemPrefix").Elements("Prefix"))
        {
            int id = (int?)xElement.Attribute("Id") ?? -1;
            string name = (string)xElement.Attribute("Name");
            ItemPrefix.Add((byte)id, name);
        }
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
