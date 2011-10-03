using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using TEdit.Common;
using System.Reflection;
using TEdit.Common.Structures;

namespace TEdit.RenderWorld
{
    public static class WorldSettings
    {
        private static readonly TileProperty[] _tiles = new TileProperty[byte.MaxValue + 1];
        private static readonly ColorProperty[] _walls = new ColorProperty[byte.MaxValue + 1];
        private static readonly Dictionary<string, Color> _globals = new Dictionary<string, Color>();
        private static readonly ObservableCollection<ItemProperty> _items = new ObservableCollection<ItemProperty>();

        static WorldSettings()
        {
            LoadXMLSettings("settings.xml");
            OnSettingsLoaded(null, new EventArgs());
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

        private static void LoadXMLSettings(string file)
        {

            // Populate Defaults
            for (ushort i = 0; i <= byte.MaxValue; i++)
            {
                _tiles[i] = new TileProperty((byte)i);
                _walls[i] = new ColorProperty((byte)i);
            }
            _globals.Clear();
            _items.Clear();

            // load xml file
            var xmlSettings = XElement.Load(file);

            // read tiles
            foreach (var tile in xmlSettings.Elements("Tiles").Elements("Tile"))
            {
                var curTile = _tiles[(int)tile.Attribute("num")];

                curTile.CanMixFrames = ((bool?)tile.Attribute("canMixFrames") ?? false);
                curTile.Color = (Color)ColorConverter.ConvertFromString((string)tile.Attribute("color"));
                curTile.IsFramed = ((bool?)tile.Attribute("isFramed") ?? false);
                ParseFrameAttributes(tile, curTile);

                if (tile.Elements().Any(x => x.Name == "Frames"))
                {
                    //byte c = 0;
                    foreach (var frame in tile.Elements("Frames").Elements("Frame"))
                    {
                        var curFrame = new FrameProperty { ID = curTile.ID };
                        ParseFrameAttributes(frame, curFrame);
                        curTile.Frames.Add(curFrame);
                    }
                }
                else if (curTile.IsFramed)
                {
                    curTile.Frames.Add((FrameProperty)curTile);
                }
            }

            // read walls
            foreach (var wall in xmlSettings.Elements("Walls").Elements("Wall"))
            {
                var curWall = _walls[(int)wall.Attribute("num")];
                curWall.Name = (string)wall.Attribute("name");
                curWall.Color = (Color)ColorConverter.ConvertFromString((string)wall.Attribute("color"));
            }

            // read items
            foreach (var item in xmlSettings.Elements("Items").Elements("Item"))
            {
                var curItem = new ItemProperty();
                curItem.ID = (byte)((int?)item.Attribute("num") ?? 0);
                curItem.Name = (string)item.Attribute("name");
                curItem.ItemType = (string)item.Attribute("type");
                _items.Add(curItem);
            }

            // read global colors
            foreach (var globalColor in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
            {
                _globals.Add((string)globalColor.Attribute("name"), (Color)ColorConverter.ConvertFromString((string)globalColor.Attribute("color")));
            }
        }


        private static void ParseFrameAttributes(XElement tile, FrameProperty curTile)
        {
            curTile.Name = (string)tile.Attribute("name") ?? string.Empty;
            curTile.ContactDamage = (int?)tile.Attribute("contactDamage") ?? 0;
            curTile.Direction = InLineEnumTryParse<FrameDirection>((string)tile.Attribute("dir"));
            curTile.IsHouseItem = ((bool?)tile.Attribute("isHouseItem") ?? false);
            curTile.IsSolid = ((bool?)tile.Attribute("isSolid") ?? false);
            curTile.IsSolidTop = ((bool?)tile.Attribute("isSolidTop") ?? false);
            curTile.LightBrightness = ((float?)tile.Attribute("lightBrightness") ?? 0F);
            curTile.Placement = InLineEnumTryParse<FramePlacement>((string)tile.Attribute("placement"));
            curTile.Size = PointShort.TryParseInline((string)tile.Attribute("size"));
            if (curTile.Size.X == 0 || curTile.Size.Y == 0)
                curTile.Size = new PointShort(1,1);
            curTile.UpperLeft = PointShort.TryParseInline((string)tile.Attribute("upperLeft"));
            curTile.Variety = (string)tile.Attribute("variety") ?? string.Empty;
            curTile.GrowsOn.ReplaceRange(StringToList<byte>((string)tile.Attribute("growsOn")));
        }

        public static ObservableCollection<ItemProperty> Items
        {
            get { return _items; }
        }

        public static Dictionary<string, Color> GlobalColors
        {
            get { return _globals; }
        }

        public static TileProperty[] Tiles
        {
            get { return _tiles; }
        }

        public static ColorProperty[] Walls
        {
            get { return _walls; }
        }

        public static Color Water { get; set; }
        public static Color Lava { get; set; }
        public static event EventHandler SettingsLoaded;

        private static void OnSettingsLoaded(object sender, EventArgs e)
        {
            if (SettingsLoaded != null)
                SettingsLoaded(sender, e);
        }
    }
}