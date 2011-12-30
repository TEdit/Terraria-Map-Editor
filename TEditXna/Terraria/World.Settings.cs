using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Xml.Linq;
using BCCL.Geometry.Primitives;
using XNA = Microsoft.Xna.Framework;
using TEditXNA.Terraria.Objects;

namespace TEditXNA.Terraria
{
    public partial class World
    {
        private static readonly Dictionary<string, XNA.Color> _globalColors = new Dictionary<string, XNA.Color>();
        private static readonly Dictionary<string, int> _npcIds = new Dictionary<string, int>();
        private static readonly Dictionary<byte, string> _prefix = new Dictionary<byte, string>();
        private static readonly IList<ItemProperty> _itemProperties = new ObservableCollection<ItemProperty>();
        private static readonly IList<TileProperty> _tileProperties = new ObservableCollection<TileProperty>();
        private static readonly IList<WallProperty> _wallProperties = new ObservableCollection<WallProperty>();
        private static readonly ObservableCollection<Sprite> _sprites = new ObservableCollection<Sprite>();


        static World()
        {
            var settingspath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "settings.xml");
            LoadObjectDbXml(settingspath);
            Sprites.Add(new Sprite());
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

        private static Color ColorFromString(string colorstring)
        {
            if (!string.IsNullOrWhiteSpace(colorstring))
            {
                var colorFromString = ColorConverter.ConvertFromString(colorstring);
                if (colorFromString != null)
                {
                    return (Color)colorFromString;
                }
            }
            return Colors.Magenta;
        }
        private static XNA.Color XnaColorFromString(string colorstring)
        {
            if (!string.IsNullOrWhiteSpace(colorstring))
            {
                var colorFromString = ColorConverter.ConvertFromString(colorstring);
                if (colorFromString != null)
                {
                    var c = (Color)colorFromString;
                    return XNA.Color.FromNonPremultiplied(c.R, c.G, c.B, c.A);
                }
            }
            return XNA.Color.Magenta;
        }

