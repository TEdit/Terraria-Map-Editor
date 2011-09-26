using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using TEdit.TerrariaWorld.Structures;
using System.Reflection;

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

                curTile.IsFramed = ((bool?)tile.Attribute("isFramed") ?? false);
                curTile.CanMixFrames = ((bool?)tile.Attribute("canMixFrames") ?? false);
                ParseCommonXML(ref curTile, tile);

                if (tile.Elements().Any(x => x.Name == "Frames"))
                {
                    byte c = 0;
                    foreach (var frame in tile.Elements("Frames").Elements("Frame"))
                    {
                        var curFrame = new FrameProperty();
                        curFrame.SetParent(ref curTile);
                        curFrame.ID = c++;
                        ParseCommonXML(ref curFrame, frame);
                        curTile.Frames.Add(curFrame);
                    }
                }
                // Default Frame 0
                else if (curTile.IsFramed)
                {
                    var curFrame = new FrameProperty();
                    curFrame.SetParent(ref curTile);
                    curTile.Frames.Add(curFrame);
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
                curItem.ID   = (int?)item.Attribute("num");
                curItem.Name = (string)item.Attribute("name");
                curItem.Type = (string)item.Attribute("type");
                _items.Add(curItem);
            }

            // read global colors
            foreach (var globalColor in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
            {
                _globals.Add((string)globalColor.Attribute("name"), (Color)ColorConverter.ConvertFromString((string)globalColor.Attribute("color")));
            }
        }

        private static void ParseCommonXML<T>(ref T curItem, XElement xml)
        {
            foreach (string prop in new[] { "name", "color", "variety", "dir", "upperLeft", "isSolid", "isSolidTop", "size", "placement", "lightBrightness" })
            {
                XAttribute xattr = xml.Attribute(prop);
                if (xattr == null) continue;
                string attr = (string)xattr;
                string field = char.ToUpper(prop[0]) + prop.Substring(1);
                if (prop == "dir") field = "Direction";  // exception
                PropertyInfo p = typeof(T).GetProperty(field);

                switch (prop)
                {
                    case "name":
                    case "variety":
                        p.SetValue(curItem, attr, null);
                        break;
                    case "upperLeft":
                    case "size":
                        p.SetValue(curItem, PointShort.Parse(attr), null);
                        break;
                    case "isSolid":
                    case "isSolidTop":
                        p.SetValue(curItem, (bool)xattr, null);
                        break;
                    case "color": p.SetValue(curItem, (Color)ColorConverter.ConvertFromString(attr), null); break;
                    case "dir": p.SetValue(curItem, (FrameDirection)Enum.Parse(typeof(FrameDirection), attr), null); break;
                    case "placement": p.SetValue(curItem, (FramePlacement)Enum.Parse(typeof(FramePlacement), char.ToUpper(attr[0]) + attr.Substring(1)), null); break;
                    case "lightBrightness": p.SetValue(curItem, (byte)((float)xattr * 100), null); break;
                }
            }
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