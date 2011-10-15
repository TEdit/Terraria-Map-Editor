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
            string xmlPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "Terraria.Object.DB.xml");
            LoadXMLSettings(xmlPath);
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

                curTile.CanMixFrames = curTile.XMLConvert(curTile.CanMixFrames, tile.Attribute("canMixFrames"));
                curTile.Color        = curTile.XMLConvert(curTile.Color,        tile.Attribute("color"));
                curTile.IsFramed     = curTile.XMLConvert(curTile.IsFramed,     tile.Attribute("isFramed"));
                ParseFrameAttributes(tile, curTile);

                if (tile.Elements().Any(x => x.Name == "Frames"))
                {
                    //byte c = 0;
                    foreach (var frame in tile.Elements("Frames").Elements("Frame"))
                    {
                        var curFrame = (FrameProperty)curTile.Clone();  // carry over the properties of the parent Tile
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
                curItem.ID           = curItem.XMLConvert(curItem.ID,           item.Attribute("num"));
                curItem.Name         = curItem.XMLConvert(curItem.Name,         item.Attribute("name"));
                curItem.MaxStack     = curItem.XMLConvert(curItem.MaxStack,     item.Attribute("maxStack"));
                curItem.Description  = curItem.XMLConvert(curItem.Description,  item.Attribute("description"));
                curItem.Description2 = curItem.XMLConvert(curItem.Description2, item.Attribute("description2"));
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
            curTile.Name            = curTile.XMLConvert(curTile.Name,            tile.Attribute("name"));
            curTile.ContactDamage   = curTile.XMLConvert(curTile.ContactDamage,   tile.Attribute("contactDamage"));
            curTile.Direction       = curTile.XMLConvert(curTile.Direction,       tile.Attribute("dir"));
            curTile.IsHouseItem     = curTile.XMLConvert(curTile.IsHouseItem, 	  tile.Attribute("isHouseItem"));
            curTile.IsSolid         = curTile.XMLConvert(curTile.IsSolid, 		  tile.Attribute("isSolid"));
            curTile.IsSolidTop      = curTile.XMLConvert(curTile.IsSolidTop, 	  tile.Attribute("isSolidTop"));
            curTile.LightBrightness = curTile.XMLConvert(curTile.LightBrightness, tile.Attribute("lightBrightness"));
            curTile.Placement       = curTile.XMLConvert(curTile.Placement,       tile.Attribute("placement"));
            curTile.Size            = curTile.XMLConvert(curTile.Size, 			  tile.Attribute("size"));
            curTile.UpperLeft       = curTile.XMLConvert(curTile.UpperLeft, 	  tile.Attribute("upperLeft"));
            curTile.Variety         = curTile.XMLConvert(curTile.Variety, 		  tile.Attribute("variety"));
                                      curTile.XMLConvert(curTile.AttachesTo,	  tile.Attribute("attachesTo"));
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