        private static void LoadObjectDbXml(string file)
        {
            var xmlSettings = XElement.Load(file);

            // Load Colors
            foreach (var xElement in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
            {
                string name = (string)xElement.Attribute("Name");
                XNA.Color color = XnaColorFromString((string)xElement.Attribute("Color"));
                GlobalColors.Add(name, color);
            }

            foreach (var xElement in xmlSettings.Elements("Tiles").Elements("Tile"))
            {

                var curTile = new TileProperty();

                // Read XML attributes
                curTile.Color = ColorFromString((string)xElement.Attribute("Color"));
                curTile.Name = (string)xElement.Attribute("Name");
                curTile.Id = (int?)xElement.Attribute("Id") ?? 0;
                curTile.IsFramed = (bool?)xElement.Attribute("Framed") ?? false;
                curTile.IsSolid = (bool?)xElement.Attribute("Solid") ?? false;
                curTile.IsSolidTop = (bool?)xElement.Attribute("SolidTop") ?? false;
                curTile.IsLight = (bool?)xElement.Attribute("Light") ?? false;
                curTile.FrameSize = StringToVector2Short((string)xElement.Attribute("Size"), 1, 1);
                curTile.Placement = InLineEnumTryParse<FramePlacement>((string)xElement.Attribute("Placement"));
                curTile.TextureGrid = StringToVector2Short((string)xElement.Attribute("TextureGrid"), 16, 16);
                foreach (var elementFrame in xElement.Elements("Frames").Elements("Frame"))
                {

                    var curFrame = new FrameProperty();
                    // Read XML attributes
                    curFrame.Name = (string)elementFrame.Attribute("Name");
                    curFrame.Variety = (string)elementFrame.Attribute("Variety");
                    curFrame.UV = StringToVector2Short((string)elementFrame.Attribute("UV"), 0, 0);
                    curFrame.Anchor = InLineEnumTryParse<FrameAnchor>((string)elementFrame.Attribute("Anchor"));

                    // Assign a default name if none existed
                    if (string.IsNullOrWhiteSpace(curFrame.Name))
                        curFrame.Name = curTile.Name;

                    curTile.Frames.Add(curFrame);
                    Sprites.Add(new Sprite
                                    {
                                        Anchor = curFrame.Anchor,
                                        IsPreviewTexture = false,
                                        Name = curFrame.Name + ", " + curFrame.Variety,
                                        Origin = curFrame.UV,
                                        Size = curTile.FrameSize,
                                        Tile = (byte)curTile.Id,
                                        TileName = curTile.Name
                                    });
                }
                if (curTile.Frames.Count == 0 && curTile.IsFramed)
                {
                    var curFrame = new FrameProperty();
                    // Read XML attributes
                    curFrame.Name = curTile.Name;
                    curFrame.Variety = string.Empty;
                    curFrame.UV = new Vector2Short(0, 0);
                    //curFrame.Anchor = InLineEnumTryParse<FrameAnchor>((string)xElement.Attribute("Anchor"));

                    // Assign a default name if none existed
                    if (string.IsNullOrWhiteSpace(curFrame.Name))
                        curFrame.Name = curTile.Name;

                    curTile.Frames.Add(curFrame);
                    Sprites.Add(new Sprite
                                    {
                                        Anchor = curFrame.Anchor,
                                        IsPreviewTexture = false,
                                        Name = curFrame.Name + ", " + curFrame.Variety,
                                        Origin = curFrame.UV,
                                        Size = curTile.FrameSize,
                                        Tile = (byte)curTile.Id,
                                        TileName = curTile.Name
                                    });
                }
                TileProperties.Add(curTile);

            }

            foreach (var xElement in xmlSettings.Elements("Walls").Elements("Wall"))
            {
                var curWall = new WallProperty();
                curWall.Color = ColorFromString((string)xElement.Attribute("Color"));
                curWall.Name = (string)xElement.Attribute("Name");
                curWall.Id = (int?)xElement.Attribute("Id") ?? -1;
                curWall.IsHouse = (bool?)xElement.Attribute("IsHouse") ?? false;
                WallProperties.Add(curWall);
            }

            foreach (var xElement in xmlSettings.Elements("Items").Elements("Item"))
            {
                var curItem = new ItemProperty();
                curItem.Id = (int?)xElement.Attribute("Id") ?? -1;
                curItem.Name = (string)xElement.Attribute("Name");
                ItemProperties.Add(curItem);
            }
            foreach (var xElement in xmlSettings.Elements("Npcs").Elements("Npc"))
            {
                int id = (int?)xElement.Attribute("Id") ?? -1;
                string name = (string)xElement.Attribute("Name");
                NpcIds.Add(name, id);
            }

            foreach (var xElement in xmlSettings.Elements("ItemPrefix").Elements("Prefix"))
            {
                int id = (int?)xElement.Attribute("Id") ?? -1;
                string name = (string)xElement.Attribute("Name");
                ItemPrefix.Add((byte)id, name);
            }


        }



        public static Dictionary<string, XNA.Color> GlobalColors
        {
            get { return _globalColors; }
        }

        public static Dictionary<string, int> NpcIds
        {
            get { return _npcIds; }
        }

        public static Dictionary<byte, string> ItemPrefix
        {
            get { return _prefix; }
        }

        public static IList<TileProperty> TileProperties
        {
            get { return _tileProperties; }
        }

        public static IList<TileProperty> TileBricks
        {
            get
            {
                return _tileProperties.Where(x => !x.IsFramed).ToList();
            }
        }

        public static IList<WallProperty> WallProperties
        {
            get { return _wallProperties; }
        }

        public static IList<ItemProperty> ItemProperties
        {
            get { return _itemProperties; }
        }

        public static ObservableCollection<Sprite> Sprites
        {
            get { return _sprites; }
        }
    }
}