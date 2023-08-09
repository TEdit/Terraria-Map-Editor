using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using TEdit.Common.Reactive;
using TEdit.Terraria.Objects;
using System.Linq;
using TEdit.Geometry;
using TEdit.Common;
using TEdit.Configuration.BiomeMorph;

namespace TEdit.Configuration;

public class WorldConfiguration
{
    public static uint CompatibleVersion { get; set; } = 275;
    public static short TileCount { get; private set; } = 693; // updated by json
    public static short WallCount { get; private set; } = 346; // updated by json
    public static short MaxNpcID { get; private set; } = 687; // updated by json
    public static int MaxChests { get; private set; } = 8000;
    public static int MaxSigns { get; private set; } = 1000;
    public static int CavernLevelToBottomOfWorld { get; private set; } = 478;
    public static byte MaxMoons = 3;

    public const string DesktopHeader = "relogic";
    public const string ChineseHeader = "xindong";

    public static bool[] SettingsTileFrameImportant { get; private set; }

    public static SaveVersionManager SaveConfiguration { get; set; }
    public static BestiaryConfiguration BestiaryData { get; set; }
    public static MorphConfiguration MorphSettings { get; set; }

    public static List<string> Biomes => MorphSettings.Biomes.Keys.ToList();
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
    private static readonly Dictionary<int, string> _accessoryNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _dyeNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _armorBodyNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _armorLegsNames = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> _rackable = new Dictionary<int, string>();

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
        if (ViewModelBase.IsInDesignModeStatic)
            return;

        var saveVersionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TerrariaVersionTileData.json");
        if (File.Exists(saveVersionPath))
            SaveConfiguration = SaveVersionManager.LoadJson(saveVersionPath);

        var settingspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
        if (File.Exists(settingspath))
            LoadObjectDbXml(settingspath);

        var bestiaryDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "npcData.json");
        if (File.Exists(bestiaryDataPath))
            BestiaryData = BestiaryConfiguration.LoadJson(bestiaryDataPath);

        try
        {
            var morphPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "morphSettings.json");
            if (File.Exists(morphPath))
                MorphSettings = MorphConfiguration.LoadJson(morphPath);

        }
        catch (Exception ex)
        {
            throw new ApplicationException("Invalid morphSettings.json", ex);
        }

        try
        {
            // Used to dynamically update static CompatibleVersion
            CompatibleVersion = (uint)SaveConfiguration.SaveVersions.Keys.Max();
            TileCount = (short)SaveConfiguration.SaveVersions[(int)CompatibleVersion].MaxTileId;
            WallCount = (short)SaveConfiguration.SaveVersions[(int)CompatibleVersion].MaxWallId;
            MaxNpcID = (short)SaveConfiguration.SaveVersions[(int)CompatibleVersion].MaxNpcId;

            if (SettingsTileFrameImportant == null || SettingsTileFrameImportant.Length <= 0)
            {
                SettingsTileFrameImportant = new bool[TileCount + 1];
                for (int i = 0; i <= TileCount; i++)
                {
                    if (TileProperties.Count > i)
                    {
                        SettingsTileFrameImportant[i] = TileProperties[i].IsFramed;
                    }
                }
            }
        }
        catch (Exception ex)
        {
        }

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
            Name = "Air",
            Color = TEditColor.Transparent
        });

        TileBricksMask.Add(new TileProperty
        {
            Id = -1,
            Name = "Air",
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

            bool acc = (bool?)xElement.Attribute("Accessory") ?? false;
            if (acc)
                _accessoryNames.Add(curItem.Id, curItem.Name);

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

    public static Dictionary<int, string> AccessoryNames
    {
        get { return _accessoryNames; }
    }

    public static Dictionary<int, string> FoodNames
    {
        get { return _foodNames; }
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
