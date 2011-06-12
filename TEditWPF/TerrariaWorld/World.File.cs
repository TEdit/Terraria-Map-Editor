using System;
using System.ComponentModel;
using System.IO;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    public partial class World
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
                    wf.Header.WorldBounds = new RectF(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    int maxy = reader.ReadInt32();
                    int maxx = reader.ReadInt32();
                    wf.Header.MaxTiles = new PointInt32(maxx, maxy);
                    wf.ClearWorld();
                    wf.Header.SpawnTile = new PointInt32(reader.ReadInt32(), reader.ReadInt32());
                    wf.Header.WorldSurface = reader.ReadDouble();
                    wf.Header.WorldRockLayer = reader.ReadDouble();
                    wf.Header.Time = reader.ReadDouble();
                    wf.Header.IsDayTime = reader.ReadBoolean();
                    wf.Header.MoonPhase = reader.ReadInt32();
                    wf.Header.IsBloodMoon = reader.ReadBoolean();
                    wf.Header.DungeonEntrance = new PointInt32(reader.ReadInt32(), reader.ReadInt32());
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
                                    tile.Frame = new PointShort(reader.ReadInt16(), reader.ReadInt16());
                                else
                                    tile.Frame = new PointShort(-1, -1);

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
                            chest.Location = new PointInt32(reader.ReadInt32(), reader.ReadInt32());

                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                var item = new Item();
                                byte stackSize = reader.ReadByte();
                                if (stackSize > 0)
                                {
                                    string itemName = reader.ReadString();
                                    item.Name = itemName;
                                    item.StackSize = stackSize;
                                }
                                chest.Items.Add(item);
                            }

                            wf.Chests.Add(chest);
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
                            if (wf.Tiles[x, y].IsActive && (wf.Tiles[x, y].Type == 55)) // validate tile location
                            {
                                var sign = new Sign();
                                sign.Location = new PointInt32(x, y);
                                sign.Text = signText;

                                wf.Signs.Add(sign);
                            }
                        }
                    }

                    bool isNpcActive = reader.ReadBoolean();
                    for (int npcIndex = 0; isNpcActive; npcIndex++)
                    {
                        OnProgressChanged(wf, new ProgressChangedEventArgs(100, "Loading NPCs"));
                        var npc = new NPC();

                        npc.Name = reader.ReadString();
                        npc.Position = new PointFloat(reader.ReadSingle(), reader.ReadSingle());
                        npc.IsHomeless = reader.ReadBoolean();
                        npc.HomeTile = new PointInt32(reader.ReadInt32(), reader.ReadInt32());

                        wf.Npcs.Add(npc);

                        isNpcActive = reader.ReadBoolean();
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

        public void SaveFile(string filename)
        {
            string backupFileName = filename + ".Tedit";
            if (File.Exists(filename))
            {
                File.Copy(filename, backupFileName, true);
            }
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(this.Header.FileVersion);
                    writer.Write(this.Header.WorldName);
                    writer.Write(this.Header.WorldId);
                    writer.Write((int)this.Header.WorldBounds.Left);
                    writer.Write((int)this.Header.WorldBounds.Right);
                    writer.Write((int)this.Header.WorldBounds.Top);
                    writer.Write((int)this.Header.WorldBounds.Bottom);
                    writer.Write(this.Header.MaxTiles.Y);
                    writer.Write(this.Header.MaxTiles.X);
                    writer.Write(this.Header.SpawnTile.X);
                    writer.Write(this.Header.SpawnTile.Y);
                    writer.Write(this.Header.WorldSurface);
                    writer.Write(this.Header.WorldRockLayer);
                    writer.Write(this.Header.Time);
                    writer.Write(this.Header.IsDayTime);
                    writer.Write(this.Header.MoonPhase);
                    writer.Write(this.Header.IsBloodMoon);
                    writer.Write(this.Header.DungeonEntrance.X);
                    writer.Write(this.Header.DungeonEntrance.Y);
                    writer.Write(this.Header.IsBossDowned1);
                    writer.Write(this.Header.IsBossDowned2);
                    writer.Write(this.Header.IsBossDowned3);
                    writer.Write(this.Header.IsShadowOrbSmashed);
                    writer.Write(this.Header.IsSpawnMeteor);
                    writer.Write((byte)this.Header.ShadowOrbCount);
                    writer.Write(this.Header.InvasionDelay);
                    writer.Write(this.Header.InvasionSize);
                    writer.Write(this.Header.InvasionType);
                    writer.Write(this.Header.InvasionX);

                    for (int x = 0; x < this.Header.MaxTiles.X; x++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)x / (double)this.Header.MaxTiles.X * 100.0), "Saving World"));
                        //float num2 = ((float) i) / ((float) this.MaxTiles.X);
                        //string statusText = "Saving world data: " + ((int) ((num2 * 100f) + 1f)) + "%";
                        for (int y = 0; y < this.Header.MaxTiles.Y; y++)
                        {
                            writer.Write(this.Tiles[x, y].IsActive);
                            if (this.Tiles[x, y].IsActive)
                            {
                                writer.Write(this.Tiles[x, y].Type);
                                if (TileProperties.Tiles[this.Tiles[x, y].Type].IsFrameImportant)
                                {
                                    writer.Write(this.Tiles[x, y].Frame.X);
                                    writer.Write(this.Tiles[x, y].Frame.Y);
                                }
                            }
                            writer.Write(this.Tiles[x, y].IsLighted);
                            if (this.Tiles[x, y].Wall > 0)
                            {
                                writer.Write(true);
                                writer.Write(this.Tiles[x, y].Wall);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                            if (this.Tiles[x, y].Liquid > 0)
                            {
                                writer.Write(true);
                                writer.Write(this.Tiles[x, y].Liquid);
                                writer.Write(this.Tiles[x, y].IsLava);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                        }
                    }
                    for (int chestIndex = 0; chestIndex < World.MaxChests; chestIndex++)
                    {
                        if (chestIndex >= this.Chests.Count)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(this.Chests[chestIndex].Location.X);
                            writer.Write(this.Chests[chestIndex].Location.Y);
                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                writer.Write((byte)this.Chests[chestIndex].Items[slot].StackSize);
                                if (this.Chests[chestIndex].Items[slot].StackSize > 0)
                                {
                                    writer.Write(this.Chests[chestIndex].Items[slot].Name);
                                }
                            }
                        }
                    }
                    for (int signIndex = 0; signIndex < World.MaxSigns; signIndex++)
                    {
                        if (signIndex >= this.Signs.Count || this.Signs[signIndex].Text == null)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(this.Signs[signIndex].Text);
                            writer.Write(this.Signs[signIndex].Location.X);
                            writer.Write(this.Signs[signIndex].Location.Y);
                        }
                    }
                    foreach (var npc in this.Npcs)
                    {
                        writer.Write(true);
                        writer.Write(npc.Name);
                        writer.Write(npc.Position.X);
                        writer.Write(npc.Position.Y);
                        writer.Write(npc.IsHomeless);
                        writer.Write(npc.HomeTile.X);
                        writer.Write(npc.HomeTile.Y);
                    }
                    writer.Write(false);

                    // Write file info check version 7+
                    writer.Write(true);
                    writer.Write(this.Header.WorldName);
                    writer.Write(this.Header.WorldId);

                    writer.Close();
                }
            }
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
        }
    }
}
