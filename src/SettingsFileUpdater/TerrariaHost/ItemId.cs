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

namespace SettingsFileUpdater.TerrariaHost
{
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
                Task.Factory.StartNew(() => main.DedServ());
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

            StringBuilder output = new StringBuilder("  <Tiles>\r\n");
            for (int i = 0; i < maxTileSets; i++)
            {
                var creatingItem = curItems.FirstOrDefault(x => x.createTile == i);

                output.AppendFormat("    <Tile Id=\"{0}\" Name=\"{22}\" {5}{16}{17}{8} {21}\r\n",
                    i,
                tileAlch[i],
                tileAxe[i],
                tileBlockLight[i],
                tileDungeon[i],
                tileFrameImportant[i] ? " Framed=\"true\"" : string.Empty,
                tileHammer[i],
                tileLavaDeath[i],
                tileLighted[i] ? " Light=\"true\"" : string.Empty,
                tileMergeDirt[i],
                //tileName[i],
                tileNoAttach[i],
                tileNoFail[i],
                tileNoSunLight[i],
                "",
                tileShine[i],
                tileShine2[i],
                tileSolid[i] ? " Solid=\"true\"" : string.Empty,
                tileSolidTop[i] ? " SolidTop=\"true\"" : string.Empty,
                tileStone[i],
                tileTable[i],
                tileWaterDeath[i],
                (tileFrameImportant[i]) ? ">\r\n      <Frames>\r\n        <Frame UV=\"0, 0\" Name=\"\" Variety=\"\" />\r\n      </ Frames>\r\n    </Tile>" : " />",
                creatingItem != null ? creatingItem.Name : string.Empty);
            }


            output.Append("  </Tiles>");

            return output.ToString();
        }

        public string GetNpcsXml()
        {
            var npcs = GetNpcs();

            var output = new StringBuilder("  <Npcs>\r\n");
            foreach (var npc in npcs)
            {
                try
                {
                    output.AppendFormat("<Npc Id=\"{1}\" Name=\"{0}\" Frames=\"{2}\" />\r\n", Localize(npc.FullName), npc.netID, npc.width);
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
            //maxTileSets
            var sitems = new List<ItemId>();

            for (int i = -255; i < maxItemTypes; i++)
            {
                try
                {
                    var curitem = new Terraria.Item();
                    curitem.netDefaults(i);

                    if (string.IsNullOrWhiteSpace(curitem.Name)) continue;
                    var isFood = Terraria.ID.ItemID.Sets.IsFood[i];
                    var isRackable = Terraria.ID.ItemID.Sets.CanBePlacedOnWeaponRacks[i];
                    var isDeprecated = Terraria.ID.ItemID.Sets.CanBePlacedOnWeaponRacks[i];

                    string name = curitem.Name;
                    if (isDeprecated) { name += " (Legacy - DO NOT USE)";}
                    //curitem.SetDefaults(i);
                    sitems.Add(new ItemId(i, name, GetItemType(curitem))
                    {
                        IsFood = isFood,
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