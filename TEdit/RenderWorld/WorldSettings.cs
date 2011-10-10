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
            string xmlPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "settings.xml");
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

                curTile.CanMixFrames = curTile.XMLConvertBool(tile.Attribute("canMixFrames"));
                curTile.Color        = curTile.XMLConvertColor(tile.Attribute("color"));
                curTile.IsFramed     = curTile.XMLConvertBool(tile.Attribute("isFramed"));
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
            if (tile.Attribute("name")            != null) curTile.Name            = curTile.XMLConvertString(tile.Attribute("name"));
            if (tile.Attribute("contactDamage")   != null) curTile.ContactDamage   = curTile.XMLConvertInt   (tile.Attribute("contactDamage"));
            if (tile.Attribute("dir")             != null) curTile.Direction       = curTile.XMLConvertDir   (tile.Attribute("dir"));
            if (tile.Attribute("isHouseItem")     != null) curTile.IsHouseItem     = curTile.XMLConvertBool  (tile.Attribute("isHouseItem"));
            if (tile.Attribute("isSolid")         != null) curTile.IsSolid         = curTile.XMLConvertBool  (tile.Attribute("isSolid"));
            if (tile.Attribute("isSolidTop")      != null) curTile.IsSolidTop      = curTile.XMLConvertBool  (tile.Attribute("isSolidTop"));
            if (tile.Attribute("lightBrightness") != null) curTile.LightBrightness = curTile.XMLConvertFloat (tile.Attribute("lightBrightness"));
            if (tile.Attribute("placement")       != null) curTile.Placement       = curTile.XMLConvertPlace (tile.Attribute("placement"));
            if (tile.Attribute("size")            != null) curTile.Size            = curTile.XMLConvertPoint (tile.Attribute("size"));
            if (tile.Attribute("upperLeft")       != null) curTile.UpperLeft       = curTile.XMLConvertPoint (tile.Attribute("upperLeft"));
            if (tile.Attribute("variety")         != null) curTile.Variety         = curTile.XMLConvertString(tile.Attribute("variety"));
            if (tile.Attribute("growsOn")         != null) curTile.GrowsOn.ReplaceRange(curTile.XMLConvertOC (tile.Attribute("growsOn")));
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