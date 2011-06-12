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
            string ext = Path.GetExtension(filename);
            if (!string.Equals(ext, ".wld", StringComparison.CurrentCultureIgnoreCase))
                throw new ApplicationException("Invalid file");

            var wf = new World();

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int version = reader.ReadInt32();
                    if (version > World.CompatableVersion)
                    {
                        // handle version
                    }
                    wf.Header.FileVersion = version;
                    wf.Header.FileName = filename;
                    wf.Header.WorldName = reader.ReadString();
                    wf.Header.WorldId = reader.ReadInt32();
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
                        OnProgressChanged(wf, new ProgressChangedEventArgs((int)((double)x / wf.Header.MaxTiles.X * 100.0), "Loading Tiles"));

                        for (int y = 0; y < wf.Header.MaxTiles.Y; y++)
                        {
                            var tile = new Tile();

                            tile.IsActive = reader.ReadBoolean();

                            if (tile.IsActive)
                            {
                                tile.Type = reader.ReadByte();
                                if (TileProperties.Tiles[tile.Type].IsFrameImportant)
                                    tile.Frame = new Common.PointS(reader.ReadInt16(), reader.ReadInt16());
                                else
                                    tile.Frame = new Common.PointS(-1, -1);

                            }

                            tile.IsLighted = reader.ReadBoolean();

                            if (reader.ReadBoolean())
                            {
                                tile.Wall = reader.ReadByte();
                            }

                            if (reader.ReadBoolean())
                            {
                                tile.Liquid = reader.ReadByte();
                                tile.IsLava = reader.ReadBoolean();
                            }

                            wf.Tiles[x, y] = wf.Tiles[x, y];
                        }
                    }

                    for (int chestIndex = 0; chestIndex < World.MaxChests; chestIndex++)
                    {
                        OnProgressChanged(wf, new ProgressChangedEventArgs((int)((double)chestIndex / World.MaxChests * 100.0), "Loading Chest Data"));

                        if (reader.ReadBoolean())
                        {
                            var chest = new Chest();
                            chest.Location = new Common.Point(reader.ReadInt32(), reader.ReadInt32());

                            for (int slot = 0; slot < Chest.MAXITEMS; slot++)
                            {
                                var item = new Item();
                                byte stackSize = reader.ReadByte();
                                if (stackSize > 0)
                                {
                                    string itemName = reader.ReadString();
                                    item.Name = itemName;
                                    item.Stack = stackSize;
                                }
                                chest.Items[slot] = item;
                            }

                            wf.Chests[chestIndex] = chest;
                        }
                    }
                    for (int signIndex = 0; signIndex < World.MaxSigns; signIndex++)
                    {
                        OnProgressChanged(wf, new ProgressChangedEventArgs((int)((double)signIndex / World.MaxSigns * 100.0), "Loading Sign Data"));

                        if (reader.ReadBoolean())
                        {
                            string signText = reader.ReadString();
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            if (wf.Tiles[x, y].IsActive && (wf.Tiles[x, y].Type == 55))
                            {
                                var sign= new Sign();
                                sign.Location = new Common.Point(x, y);
                                sign.Text = signText;

                                wf.Signs[signIndex] = sign;
                            }
                        }
                    }
                    bool flag = reader.ReadBoolean();
                    for (int npcIndex = 0; flag; npcIndex++)
                    {
                        OnProgressChanged(wf, new ProgressChangedEventArgs(100, "Loading NPCs"));
                        var npc = new NPC();

                        npc.Name = reader.ReadString();
                        npc.Position = new Common.PointF(reader.ReadSingle(), reader.ReadSingle());
                        npc.IsHomeless = reader.ReadBoolean();
                        npc.HomeTile = new Common.Point(reader.ReadInt32(), reader.ReadInt32());
                        
                        wf.NPCs[npcIndex] = new NPC();
                   

                        flag = reader.ReadBoolean();
                    }
                    if (wf.Header.FileVersion > 7)
                    {
                        OnProgressChanged(wf, new ProgressChangedEventArgs(100, "Checking format"));
                        bool test = reader.ReadBoolean();
                        var worldNameCheck = reader.ReadString();
                        var worldIdCheck = reader.ReadInt32();
                        if (!(test && string.Equals(worldNameCheck, wf.Header.WorldName) && worldIdCheck == wf.Header.WorldId))
                        {
                            // Test FAILED!
                            throw new ApplicationException("Invalid World File");
                        }
                    }

                    reader.Close();
                }
            }
            OnProgressChanged(wf, new ProgressChangedEventArgs(0, ""));
            return wf;
        }
    }
}
