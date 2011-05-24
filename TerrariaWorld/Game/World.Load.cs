using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace TerrariaWorld.Game
{
    partial class World
    {

        public static event ProgressChangedEventHandler ProgressChanged;
        protected static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }

        public static World Load(string filename)
        {
            if (!TileProperties.IsInitialized)
                TileProperties.InitializeTileProperties();

            World wf = new World();


            var genRand = new Random((int)DateTime.Now.Ticks);

            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    try
                    {
                        int version = reader.ReadInt32();
                        if (version > World.COMPATIBLEVERSION)
                        {
                            // handle version incompat
                        }
                        wf.Header.FileName = filename;

                        wf.Header.WorldName = reader.ReadString();
                        wf.Header.WorldID = reader.ReadInt32();
                        wf.Header.WorldBounds = new Common.RectF(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        int maxy = reader.ReadInt32();
                        int maxx = reader.ReadInt32();
                        wf.Header.MaxTiles = new Common.Point(maxx, maxy);
                        wf.ClearWorld();
                        wf.Header.SpawnTile = new Common.Point(reader.ReadInt32(), reader.ReadInt32());
                        wf.Header.WorldSurface = reader.ReadDouble();
                        wf.Header.WorldRockLayer = reader.ReadDouble();
                        wf.Header.Time = reader.ReadDouble();
                        wf.Header.IsDayTime = reader.ReadBoolean();
                        wf.Header.MoonPhase = reader.ReadInt32();
                        wf.Header.IsBloodMoon = reader.ReadBoolean();
                        wf.Header.DungeonEntrance = new Common.Point(reader.ReadInt32(), reader.ReadInt32());
                        wf.Header.IsBossDowned1 = reader.ReadBoolean();
                        wf.Header.IsBossDowned2 = reader.ReadBoolean();
                        wf.Header.IsBossDowned3 = reader.ReadBoolean();
                        wf.Header.IsShadowOrbSmashed = reader.ReadBoolean();
                        wf.Header.IsSpawnMeteor = reader.ReadBoolean();
                        wf.Header.ShadowOrbCount = reader.ReadByte();
                        wf.Header.InvasionDelay = reader.ReadInt32();
                        wf.Header.InvasionSize = reader.ReadInt32();
                        wf.Header.InvasionType = reader.ReadInt32();
                        wf.Header.InvasionX = reader.ReadDouble();

                        for (int x = 0; x < wf.Header.MaxTiles.X; x++)
                        {
                            OnProgressChanged(wf, new ProgressChangedEventArgs((int)((double)x / (double)wf.Header.MaxTiles.X * 100.0), "Loading World"));

                            for (int y = 0; y < wf.Header.MaxTiles.Y; y++)
                            {
                                wf.Tiles[x, y] = new Tile();
                                wf.Tiles[x, y].IsActive = reader.ReadBoolean();
                                if (wf.Tiles[x, y].IsActive)
                                {
                                    wf.Tiles[x, y].Type = reader.ReadByte();
                                    if (TileProperties.IsFrameImportant[wf.Tiles[x, y].Type])
                                        wf.Tiles[x, y].Frame = new Common.PointS(reader.ReadInt16(), reader.ReadInt16());
                                    else
                                        wf.Tiles[x, y].Frame = new Common.PointS(-1, -1);

                                }
                                wf.Tiles[x, y].IsLighted = reader.ReadBoolean();
                                if (reader.ReadBoolean())
                                {
                                    wf.Tiles[x, y].Wall = reader.ReadByte();
                                }
                                if (reader.ReadBoolean())
                                {
                                    wf.Tiles[x, y].Liquid = reader.ReadByte();
                                    wf.Tiles[x, y].IsLava = reader.ReadBoolean();
                                }
                            }
                        }

                        for (int chestIndex = 0; chestIndex < World.MAXCHESTS; chestIndex++)
                        {
                            if (reader.ReadBoolean())
                            {
                                wf.Chests[chestIndex] = new Chest();
                                wf.Chests[chestIndex].Location = new Common.Point(reader.ReadInt32(), reader.ReadInt32());

                                for (int slot = 0; slot < Chest.MAXITEMS; slot++)
                                {
                                    wf.Chests[chestIndex].Items[slot] = new Item();
                                    byte stackSize = reader.ReadByte();
                                    if (stackSize > 0)
                                    {
                                        string itemName = reader.ReadString();
                                        wf.Chests[chestIndex].Items[slot].Name = itemName;
                                        wf.Chests[chestIndex].Items[slot].Stack = stackSize;
                                    }
                                }
                            }
                        }
                        for (int signIndex = 0; signIndex < World.MAXSIGNS; signIndex++)
                        {
                            if (reader.ReadBoolean())
                            {
                                string signText = reader.ReadString();
                                int x = reader.ReadInt32();
                                int y = reader.ReadInt32();
                                if (wf.Tiles[x, y].IsActive && (wf.Tiles[x, y].Type == 0x37))
                                {
                                    wf.Signs[signIndex] = new Sign();
                                    wf.Signs[signIndex].Location = new Common.Point(x, y);
                                    wf.Signs[signIndex].Text = signText;
                                }
                            }
                        }
                        bool flag = reader.ReadBoolean();
                        for (int npcIndex = 0; flag; npcIndex++)
                        {
                            wf.NPCs[npcIndex] = new NPC();
                            wf.NPCs[npcIndex].Name = reader.ReadString();
                            wf.NPCs[npcIndex].Position = new Common.PointF(reader.ReadSingle(), reader.ReadSingle());
                            wf.NPCs[npcIndex].IsHomeless = reader.ReadBoolean();
                            wf.NPCs[npcIndex].HomeTile = new Common.Point(reader.ReadInt32(), reader.ReadInt32());
                            flag = reader.ReadBoolean();
                        }
                        reader.Close();

                    }
                    catch (Exception err)
                    {
                        wf.Header.WorldName = "ERROR";
                        return wf;
                    }
                }
            }
            OnProgressChanged(wf, new ProgressChangedEventArgs(0, ""));
            return wf;
        }
    }
}
