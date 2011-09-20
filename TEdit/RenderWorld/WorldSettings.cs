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
        private static readonly ObservableCollection<ItemProperty> _items = new ObservableCollection<ItemProperty>();

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

                curTile.IsFramed        = ((bool?)tile.Attribute("isFramed")     ?? false);
                curTile.IsSolid         = ((bool?)tile.Attribute("isSolid")      ?? false);
                curTile.IsSolidTop      = ((bool?)tile.Attribute("isSolidTop")   ?? false);
                curTile.IsHouseItem     = ((bool?)tile.Attribute("isHouseItem")  ?? false);
                curTile.CanMixFrames    = ((bool?)tile.Attribute("canMixFrames") ?? false);
                curTile.Name            = (string)tile.Attribute("name");
                curTile.Variety         = (string)tile.Attribute("variety");
                curTile.Dir             = (char)tile.Attribute("dir");
                
                curTile.LightBrightness = (byte)tile.Attribute("lightBrightness").ToString.Replace('%','');
                curTile.ContactDmg      = (ushort)tile.Attribute("contactDmg");
                
                curTile.Color           = (Color)ColorConverter.ConvertFromString((string)tile.Attribute("color")) ;

                curTile.GrowsOn         = (byte[])tile.Attribute("growsOn").ToString.Split( new[] { ', ', ',' } ) ?? new[] {};
                curTile.HangsOn         = (byte[])tile.Attribute("hangsOn").ToString.Split( new[] { ', ', ',' } ) ?? new[] {};
                
                curTile.Size            = new SizeProperty((string)tile.Attribute("size"));
                curTile.Placement       = new PlacementProperty((string)tile.Attribute("placement"));
                
                if (curTile.IsFramed) {

                    // read frames
                    foreach (var frame in tile.Elements("Frame"))
                    {
                        var curFrame = curTile.Frames[(int)frame.Attribute("num")];
                        
                        curFrame.Parent = curTile;
        
                        curFrame.IsSolid         = (bool?)frame.Attribute("isSolid");
                        curFrame.IsSolidTop      = (bool?)frame.Attribute("isSolidTop");
                        curFrame.IsHouseItem     = (bool?)frame.Attribute("isHouseItem");
                        curFrame.CanMixFrames    = (bool?)frame.Attribute("canMixFrames");
                        curFrame.Name            = (string?)frame.Attribute("name");
                        curFrame.Variety         = (string?)frame.Attribute("variety");
                        curFrame.Dir             = (char?)frame.Attribute("dir");
                        
                        curFrame.LightBrightness = (byte?)frame.Attribute("lightBrightness").ToString.Replace('%','');
                        curFrame.ContactDmg      = (ushort?)frame.Attribute("contactDmg");
                        
                        curFrame.Color           = (Color?)ColorConverter.ConvertFromString((string)frame.Attribute("color")) ;
        
                        curFrame.GrowsOn         = frame.Attribute("growsOn") ? (byte[])frame.Attribute("growsOn").ToString.Split( new[] { ', ', ',' } ) : null;
                        curFrame.HangsOn         = frame.Attribute("hangsOn") ? (byte[])frame.Attribute("hangsOn").ToString.Split( new[] { ', ', ',' } ) : null;
                        
                        curFrame.Size            = frame.Attribute("size")      ? new      SizeProperty((string)frame.Attribute("size"))      : null;
                        curFrame.Placement       = frame.Attribute("placement") ? new PlacementProperty((string)frame.Attribute("placement")) : null;
                        
                    }
                    
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
                var curItem = new ItemProperty
                {   
                    ID   = (int)    item.Attribute("num"), 
                    Name = (string) item.Attribute("name"),
                    Type = (string) item.Attribute("type")
                };
                
                _items.Add(curItem);
            }

            // read global colors
            foreach (var globalColor in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
            {
                _globals.Add((string)globalColor.Attribute("name"),(Color)ColorConverter.ConvertFromString((string)globalColor.Attribute("color")));
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