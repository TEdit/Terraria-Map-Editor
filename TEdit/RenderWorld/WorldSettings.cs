using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;

namespace TEdit.RenderWorld
{
    public static class WorldSettings
    {
        private static readonly TileProperty[] _tiles = new TileProperty[byte.MaxValue + 1];
        private static readonly ColorProperty[] _walls = new ColorProperty[byte.MaxValue + 1];
        private static readonly Dictionary<string, Color> _globals = new Dictionary<string, Color>();
        private static readonly ObservableCollection<string> _items = new ObservableCollection<string>();

        static WorldSettings()
        {
            LoadXMLSettings("settings.xml");
            OnSettingsLoaded(null, new EventArgs());
        }

        private static void LoadXMLSettings(string file)
        {

            // Populate Defaults
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                _tiles[i] = new TileProperty { Color = Colors.Magenta, ID = (byte)i, Name = "UNKNOWN" };
                _walls[i] = new ColorProperty { Color = Colors.Magenta, ID = (byte)i, Name = "UNKNOWN" };
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
                curTile.IsSolid = ((bool?)tile.Attribute("isSolid") ?? false);
                curTile.IsSolidTop = ((bool?)tile.Attribute("isSolidTop") ?? false);
                curTile.Name = (string)tile.Attribute("name");
                curTile.Color = (Color)ColorConverter.ConvertFromString((string)tile.Attribute("color")) ;
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
                //var curItem = new ItemProperty
                //{   
                //    Id = (int) item.Attribute("num"), 
                //    Name = (string) item.Attribute("name")
                //};
                _items.Add((string)item.Attribute("name"));
            }

            // read global colors
            foreach (var globalColor in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
            {
                _globals.Add((string)globalColor.Attribute("name"),(Color)ColorConverter.ConvertFromString((string)globalColor.Attribute("color")));
            }
        }

        public static ObservableCollection<string> Items
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