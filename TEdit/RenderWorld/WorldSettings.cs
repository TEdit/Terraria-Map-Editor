using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TEdit.Common;
using System.Reflection;
using TEdit.Common.Structures;
using TEdit.Views;
using System.Windows.Interop;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace TEdit.RenderWorld
{
    public static class WorldSettings
    {
        private static readonly TileProperty[] _tiles = new TileProperty[byte.MaxValue + 1];
        private static readonly ColorProperty[] _walls = new ColorProperty[byte.MaxValue + 1];
        private static readonly Dictionary<string, ColorProperty> _globals = new Dictionary<string, ColorProperty>();
        private static readonly ObservableCollection<ItemProperty> _items = new ObservableCollection<ItemProperty>();
        private static readonly ObservableCollection<string> _itemNames = new ObservableCollection<string>();

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
                var curWall   = _walls[(int)wall.Attribute("num")];
                curWall.ID    = curWall.XMLConvert(curWall.ID,    wall.Attribute("num"));
                curWall.Name  = curWall.XMLConvert(curWall.Name,  wall.Attribute("name"));
                curWall.Color = curWall.XMLConvert(curWall.Color, wall.Attribute("color"));
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
                _itemNames.Add(curItem.Name);
            }

            // read global colors
            foreach (var global in xmlSettings.Elements("GlobalColors").Elements("GlobalColor"))
            {
                var curGlobal = new ColorProperty();
                curGlobal.Name  = curGlobal.XMLConvert(curGlobal.Name,  global.Attribute("name"));
                curGlobal.Color = curGlobal.XMLConvert(curGlobal.Color, global.Attribute("color"));
                _globals.Add(curGlobal.Name, curGlobal);
            }
        }


        private static void ParseFrameAttributes(XElement tile, FrameProperty curTile)
        {
            curTile.ID              = curTile.XMLConvert(curTile.ID,              tile.Attribute("num"));
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
                                      curTile.XMLConvert(curTile.NoAttach,        tile.Attribute("noAttach"));
                                      curTile.XMLConvert(curTile.CanMorphFrom,    tile.Attribute("canMorphFrom"));
        }

        public static ObservableCollection<string> ItemNames
        {
            get { return _itemNames; }
        }
        public static ObservableCollection<ItemProperty> Items
        {
            get { return _items; }
        }

        public static Dictionary<string, ColorProperty> GlobalColors
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

        // Post-window settings
        private static ContentManager cm = null;
        public static bool FindSteam()
        {
            // find steam
            string path = "";
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Valve\\Steam");
            if (key != null)
                path = key.GetValue("SteamPath") as string;

            // no steam key, let's try the default
            if (path.Equals("") || !Directory.Exists(path))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                path = Path.Combine(path, "Steam");
            }
            path = Path.Combine(path, "steamapps");
            path = Path.Combine(path, "common");
            path = Path.Combine(path, "terraria");
            path = Path.Combine(path, "Content");
            if (Directory.Exists(path))
            {
                HwndSource hwnd = HwndSource.FromVisual(App.Current.MainWindow) as HwndSource;
                cm = new ContentManager(new SimpleProvider(hwnd.Handle), path);
                return (cm == null) ? false : true;
            }

            return false;
        }

        public static bool TryLoadTexture(string filePart, int id, ColorProperty obj)
        {
            if (obj == null || obj.Name == "UNKNOWN" || obj.Texture != null) return false;
            var t2d = cm.Load<Texture2D>(String.Format("Images{0}{1}_{2}", Path.DirectorySeparatorChar, filePart, id));

            // TODO: Add color masking for items that use it //
            // TODO: Add scale //
            // TODO: Add alpha (some items are semi-transparent) //
            obj.Texture = new TexturePlus(t2d);
            obj.Texture.Name = obj.Name;
            return true;
        }

        public static SoundEffect LoadSound(string filePart, int id)
        {
            return cm.Load<SoundEffect>(String.Format("Sounds{0}{1}_{2}", Path.DirectorySeparatorChar, filePart, id));
        }

    }
}