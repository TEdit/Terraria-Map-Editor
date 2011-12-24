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
        public static readonly Dictionary<string, XNA.Color> _globalColors = new Dictionary<string, XNA.Color>();
        public static readonly IList<ItemProperty> _itemProperties = new ObservableCollection<ItemProperty>();
        public static readonly IList<TileProperty> _tileProperties = new ObservableCollection<TileProperty>();
        public static readonly IList<WallProperty> _wallProperties = new ObservableCollection<WallProperty>();
        static World()
        {
            var settingspath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "settings.xml");
            LoadObjectDbXml(settingspath);
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

        private static T InLineEnumTryParse<T>(string value) where T : struct
        {
            T result;
            Enum.TryParse(value, true, out result);
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

                foreach (var elementFrame in xElement.Elements("Frames").Elements("Frame"))
                {

                    var curFrame = new FrameProperty();
                    // Read XML attributes
                    curFrame.Name = (string)elementFrame.Attribute("Name");
                    curFrame.Variety = (string)elementFrame.Attribute("Variety");
                    curFrame.UV = StringToVector2Short((string)elementFrame.Attribute("UV"), 0, 0);
                    curFrame.Direction = InLineEnumTryParse<FrameDirection>((string)xElement.Attribute("Dir"));

                    // Assign a default name if none existed
                    if (string.IsNullOrWhiteSpace(curFrame.Name))
                        curFrame.Name = curTile.Name;

                    curTile.Frames.Add(curFrame);
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
        }



        public static Dictionary<string, XNA.Color> GlobalColors
        {
            get { return _globalColors; }
        }

        public static IList<TileProperty> TileProperties
        {
            get { return _tileProperties; }
        }

        public static IList<TileProperty> TileBricks
        {
            get { return _tileProperties.Where(x => !x.IsFramed).ToList(); }
        }

        public static IList<WallProperty> WallProperties
        {
            get { return _wallProperties; }
        }

        public static IList<ItemProperty> ItemProperties
        {
            get { return _itemProperties; }
        }
    }
}