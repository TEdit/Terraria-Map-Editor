using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SettingsFileUpdater.TerrariaHost.DataModel;
using TEdit.Common;
using TEdit.Geometry;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ObjectData;
using Terraria.Server;
using Terraria.Social;
using Terraria.WorldBuilding;

namespace SettingsFileUpdater.TerrariaHost
{

    public class TerrariaWrapper : Terraria.Main

    {
        public static new bool dedServ = true;

        public static TerrariaWrapper Initialize()
        {

            var terrariaAsm = typeof(Terraria.Program).Assembly;
            //var init = typeof(Terraria.Program).GetMethod("ForceLoadAssembly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new Type[] { typeof(Assembly), typeof(bool) }, null);
            //init.Invoke((object)null, new object[] { terrariaAsm, true });
            // Program.LaunchParameters = Utils.ParseArguements(args);
            //ThreadPool.SetMinThreads(8, 8);
            LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);
            //Program.InitializeConsoleOutput();
            //Program.SetupLogging();
            //Platform.Get<IWindowService>().SetQuickEditEnabled(false);

            var wrapper = RunGame();
            return wrapper;
        }

        private Thread _serverThread;

        public static TerrariaWrapper RunGame()
        {
            // Set dedServ BEFORE any access to Terraria.Main to ensure static initialization runs in server mode
            Main.dedServ = true;

            LanguageManager.Instance.SetLanguage(GameCulture.DefaultCulture);

            var game = new TerrariaWrapper();

            Lang.InitializeLegacyLocalization();
            SocialAPI.Initialize();
            MapHelper.Initialize();
            LaunchInitializer.LoadParameters((Main)game);
            TerrariaWrapper.OnEnginePreload += new Action(Terraria.Program.StartForceLoad);

            // Run DedServ on a background thread so data extractors can execute
            game._serverThread = new Thread(() =>
            {
                try
                {
                    if (TerrariaWrapper.dedServ)
                        game.DedServ();
                    game.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server thread error: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            });
            game._serverThread.IsBackground = true;
            game._serverThread.Name = "Server Thread";
            game._serverThread.Start();

            // Wait for content initialization to complete
            // DedServ calls Initialize() which populates ContentSamples
            Console.WriteLine("Waiting for server initialization...");
            while (ContentSamples.ItemsByType == null || ContentSamples.ItemsByType.Count == 0)
            {
                Thread.Sleep(250);
            }
            Console.WriteLine($"Server initialized. {ContentSamples.ItemsByType.Count} items loaded.");

            return game;
        }

        public static string Localize(string value) =>
            Language.GetTextValue(value);

        public TerrariaWrapper()
        {
            instance = this;
        }
        public void LoadWorld(string worldName)
        {
            bool cloudSave = false;
            WorldFileData worldFileData = new WorldFileData(Main.GetWorldPathFromName(worldName, cloudSave), cloudSave);
            ActiveWorldFileData = worldFileData;

            WorldFile.LoadWorld();
        }
        public void MakeWorldFile(string seedName, string worldname, int gameMode = 0)
        {
            GameMode = gameMode;
            worldName = worldname;
            bool cloudSave = false;

            WorldFileData worldFileData = new WorldFileData(Main.GetWorldPathFromName(worldname, cloudSave), cloudSave);
            if (Main.autoGenFileLocation != null && Main.autoGenFileLocation != "")
            {
                worldFileData = new WorldFileData(Main.autoGenFileLocation, cloudSave);
                Main.autoGenFileLocation = null;
            }

            worldFileData.Name = worldname;
            worldFileData.GameMode = GameMode;
            worldFileData.CreationTime = DateTime.Now;
            worldFileData.Metadata = FileMetadata.FromCurrentSettings(FileType.World);
            worldFileData.WorldGeneratorVersion = 1198295875585uL;
            worldFileData.UniqueId = Guid.NewGuid();
            if (Main.DefaultSeed == "")
            {
                worldFileData.SetSeedToRandom();
            }
            else
            {
                worldFileData.SetSeed(Main.DefaultSeed);
            }


            ActiveWorldFileData = worldFileData;


            seedName = seedName.Trim();
            ActiveWorldFileData.SetSeed(seedName);
            GenerationProgress progress = new GenerationProgress();


            Task newWorld = WorldGen.CreateNewWorld(progress);
            while (!newWorld.IsCompleted)
            {
                string msg = $"Generating World [{worldName}] {progress.TotalProgress:P0}: {progress.Message} {progress.Value:P0}";
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
                Task.Delay(250).Wait();
            }


            WorldFile.SaveWorld(false, true);
        }

        public void InitBase()
        {
            base.Initialize();
        }


        public Terraria.Item GetItem(int id)
        {
            return ContentSamples.ItemsByType[id];
        }

        public IEnumerable<Terraria.NPC> GetMobs()
        {
            return ContentSamples.NpcsByNetId.Values.Where(npc => !npc.friendly);
        }

        public IEnumerable<Terraria.NPC> GetNpcs(bool includeEnemy = false)
        {
            return ContentSamples.NpcsByNetId.Values.Where(npc => npc.friendly || includeEnemy);
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
        //    _tile.
        //    var color = MapHelper.Get.GetMapTileXnaColor(ref _tile).Hex4();
        //    return color;
        //}

        public string GetWallsXml()
        {
            List<Terraria.Item> curItems = new List<Item>();
            for (int i = -255; i < ItemID.Count; i++)
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
            for (int i = 0; i < WallID.Count; i++)
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
            for (int i = 0; i < ItemID.Count; i++)
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

            for (int i = 0; i < TileID.Count; i++)
            {

                var color = MapHelper.GetMapTileXnaColor(MapTile.Create((ushort)i, byte.MaxValue, 0));

                var node = origTiles.Elements().FirstOrDefault(e => e.Attribute("Id").Value == i.ToString());
                string origName = node?.Attribute("Name").Value;

                string colorHex = node?.Attributes().FirstOrDefault(a => a.Name == "Color")?.Value;

                if (string.IsNullOrEmpty(colorHex) || i > 659)
                {
                    colorHex = color.Hex4();
                    colorHex = colorHex.Substring(6, 2) + colorHex.Substring(0, 6);
                }

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
                    new XAttribute("Name", itemName),
                    new XAttribute("Color", colorHex));
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

        public string GetMobsText()
        {
            var npcs = GetMobs();

            var output = new StringBuilder("MOBS: ");
            foreach (var npc in npcs)
            {
                try
                {
                    output.Append(npc.netID);
                    output.Append(',');
                    //output.AppendFormat("<Npc Id=\"{1}\" Name=\"{0}\" Size=\"{2},{3}\" />\r\n", Localize(npc.FullName), npc.netID, npc.width, npc.height);
                }
                catch
                {

                }
            }
            output.Append("  </Npcs>");

            return output.ToString();
        }

        public IEnumerable<NpcData> GetNPCData()
        {
            foreach (var npc in GetNpcs(true))
            {
                int killId = BannerSystem.NPCtoBanner(npc.BannerID());
                if (killId <= 0 || npc.ExcludedFromDeathTally())
                    killId = -1;

                yield return new NpcData
                {
                    Id = npc.netID,
                    BannerId = killId,
                    IsTownNpc = npc.townNPC,
                    FullName = npc.FullName,
                    Name = Localize(npc.FullName),
                    BestiaryId = npc.GetBestiaryCreditId(),
                    CanTalk = npc.CanBeTalkedTo,
                    IsCritter = npc.CountsAsACritter,
                    IsKillCredit = npc.IsNPCValidForBestiaryKillCredit()
                };
            }
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
                var curitem = new Item();
                curitem.SetDefaults(item.Id);

                // this could probably be inverted to slot="head" etc.
                string attribs = string.Join(" ", new string[]
                {
                    (ItemID.Sets.IsAKite[item.Id] ? " IsKite=\"True\"" : ""),
                    ((curitem.createTile == 724 && curitem.makeNPC != 0) ? " IsCritter=\"True\"" : ""),
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

        static FieldInfo NPCKillCounterInfoElementInstance;

        static NPC GetNpcFromNPCKillCounterInfoElement(NPCKillCounterInfoElement element)
        {
            if (element == null) return null;

            if (NPCKillCounterInfoElementInstance == null)
            {
                NPCKillCounterInfoElementInstance = typeof(NPCKillCounterInfoElement).GetField("_instance", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            return (NPC)NPCKillCounterInfoElementInstance.GetValue(element);
        }

        public IEnumerable<NpcData> GetBestiaryData()
        {
            var bPopulator = new BestiaryDatabaseNPCsPopulator();
            var bestiary = new BestiaryDatabase();

            bPopulator.Populate(bestiary);

            BestiaryUICollectionInfo bestiaryUICollectionInfo = new BestiaryUICollectionInfo()
            {
                UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4
            };


            foreach (var entry in bestiary.Entries)
            {
                var info = entry.Info.OfType<NPCNetIdBestiaryInfoElement>().FirstOrDefault();
                var npc = GetNpcFromNPCKillCounterInfoElement(entry.Info.OfType<NPCKillCounterInfoElement>().FirstOrDefault());

                //var isEnemy = entry.UIInfoProvider
                if (npc == null)
                {
                    npc = new NPC();
                    npc.SetDefaults(info.NetId);
                }

                int killId = BannerSystem.NPCtoBanner(npc.BannerID());
                if (killId <= 0 || npc.ExcludedFromDeathTally())
                    killId = -1;

                var data = new NpcData
                {
                    Id = npc.netID,
                    BannerId = killId,
                    IsTownNpc = npc.townNPC,
                    BestiaryDisplayIndex = info.BestiaryDisplayIndex,
                    FullName = npc.FullName,
                    Name = Localize(npc.FullName),
                    BestiaryId = npc.GetBestiaryCreditId(),
                    CanTalk = npc.CanBeTalkedTo,
                    IsCritter = npc.CountsAsACritter,
                    IsKillCredit = npc.IsNPCValidForBestiaryKillCredit()
                };

                yield return data;
            }
        }

        /// <summary>
        /// Gets the full bestiary configuration.
        /// </summary>
        public BestiaryConfigJson GetBestiaryConfigJson()
        {
            var config = new BestiaryConfigJson
            {
                NpcData = GetBestiaryData().ToList()
            };

            return config;
        }

        public VersionData GetVersionData()
        {
            var framed = new List<int>();
            for (int a = 0; a < Main.tileFrameImportant.Length; a++)
            {
                if (Main.tileFrameImportant[a])
                    framed.Add(a);
            }

            return new VersionData
            {
                SaveVersion = Main.curRelease,
                GameVersion = Main.versionNumber,
                MaxTileId = TileID.Count - 1,
                MaxWallId = WallID.Count - 1,
                MaxItemId = ItemID.Count - 1,
                MaxNpcId = NPCID.Count - 1,
                MaxMoonId = Main.maxMoons,
                FramedTileIds = framed.ToArray()
            };
        }

        public string GetMaxCounts()
        {
            var data = GetVersionData();
            return string.Join("", new string[]
            {
                Environment.NewLine,
                "    {" + Environment.NewLine,
                "      \"saveVersion\": " + data.SaveVersion + "," + Environment.NewLine,
                "      \"gameVersion\": \"" + data.GameVersion + "\"," + Environment.NewLine,
                "      \"maxTileId\": " + data.MaxTileId + "," + Environment.NewLine,
                "      \"maxWallId\": " + data.MaxWallId + "," + Environment.NewLine,
                "      \"maxItemId\": " + data.MaxItemId + "," + Environment.NewLine,
                "      \"maxNpcId\": " + data.MaxNpcId + "," + Environment.NewLine,
                "      \"maxMoonId\": " + data.MaxMoonId + "," + Environment.NewLine,
                "      \"framedTileIds\": [ " + string.Join(", ", data.FramedTileIds) + " ]" + Environment.NewLine,
                "    },"
            });
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
            for (int bannerId = 0; bannerId < BannerSystem.MaxBannerTypes; bannerId++)
            {
                int itemId = BannerSystem.BannerToItem(bannerId);
                banners[itemId] = bannerId;
            }

            const int maxBanner = 289;
            //maxTileSets
            var sitems = new List<ItemId>();

            foreach (var itemKvp in ContentSamples.ItemsByType)
            {
                var i = itemKvp.Key;
                var curitem = itemKvp.Value;

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

            return sitems;
        }

        /// <summary>
        /// Returns the generated MapColors XML as a string (optional original file can override BuildSafe).
        /// </summary>
        public string GetMapColorsXml(string optionalOriginalPath = null)
        {
            return MapColorsExporter.BuildMapColorsXmlString(optionalOriginalPath);
        }

        /// <summary>
        /// Writes the generated MapColors XML to a file (optional original file can override BuildSafe).
        /// </summary>
        public void WriteMapColorsXml(string outputPath, string optionalOriginalPath = null)
        {
            MapColorsExporter.WriteMapColorsXml(outputPath, optionalOriginalPath);
        }

        #region JSON Output Methods

        /// <summary>
        /// Converts an XNA Color to TEditColor for JSON serialization.
        /// </summary>
        private static TEditColor ToTEditColor(Microsoft.Xna.Framework.Color xnaColor)
        {
            return new TEditColor(xnaColor.R, xnaColor.G, xnaColor.B, xnaColor.A);
        }

        /// <summary>
        /// Gets tile data in JSON-compatible format.
        /// </summary>
        public List<TileDataJson> GetTilesJson()
        {
            var result = new List<TileDataJson>();

            // Build item lookup for names
            var curItems = new List<Item>();
            for (int i = 0; i < ItemID.Count; i++)
            {
                try
                {
                    var curitem = new Item();
                    curitem.SetDefaults(i);
                    curItems.Add(curitem);
                }
                catch { }
            }

            for (int i = 0; i < TileID.Count; i++)
            {
                var color = MapHelper.GetMapTileXnaColor(MapTile.Create((ushort)i, byte.MaxValue, 0));
                var creatingItem = curItems.FirstOrDefault(x => x.createTile == i);
                string tileName = creatingItem?.Name ?? i.ToString();

                var tile = new TileDataJson
                {
                    Id = i,
                    Name = tileName,
                    Color = ToTEditColor(color),
                    IsLight = tileLighted[i],
                    SaveSlope = TileID.Sets.NonSolidSaveSlopes[i],
                    IsSolid = tileSolid[i],
                    IsSolidTop = tileSolidTop[i],
                    IsFramed = tileFrameImportant[i],
                    IsStone = TileID.Sets.Conversion.Stone[i],
                    CanBlend = TileID.Sets.Conversion.Stone[i] || TileID.Sets.Conversion.Grass[i] || tileSolid[i],
                    IsPlatform = TileID.Sets.Platforms[i],
                    IsCactus = i == TileID.Cactus,
                    IsGrass = TileID.Sets.Conversion.Grass[i] || TileID.Sets.GrassSpecial[i],
                };

                // Determine merge behavior
                if (TileID.Sets.Conversion.Grass[i] || TileID.Sets.GrassSpecial[i])
                    tile.MergeWith = TileID.Dirt;
                else if (TileID.Sets.Conversion.Stone[i])
                    tile.MergeWith = TileID.Dirt;

                // Handle framed tiles
                if (tileFrameImportant[i])
                {
                    TileObjectData data = TileObjectData.GetTileData(i, 0);
                    if (data != null)
                    {
                        var textureWidth = data.CoordinateWidth;
                        var textureHeight = data.CoordinateHeights.FirstOrDefault();
                        if (textureWidth != 16 || textureHeight != 16)
                        {
                            tile.TextureGrid = new Vector2Short((short)textureWidth, (short)textureHeight);
                        }

                        tile.FrameSize = new[] { new Vector2Short((short)data.Width, (short)data.Height) };

                        // Build frames list
                        var frames = new List<FrameDataJson>();
                        int style = 0;
                        while ((data = TileObjectData.GetTileData(i, style, 0)) != null)
                        {
                            var creatingSubItem = curItems.FirstOrDefault(x => x.createTile == i && x.placeStyle == style);
                            string frameName = creatingSubItem?.Name ?? tileName;

                            int altCount = data.AlternatesCount;
                            for (int alt = 0; alt < altCount || alt == 0; alt++)
                            {
                                data = TileObjectData.GetTileData(i, style, alt);
                                if (data == null) continue;

                                string anchor = null;
                                if (data.AnchorBottom.tileCount > 0) anchor = "Bottom";
                                else if (data.AnchorLeft.tileCount > 0) anchor = "Left";
                                else if (data.AnchorRight.tileCount > 0) anchor = "Right";
                                else if (data.AnchorTop.tileCount > 0) anchor = "Top";

                                frames.Add(new FrameDataJson
                                {
                                    Name = frameName,
                                    UV = new Vector2Short((short)(data.CoordinateFullWidth * alt), (short)(data.CoordinateFullHeight * style)),
                                    Size = new Vector2Short((short)data.Width, (short)data.Height),
                                    Anchor = anchor
                                });
                            }

                            style++;
                            if (creatingSubItem == null && style > 0) break;
                        }

                        if (frames.Count > 0)
                            tile.Frames = frames;
                    }
                    else
                    {
                        // Simple framed tile with no TileObjectData
                        tile.Frames = new List<FrameDataJson>
                        {
                            new FrameDataJson { Name = tileName, Size = new Vector2Short(1, 1) }
                        };
                    }
                }

                result.Add(tile);
            }

            return result;
        }

        /// <summary>
        /// Gets wall data in JSON-compatible format.
        /// </summary>
        public List<WallDataJson> GetWallsJson()
        {
            var result = new List<WallDataJson>();

            // Build item lookup for names
            var curItems = new List<Item>();
            for (int i = -255; i < ItemID.Count; i++)
            {
                try
                {
                    var curitem = new Item();
                    curitem.SetDefaults(i);
                    curItems.Add(curitem);
                }
                catch { }
            }

            for (int i = 0; i < WallID.Count; i++)
            {
                var creatingWall = curItems.FirstOrDefault(x => x.createWall == i);
                var color = MapHelper.GetMapTileXnaColor(MapTile.Create(0, byte.MaxValue, (byte)i));

                result.Add(new WallDataJson
                {
                    Id = i,
                    Name = creatingWall?.Name ?? (i == 0 ? "Sky" : $"Wall_{i}"),
                    Color = i == 0 ? TEditColor.Transparent : ToTEditColor(color)
                });
            }

            return result;
        }

        /// <summary>
        /// Gets item data in JSON-compatible format.
        /// </summary>
        public List<ItemDataJson> GetItemsJson()
        {
            var result = new List<ItemDataJson>();

            var banners = new Dictionary<int, int>();
            for (int bannerId = 0; bannerId < BannerSystem.MaxBannerTypes; bannerId++)
            {
                int itemId = BannerSystem.BannerToItem(bannerId);
                banners[itemId] = bannerId;
            }

            foreach (var itemKvp in ContentSamples.ItemsByType.OrderBy(kvp => kvp.Key))
            {
                var id = itemKvp.Key;
                var curitem = itemKvp.Value;

                if (string.IsNullOrWhiteSpace(curitem.Name)) continue;

                var isFood = id >= 0 && ItemID.Sets.IsFood[id];
                var isKite = id >= 0 && ItemID.Sets.IsAKite[id];
                var isCritter = curitem.createTile == 724 && curitem.makeNPC != 0;
                var isRackable = (id >= 0 && ItemID.Sets.CanBePlacedOnWeaponRacks[id]) ||
                                 curitem.fishingPole > 0 ||
                                 (curitem.damage > 0 && curitem.useStyle != 0);
                var isDeprecated = id >= 0 && ItemID.Sets.Deprecated[id];

                string name = curitem.Name;
                if (isDeprecated) name += " (Deprecated)";

                banners.TryGetValue(id, out int tally);

                result.Add(new ItemDataJson
                {
                    Id = id,
                    Name = name,
                    Scale = curitem.scale,
                    MaxStackSize = curitem.maxStack,
                    IsFood = isFood,
                    IsKite = isKite,
                    IsCritter = isCritter,
                    IsAccessory = curitem.accessory,
                    IsRackable = isRackable,
                    Head = curitem.headSlot > 0 ? curitem.headSlot : null,
                    Body = curitem.bodySlot > 0 ? curitem.bodySlot : null,
                    Legs = curitem.legSlot > 0 ? curitem.legSlot : null,
                    Tally = tally,
                    Rarity = GetRarityName(curitem.rare)
                });
            }

            return result;
        }

        /// <summary>
        /// Maps a rarity number to its name for JSON output.
        /// Returns null for White (rarity 0) since that's the default.
        /// </summary>
        private static string? GetRarityName(int rarity) => rarity switch
        {
            -13 => "Master",
            -12 => "Expert",
            -11 => "Quest",
            -1 => "Gray",
            1 => "Blue",
            2 => "Green",
            3 => "Orange",
            4 => "LightRed",
            5 => "Pink",
            6 => "LightPurple",
            7 => "Lime",
            8 => "Yellow",
            9 => "Cyan",
            10 => "Red",
            11 => "Purple",
            _ => null  // White (0) or unknown rarities
        };

        /// <summary>
        /// Gets friendly NPC data in JSON-compatible format.
        /// </summary>
        public List<NpcDataJson> GetNpcsJson()
        {
            var result = new List<NpcDataJson>();

            foreach (var npc in GetNpcs(false))
            {
                result.Add(new NpcDataJson
                {
                    Id = npc.netID,
                    Name = Localize(npc.FullName),
                });
            }

            return result;
        }

        /// <summary>
        /// Gets prefix data in JSON-compatible format.
        /// </summary>
        public List<PrefixDataJson> GetPrefixesJson()
        {
            var result = new List<PrefixDataJson>();
            var prefixes = Prefixes();

            for (int i = 0; i < prefixes.Count; i++)
            {
                result.Add(new PrefixDataJson
                {
                    Id = i,
                    Name = prefixes[i]
                });
            }

            return result;
        }

        /// <summary>
        /// Gets paint data in JSON-compatible format.
        /// </summary>
        public List<PaintDataJson> GetPaintsJson()
        {
            var result = new List<PaintDataJson>();

            // Paint colors are defined in PaintID
            // We'll use reflection to get paint names and hardcoded colors
            var paintColors = new Dictionary<int, (string Name, TEditColor Color)>
            {
                { 0, ("None", TEditColor.Transparent) },
                { 1, ("Red", new TEditColor(255, 0, 0, 255)) },
                { 2, ("Orange", new TEditColor(255, 127, 0, 255)) },
                { 3, ("Yellow", new TEditColor(255, 255, 0, 255)) },
                { 4, ("Lime", new TEditColor(127, 255, 0, 255)) },
                { 5, ("Green", new TEditColor(0, 255, 0, 255)) },
                { 6, ("Teal", new TEditColor(0, 255, 127, 255)) },
                { 7, ("Cyan", new TEditColor(0, 255, 255, 255)) },
                { 8, ("Sky Blue", new TEditColor(0, 127, 255, 255)) },
                { 9, ("Blue", new TEditColor(0, 0, 255, 255)) },
                { 10, ("Purple", new TEditColor(127, 0, 255, 255)) },
                { 11, ("Violet", new TEditColor(255, 0, 255, 255)) },
                { 12, ("Pink", new TEditColor(255, 0, 127, 255)) },
                { 13, ("Deep Red", new TEditColor(255, 0, 0, 255)) },
                { 14, ("Deep Orange", new TEditColor(255, 127, 0, 255)) },
                { 15, ("Deep Yellow", new TEditColor(255, 255, 0, 255)) },
                { 16, ("Deep Lime", new TEditColor(127, 255, 0, 255)) },
                { 17, ("Deep Green", new TEditColor(0, 255, 0, 255)) },
                { 18, ("Deep Teal", new TEditColor(0, 255, 127, 255)) },
                { 19, ("Deep Cyan", new TEditColor(0, 255, 255, 255)) },
                { 20, ("Deep Sky Blue", new TEditColor(0, 127, 255, 255)) },
                { 21, ("Deep Blue", new TEditColor(0, 0, 255, 255)) },
                { 22, ("Deep Purple", new TEditColor(127, 0, 255, 255)) },
                { 23, ("Deep Violet", new TEditColor(255, 0, 255, 255)) },
                { 24, ("Deep Pink", new TEditColor(255, 0, 127, 255)) },
                { 25, ("Black", new TEditColor(75, 75, 75, 255)) },
                { 26, ("White", new TEditColor(255, 255, 255, 255)) },
                { 27, ("Gray", new TEditColor(175, 175, 175, 255)) },
                { 28, ("Brown", new TEditColor(255, 178, 125, 255)) },
                { 29, ("Shadow", new TEditColor(25, 25, 25, 255)) },
                { 30, ("Negative", new TEditColor(255, 255, 255, 255)) },
                { 31, ("Illuminant Paint", new TEditColor(255, 255, 255, 255)) },
            };

            foreach (var kvp in paintColors.OrderBy(x => x.Key))
            {
                result.Add(new PaintDataJson
                {
                    Id = kvp.Key,
                    Name = kvp.Value.Name,
                    Color = kvp.Value.Color
                });
            }

            return result;
        }

        /// <summary>
        /// Gets global colors (Sky, Dirt, Rock, Hell, Water, Lava, Honey, Shimmer) in JSON-compatible format.
        /// These are special map colors used for rendering backgrounds and liquids.
        /// </summary>
        public List<GlobalColorJson> GetGlobalColorsJson()
        {
            var result = new List<GlobalColorJson>();

            // Access MapHelper's colorLookup array via reflection
            var mapHelperType = typeof(Terraria.Map.MapHelper);
            var bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var fiColorLookup = mapHelperType.GetField("colorLookup", bf);
            if (fiColorLookup == null)
            {
                Console.WriteLine("Warning: Could not access MapHelper.colorLookup");
                return result;
            }

            var colorLookup = (Microsoft.Xna.Framework.Color[])fiColorLookup.GetValue(null);
            if (colorLookup == null)
            {
                Console.WriteLine("Warning: MapHelper.colorLookup is null");
                return result;
            }

            // The colorLookup array layout (after tiles and walls) is:
            // - 3 liquid colors (water, lava, honey)
            // - 256 sky colors
            // - 256 dirt colors
            // - 256 rock colors
            // - 1 hell color
            // - Additional entries may include shimmer

            // Find where tiles/walls end in the lookup
            var fiTileLookup = mapHelperType.GetField("tileLookup", bf);
            var fiTileOptionCounts = mapHelperType.GetField("tileOptionCounts", bf);
            var fiWallLookup = mapHelperType.GetField("wallLookup", bf);
            var fiWallOptionCounts = mapHelperType.GetField("wallOptionCounts", bf);

            var tileLookup = (ushort[])fiTileLookup?.GetValue(null);
            var tileOptionCounts = (int[])fiTileOptionCounts?.GetValue(null);
            var wallLookup = (ushort[])fiWallLookup?.GetValue(null);
            var wallOptionCounts = (int[])fiWallOptionCounts?.GetValue(null);

            // Calculate the end index for walls to find global colors
            int globalColorStart = 1; // Start after empty slot
            if (tileLookup != null && tileOptionCounts != null)
            {
                for (int i = 0; i < TileID.Count && i < tileOptionCounts.Length; i++)
                {
                    int count = tileOptionCounts[i];
                    if (count > 0 && tileLookup[i] > 0)
                        globalColorStart = Math.Max(globalColorStart, tileLookup[i] + count);
                }
            }
            if (wallLookup != null && wallOptionCounts != null)
            {
                for (int i = 0; i < WallID.Count && i < wallOptionCounts.Length; i++)
                {
                    int count = wallOptionCounts[i];
                    if (count > 0 && wallLookup[i] > 0)
                        globalColorStart = Math.Max(globalColorStart, wallLookup[i] + count);
                }
            }

            // After tiles/walls come: 3 liquids, then sky(256), dirt(256), rock(256), hell(1), shimmer(1)
            int liquidStart = globalColorStart;

            // Add liquid colors
            string[] liquidNames = { "Water", "Lava", "Honey" };
            for (int i = 0; i < 3 && liquidStart + i < colorLookup.Length; i++)
            {
                var c = colorLookup[liquidStart + i];
                result.Add(new GlobalColorJson
                {
                    Name = liquidNames[i],
                    Color = ToTEditColor(c)
                });
            }

            // Sky colors (256 shades, we take the middle one as representative)
            int skyStart = liquidStart + 3;
            if (skyStart + 127 < colorLookup.Length)
            {
                var c = colorLookup[skyStart + 127]; // Middle sky shade
                result.Add(new GlobalColorJson
                {
                    Name = "Sky",
                    Color = ToTEditColor(c)
                });
            }

            // Dirt colors (256 shades)
            int dirtStart = skyStart + 256;
            if (dirtStart + 127 < colorLookup.Length)
            {
                var c = colorLookup[dirtStart + 127]; // Middle dirt shade
                result.Add(new GlobalColorJson
                {
                    Name = "Dirt",
                    Color = ToTEditColor(c)
                });
            }

            // Rock colors (256 shades)
            int rockStart = dirtStart + 256;
            if (rockStart + 127 < colorLookup.Length)
            {
                var c = colorLookup[rockStart + 127]; // Middle rock shade
                result.Add(new GlobalColorJson
                {
                    Name = "Rock",
                    Color = ToTEditColor(c)
                });
            }

            // Hell color (1)
            int hellStart = rockStart + 256;
            if (hellStart < colorLookup.Length)
            {
                var c = colorLookup[hellStart];
                result.Add(new GlobalColorJson
                {
                    Name = "Hell",
                    Color = ToTEditColor(c)
                });
            }

            // Shimmer (if available, added in 1.4.4)
            int shimmerStart = hellStart + 1;
            if (shimmerStart < colorLookup.Length)
            {
                var c = colorLookup[shimmerStart];
                result.Add(new GlobalColorJson
                {
                    Name = "Shimmer",
                    Color = ToTEditColor(c)
                });
            }

            // Add rarity colors using Terraria's Item.GetPopupRarityColor
            // These are used for DisplayJar and other item displays
            var rarityNames = new Dictionary<int, string>
            {
                { -13, "Rarity_Master" },      // Animated in-game (fiery)
                { -12, "Rarity_Expert" },      // Animated in-game (rainbow)
                { -11, "Rarity_Quest" },
                { -1, "Rarity_Gray" },
                { 0, "Rarity_White" },
                { 1, "Rarity_Blue" },
                { 2, "Rarity_Green" },
                { 3, "Rarity_Orange" },
                { 4, "Rarity_LightRed" },
                { 5, "Rarity_Pink" },
                { 6, "Rarity_LightPurple" },
                { 7, "Rarity_Lime" },
                { 8, "Rarity_Yellow" },
                { 9, "Rarity_Cyan" },
                { 10, "Rarity_Red" },
                { 11, "Rarity_Purple" },
            };

            foreach (var kvp in rarityNames.OrderBy(x => x.Key))
            {
                Microsoft.Xna.Framework.Color rarityColor;

                // Animated rarities need static representative colors
                if (kvp.Key == -13)
                {
                    // Master: Fiery orange (peak of animation)
                    rarityColor = new Microsoft.Xna.Framework.Color(255, 140, 0);
                }
                else if (kvp.Key == -12)
                {
                    // Expert: Rainbow (using magenta as representative)
                    rarityColor = new Microsoft.Xna.Framework.Color(255, 0, 255);
                }
                else
                {
                    // Use Terraria's actual rarity color
                    rarityColor = Item.GetPopupRarityColor(kvp.Key);
                }

                result.Add(new GlobalColorJson
                {
                    Name = kvp.Value,
                    Color = ToTEditColor(rarityColor)
                });
            }

            return result;
        }

        #endregion
    }
}
