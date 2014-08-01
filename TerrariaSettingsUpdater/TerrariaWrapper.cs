using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Terraria;

namespace TerrariaSettingsUpdater
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
    }
    public class TerrariaWrapper : Main
    {
        public Main Game { get; set; }

        public TerrariaWrapper()
        {

            Main.dedServ = true;
            Main.showSplash = false;
            this.Initialize();

            Terraria.Item curItem = new Item();
            for (int i = -1; i > -255; i--)
            {
                curItem = new Item();
                curItem.netDefaults(i);
                if (string.IsNullOrWhiteSpace(curItem.name))
                    break;

                HardCodedItems.Add(new ItemId(curItem.type, curItem.name, GetItemType(curItem)));
            }

        }

        private List<ItemId> HardCodedItems = new List<ItemId>();
        //new[]
        //    {
        //        new ItemId(1, "Gold Pickaxe"),
        //        new ItemId(4, "Gold Broadsword"),
        //        new ItemId(6, "Gold Shortsword"),
        //        new ItemId(10, "Gold Axe"),
        //        new ItemId(7, "Gold Hammer"),
        //        new ItemId(99, "Gold Bow"),
        //        new ItemId(1, "Silver Pickaxe"),
        //        new ItemId(4, "Silver Broadsword"),
        //        new ItemId(6, "Silver Shortsword"),
        //        new ItemId(10, "Silver Axe"),
        //        new ItemId(7, "Silver Hammer"),
        //        new ItemId(99, "Silver Bow"),
        //        new ItemId(1, "Copper Pickaxe"),
        //        new ItemId(4, "Copper Broadsword"),
        //        new ItemId(6, "Copper Shortsword"),
        //        new ItemId(10, "Copper Axe"),
        //        new ItemId(7, "Copper Hammer"),
        //        new ItemId(198, "Blue Phasesaber"),
        //        new ItemId(199, "Red Phasesaber"),
        //        new ItemId(200, "Green Phasesaber"),
        //        new ItemId(201, "Purple Phasesaber"),
        //        new ItemId(202, "White Phasesaber"),
        //        new ItemId(203, "Yellow Phasesaber"),
        //    };

        public Terraria.Item GetItem(int id)
        {
            var curitem = new Terraria.Item();
            curitem.SetDefaults(id);
            return curitem;

        }

        public IEnumerable<Terraria.NPC> GetNpcs()
        {
            int[] friends = new int[]
            {
                17,
                18,
                19,
                20,
                22,
                54,
                38,
                107,
                108,
                124,
                160,
                178,
                207,
                208,
                209,
                227,
                228,
                229
            };
            foreach (int id in friends)
            {
                var npc = new NPC();
                npc.SetDefaults(id);

                yield return npc;
            }            


        }

        public string GetWalls()
        {
            List<Terraria.Item> curItems = new List<Item>();
            for (int i = -255; i < maxItemTypes; i++)
            {
                var curitem = new Terraria.Item();
                curitem.SetDefaults(i);
                curItems.Add(curitem);
            }
            string output = "<Walls>\r\n";
            for (int i = 0; i < maxWallTypes; i++)
            {

                var creatingWall = curItems.FirstOrDefault(x => x.createWall == i);

                output += string.Format("<Wall Id=\"{0}\" Name=\"{2}\" Color=\"#FFFF00FF\" IsHouse=\"{1}\"/>\r\n",
                    i,
                    wallHouse[i],
                    creatingWall != null ? creatingWall.name : string.Empty);
                
            }

            return output + "</Walls>";
        }

        public string GetTiles()
        {
            List<Terraria.Item> curItems = new List<Item>();
            for (int i = 0; i < maxItemTypes; i++)
            {
                var curitem = new Terraria.Item();
                curitem.SetDefaults(i);
                curItems.Add(curitem);
            }
            
            string output = "<Tiles>\r\n";
            for (int i = 0; i < maxTileSets; i++)
            {
                var creatingItem = curItems.FirstOrDefault(x => x.createTile == i);
                
                output += string.Format("<Tile Id=\"{0}\" Name=\"{22}\" {5}{16}{17}{8} {21}\r\n",
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
                tilePick[i],
                tileShine[i],
                tileShine2[i],
                tileSolid[i] ? " Solid=\"true\"" : string.Empty,
                tileSolidTop[i] ? " SolidTop=\"true\"" : string.Empty,
                tileStone[i],
                tileTable[i],
                tileWaterDeath[i],
                (tileFrameImportant[i]) ? ">\r\n  <Frames>\r\n  </Frames>\r\n</Tile>" : " />",
                creatingItem != null ? creatingItem.name : string.Empty);

            }

            return output + "</Tiles>";
        }

        //public Terraria.Item GetN

        public List<string> Prefixes()
        {

            var result = new List<string> { string.Empty };
            var curitem = new Terraria.Item();
            curitem.name = "";
            for (int prefix = 1; prefix < byte.MaxValue; prefix++)
            {
                curitem.prefix = (byte)prefix;
                string affixName = curitem.AffixName();
                if (string.IsNullOrWhiteSpace(affixName))
                    break;

                result.Add(affixName);
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
            for (int i = -1; i > -255; i--)
            {
                var curItem = new Item();
                curItem.netDefaults(i);
                if (string.IsNullOrWhiteSpace(curItem.name))
                    break;

                sitems.Add(new ItemId(i, curItem.name, GetItemType(curItem)));
            }
            for (int i = 0; i < maxItemTypes; i++)
            {
                var curitem = new Terraria.Item();
                curitem.netDefaults(i);
                //curitem.SetDefaults(i);
                sitems.Add(new ItemId(i, curitem.name, GetItemType(curitem)));
            }
            //sitems.AddRange(HardCodedItems);
            return sitems;
        }
    }
}