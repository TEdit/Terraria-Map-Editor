using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Terraria;
using Terraria.Localization;
using Terraria.Initializers;
using Terraria.Social;
using System.Text;
using Terraria.ObjectData;
using System.Reflection;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Terraria.Map;

namespace SettingsFileUpdater.TerrariaHost
{
    //public static class ColorExt
    //{
    //    public static string ColorToHex(this Color color) => string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
    //}
    public class ItemId
    {
        public ItemId(int id, string name, string type)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }

        public bool IsFood { get; set; }

        public int Head { get; set; }
        public int Banner { get; set; }
        public int Body { get; set; }
        public int Legs { get; set; }
        public bool Accessory { get; set; }
        public bool Rack { get; set; }
    }

    public class TerrariaWrapper : Terraria.Main

    {
        public static TerrariaWrapper Initialize()
        {
            Thread.CurrentThread.Name = "Main Thread";

            var terrariaAsm = typeof(Terraria.Program).Assembly;
            //var init = typeof(Terraria.Program).GetMethod("ForceLoadAssembly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(Assembly), typeof(bool) }, null);
            //init.Invoke((object)null, new object[] { terrariaAsm, true });
            // Program.LaunchParameters = Utils.ParseArguements(args);
            //ThreadPool.SetMinThreads(8, 8);
            LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);
            //Program.InitializeConsoleOutput();
            //Program.SetupLogging();
            //Platform.Get<IWindowService>().SetQuickEditEnabled(false);
            var main = new TerrariaWrapper();
            try
            {
                Lang.InitializeLegacyLocalization();
                SocialAPI.Initialize(null);
                LaunchInitializer.LoadParameters(main);
                main.InitBase();
                //Task.Factory.StartNew(() => main.DedServ());
                //Thread.Sleep(10000);
                //main.Run();
                return main;

            }
            catch (Exception exception)
            {
            }
            return null;
        }

        public static string Localize(string value) =>
            Language.GetTextValue(value);

        public TerrariaWrapper() : base()
        {

        }

        public void InitBase()
        {
            base.Initialize();
        }


        public Terraria.Item GetItem(int id)
        {
            try
            {
                var curitem = new Terraria.Item();
                curitem.SetDefaults(id);
                return curitem;
            }
            catch
            {
                return null;
            }

        }

        public IEnumerable<Terraria.NPC> GetNpcs()
        {
            for (int id = 0; id < maxNPCTypes; id++)
            {
                var npc = new NPC();
                npc.SetDefaults(id);

                if (npc.friendly)
                {

                    yield return npc;
                }
            }

        }

        MapTile _tile = new MapTile { Type = 0, Light = byte.MaxValue };

        //public (Dictionary<int, string>, Dictionary<int,string>) GetMapTileColors()
        //{
        //    var colors = new Dictionary<int, string>();
        //    var wallColors = new Dictionary<int, string>();

        //    int colorLookup = 1;
        //    MapHelper.Initialize();
        //    for (int i = 0; i < maxTileSets; i++)
        //    {
        //        colors[i] = GetTileColor(colorLookup);
        //        colorLookup++;
        //    }
        //    for (int i = 0; i < maxWallTypes; i++)
        //    {
        //        wallColors[i] = GetTileColor(colorLookup);
        //        colorLookup++;
        //    }
        //    for (int i = 0; i < 3; i++)
        //    {
        //        // water, lava, honey
        //        colorLookup++;
        //    }
        //    for (int i = 0; i < 256; i++)
        //    {
        //        //sky
        //        colorLookup++;
        //    }
        //    for (int i = 0; i < 256; i++)
        //    {
        //        //dirt
        //        colorLookup++;
        //    }
        //    for (int i = 0; i < 256; i++)
        //    {
        //        //rock
        //        colorLookup++;
        //    }

        //    // hell is 1
        //    colorLookup++;

        //    return (colors,wallColors);
        //}

        //private string GetTileColor(int colorLookup)
        //{
        //    _tile.Type = (ushort)colorLookup;
        //    var color = MapHelper.GetMapTileXnaColor(ref _tile);
        //    return color.ColorToHex();
        //}

        public string GetWallsXml()
        {
            List<Terraria.Item> curItems = new List<Item>();
            for (int i = -255; i < maxItemTypes; i++)
            {
                try
                {
                    var curitem = new Terraria.Item();
                    curitem.SetDefaults(i);
                    curItems.Add(curitem);
                }
                catch
                {

                }
            }
            var output = new StringBuilder("  <Walls>\r\n");
            for (int i = 0; i < maxWallTypes; i++)
            {

                var creatingWall = curItems.FirstOrDefault(x => x.createWall == i);

                output.AppendFormat("    <Wall Id=\"{0}\" Name=\"{2}\" Color=\"#FFFF00FF\" IsHouse=\"{1}\"/>\r\n",
                    i,
                    wallHouse[i],
                    creatingWall != null ? creatingWall.Name : string.Empty);

            }

            output.Append("  </Walls>");

            return output.ToString();
        }

        public string GetTilesXml()
        {
            XDocument original = null;
            using (TextReader tr = new StreamReader("settings.xml"))
            {
                original = XDocument.Load(tr);
            }

            var origTiles = original.Element("Settings").Element("Tiles");

            XElement root = new XElement("Tiles");
            XDocument tiles = new XDocument(root);

            List<Terraria.Item> curItems = new List<Item>();
            for (int i = 0; i < maxItemTypes; i++)
            {
                try
                {
                    var curitem = new Terraria.Item();
                    curitem.SetDefaults(i);
                    curItems.Add(curitem);
                }
                catch
                {

                }
            }

            for (int i = 0; i < maxTileSets; i++)
            {
                string origName = origTiles.Elements().FirstOrDefault(e => e.Attribute("Id").Value == i.ToString())?.Attribute("Name").Value;

                var creatingItem = curItems.FirstOrDefault(x => x.createTile == i);
                //var creatingItems = curItems.Where(x => x.createTile == i).ToList();

                string itemName = creatingItem != null ? creatingItem.Name : string.Empty;
                if (string.IsNullOrWhiteSpace(itemName))
                {
                    itemName = origName ?? i.ToString();
                }

                var tile = new XElement(
                    "Tile",
                    new XAttribute("Id", i.ToString()),
                    new XAttribute("Name", itemName));
                root.Add(tile);

                if (tileLighted[i]) { tile.Add(new XAttribute("Light", "true")); }
                if (Terraria.ID.TileID.Sets.NonSolidSaveSlopes[i]) { tile.Add(new XAttribute("SaveSlope", "true")); }
                if (tileSolid[i]) { tile.Add(new XAttribute("Solid", "true")); }
                if (tileSolidTop[i]) { tile.Add(new XAttribute("SolidTop", "true")); }

                if (tileFrameImportant[i])
                {
                    tile.Add(new XAttribute("Framed", "true"));
                    var frames = new XElement("Frames");
                    tile.Add(frames);

                    TileObjectData data = TileObjectData.GetTileData(i, 0);

                    if (data == null)
                    {
                        string value = itemName;
                        frames.Add(new XElement("Frame",
                            new XAttribute("Name", value),
                            new XAttribute("UV", $"0, 0"))
                        );
                    }
                    else
                    {
                        var tileWidth = data.Width;
                        var tileHeight = data.Height;
                        var textureWidth = data.CoordinateWidth;
                        var textureHeight = data.CoordinateHeights.First(); ;
                        var shiftWidth = data.CoordinateFullWidth;
                        var shiftHeight = data.CoordinateFullHeight;
                        var anchor = string.Empty;

                        var styleMultiplier = data.StyleMultiplier;
                        var styleWrapLimit = data.StyleWrapLimit;

                        if (textureWidth != 16 || textureHeight != 16)
                        {
                            tile.Add(new XAttribute("TextureGrid", $"{textureWidth},{textureHeight}"));
                        }
                        tile.Add(new XAttribute("Size", $"{tileWidth},{tileHeight}"));


                        int style = 0;
                        while ((data = TileObjectData.GetTileData(i, style, 0)) != null)
                        {
                            var creatingSubItem = curItems.FirstOrDefault(x => x.createTile == i && x.placeStyle == style);
                            if (creatingSubItem == null)
                            {
                                if (style == 0)
                                {
                                    frames.Add(new XElement("Frame",
                                        new XAttribute("Name", itemName),
                                        new XAttribute("UV", $"0, 0"))
                                    );
                                }
                                break;
                            }

                            string subTypeName = creatingSubItem != null ? creatingSubItem.Name : string.Empty;
                            int altCount = data.AlternatesCount;
                            for (int alt = 0; alt < altCount || alt == 0; alt++)
                            {
                                data = TileObjectData.GetTileData(i, style, alt);
                                if (data == null) continue;

                                var frame = new XElement("Frame", new XAttribute("Name", subTypeName));
                                frames.Add(frame);
                                frame.Add(new XAttribute("UV", $"{shiftWidth * alt}, {shiftHeight * style}"));

                                //if (alt > 0 && data.AlternatesCount > 0) System.Diagnostics.Debugger.Break();

                                if (data.AnchorBottom.tileCount > 0) { anchor = "Bottom"; }
                                if (data.AnchorLeft.tileCount > 0) { anchor = "Left"; }
                                if (data.AnchorRight.tileCount > 0) { anchor = "Right"; }
                                if (data.AnchorTop.tileCount > 0) { anchor = "Top"; }

                                frame.Add(new XAttribute("Anchor", anchor));
                            }

                            style++;

                        }
                    }
                }
            }

            return tiles.ToString();
        }

        public string GetNpcsXml()
        {
            var npcs = GetNpcs();

            var output = new StringBuilder("  <Npcs>\r\n");
            foreach (var npc in npcs)
            {
                try
                {
                    output.AppendFormat("<Npc Id=\"{1}\" Name=\"{0}\" Size=\"{2},{3}\" />\r\n", Localize(npc.FullName), npc.netID, npc.width, npc.height);
                }
                catch
                {

                }
            }
            output.Append("  </Npcs>");

            return output.ToString();
        }

        public string GetItemsXml()
        {
            var items = GetItems().ToList().OrderBy(x => x.Id).ToList();

            var output = new StringBuilder("  <Items>\r\n");
            foreach (var item in items)
            {
                // this could probably be inverted to slot="head" etc.
                string attribs = string.Join(" ", new string[]
                {
                    (item.IsFood ? " IsFood=\"True\"" : ""),
                    (item.Head > 0? $" Head=\"{item.Head}\"" : ""),
                    (item.Body > 0? $" Body=\"{item.Body}\"" : ""),
                    (item.Legs > 0? $" Legs=\"{item.Legs}\"" : ""),
                    (item.Accessory ? " Accessory=\"True\"" : ""),
                    (item.Rack ? " Rack=\"True\"" : ""),
                    (item.Banner > 0 ? $" Tally=\"{item.Banner}\"": "")
                });
                output.AppendFormat("    <Item Id=\"{0}\" Name=\"{1}\"{3}/>\r\n", item.Id, Localize(item.Name), item.Type, attribs);
            }
            output.Append("  </Items>");

            return output.ToString();
        }

        public string GetPrefixesXml()
        {
            var p = Prefixes();

            var output = new StringBuilder("  <ItemPrefix>\r\n");
            for (int i = 0; i < p.Count; i++)
            {
                output.AppendFormat("    <Prefix Id=\"{0}\" Name=\"{1}\" />\r\n", i, p[i]);
            }
            output.Append("  </ItemPrefix>");

            return output.ToString();
        }


        //public Terraria.Item GetN

        public List<string> Prefixes()
        {

            var result = new List<string> { string.Empty };
            var curitem = new Terraria.Item();
            //curitem.Name = "";
            for (int prefix = 1; prefix < byte.MaxValue; prefix++)
            {
                try
                {
                    curitem.prefix = (byte)prefix;
                    string affixName = curitem.AffixName();
                    if (string.IsNullOrWhiteSpace(affixName))
                        break;

                    result.Add(affixName);
                }
                catch
                {

                }
            }

            return result;
        }

        public Terraria.Recipe[] Recipes
        {
            get { return recipe; }
        }

        private string GetItemType(Item i)
        {
            if (i.damage > 0)
                return "Weapon";
            if (i.potion)
                return "Potion";
            if (i.accessory)
                return "Accessory";
            if (i.createTile > 0)
                return "Tile";
            if (i.createWall > 0)
                return "Wall";
            if (i.hammer > 0)
                return "Hammer";

            return string.Empty;
        }

        public List<ItemId> GetItems()
        {
            var banners = new Dictionary<int, int>();
            for (int bannerId = 0; bannerId < Terraria.Main.MaxBannerTypes; bannerId++)
            {
                int itemId = Terraria.Item.BannerToItem(bannerId);
                banners[itemId] = bannerId;
            }

            const int maxBanner = 289;
            //maxTileSets
            var sitems = new List<ItemId>();

            for (int i = -255; i < maxItemTypes; i++)
            {
                try
                {
                    var curitem = new Terraria.Item();
                    curitem.netDefaults(i);

                    if (string.IsNullOrWhiteSpace(curitem.Name)) continue;
                    var isFood = i >= 0 ? Terraria.ID.ItemID.Sets.IsFood[i] : false;
                    var isRackable = (i >= 0 ? Terraria.ID.ItemID.Sets.CanBePlacedOnWeaponRacks[i] : false) || curitem.fishingPole > 0 || (curitem.damage > 0 && curitem.useStyle != 0);
                    var isDeprecated = i >= 0 ? Terraria.ID.ItemID.Sets.Deprecated[i] : false;
                    string name = curitem.Name;

                    int banner = 0;
                    banners.TryGetValue(i, out banner);

                    if (isDeprecated) { name += " (Deprecated)"; }
                    //curitem.SetDefaults(i);
                    sitems.Add(new ItemId(i, name, GetItemType(curitem))
                    {
                        IsFood = isFood,
                        Banner = banner,
                        Head = curitem.headSlot,
                        Body = curitem.bodySlot,
                        Legs = curitem.legSlot,
                        Accessory = curitem.accessory,
                        Rack = isRackable
                    });
                }
                catch
                {

                }
            }
            //sitems.AddRange(HardCodedItems);
            return sitems;
        }
    }
}