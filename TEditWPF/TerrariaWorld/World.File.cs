using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using TEditWPF.TerrariaWorld.Structures;

namespace TEditWPF.TerrariaWorld
{
    public partial class World
    {
        public event ProgressChangedEventHandler ProgressChanged;
        protected void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }

        public void Load(string filename)
        {
            string ext = Path.GetExtension(filename);
            if (!string.Equals(ext, ".wld", StringComparison.CurrentCultureIgnoreCase))
                throw new ApplicationException("Invalid file");

            this.ClearWorld();

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int version = reader.ReadInt32();
                    if (version > World.CompatableVersion)
                    {
                        // handle version
                    }
                    this.Header.FileVersion = version;
                    this.Header.FileName = filename;
                    this.Header.WorldName = reader.ReadString();
                    this.Header.WorldId = reader.ReadInt32();
                    this.Header.WorldBounds = new RectF(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    int maxy = reader.ReadInt32();
                    int maxx = reader.ReadInt32();
                    this.Header.MaxTiles = new PointInt32(maxx, maxy);
                    this.ClearWorld();
                    this.Header.SpawnTile = new PointInt32(reader.ReadInt32(), reader.ReadInt32());
                    this.Header.WorldSurface = reader.ReadDouble();
                    this.Header.WorldRockLayer = reader.ReadDouble();
                    this.Header.Time = reader.ReadDouble();
                    this.Header.IsDayTime = reader.ReadBoolean();
                    this.Header.MoonPhase = reader.ReadInt32();
                    this.Header.IsBloodMoon = reader.ReadBoolean();
                    this.Header.DungeonEntrance = new PointInt32(reader.ReadInt32(), reader.ReadInt32());
                    this.Header.IsBossDowned1 = reader.ReadBoolean();
                    this.Header.IsBossDowned2 = reader.ReadBoolean();
                    this.Header.IsBossDowned3 = reader.ReadBoolean();
                    this.Header.IsShadowOrbSmashed = reader.ReadBoolean();
                    this.Header.IsSpawnMeteor = reader.ReadBoolean();
                    this.Header.ShadowOrbCount = reader.ReadByte();
                    this.Header.InvasionDelay = reader.ReadInt32();
                    this.Header.InvasionSize = reader.ReadInt32();
                    this.Header.InvasionType = reader.ReadInt32();
                    this.Header.InvasionX = reader.ReadDouble();

                    for (int x = 0; x < this.Header.MaxTiles.X; x++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)x / this.Header.MaxTiles.X * 100.0), "Loading Tiles"));

                        for (int y = 0; y < this.Header.MaxTiles.Y; y++)
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

                            this.Tiles[x, y] = tile;
                        }
                    }

                    for (int chestIndex = 0; chestIndex < World.MaxChests; chestIndex++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)chestIndex / World.MaxChests * 100.0), "Loading Chest Data"));

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

                            this.Chests[chestIndex] = chest;
                        }
                    }
                    for (int signIndex = 0; signIndex < World.MaxSigns; signIndex++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs((int)((double)signIndex / World.MaxSigns * 100.0), "Loading Sign Data"));

                        if (reader.ReadBoolean())
                        {
                            string signText = reader.ReadString();
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            if (this.Tiles[x, y].IsActive && (this.Tiles[x, y].Type == 55)) // validate tile location
                            {
                                var sign = new Sign();
                                sign.Location = new PointInt32(x, y);
                                sign.Text = signText;

                                this.Signs[signIndex] = sign;
                            }
                        }
                    }

                    bool isNpcActive = reader.ReadBoolean();
                    for (int npcIndex = 0; isNpcActive; npcIndex++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs(100, "Loading NPCs"));
                        var npc = new NPC();

                        npc.Name = reader.ReadString();
                        npc.Position = new PointFloat(reader.ReadSingle(), reader.ReadSingle());
                        npc.IsHomeless = reader.ReadBoolean();
                        npc.HomeTile = new PointInt32(reader.ReadInt32(), reader.ReadInt32());

                        this.Npcs[npcIndex] =npc;

                        isNpcActive = reader.ReadBoolean();
                    }

                    if (this.Header.FileVersion > 7)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs(100, "Checking format"));
                        bool test = reader.ReadBoolean();
                        var worldNameCheck = reader.ReadString();
                        var worldIdCheck = reader.ReadInt32();
                        if (!(test && string.Equals(worldNameCheck, this.Header.WorldName) && worldIdCheck == this.Header.WorldId))
                        {
                            // Test FAILED!
                            throw new ApplicationException("Invalid World File");
                        }
                    }

                    reader.Close();
                }
            }
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
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
                        if (this.Chests[chestIndex] == null)
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
                        if (this.Signs[signIndex] == null)
                        {
                            writer.Write(false);
                        }
                        else if (string.IsNullOrWhiteSpace(this.Signs[signIndex].Text))
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
                        if (npc == null)
                        {
                            writer.Write(false);
                            break;
                        }

                        writer.Write(true);
                        writer.Write(npc.Name);
                        writer.Write(npc.Position.X);
                        writer.Write(npc.Position.Y);
                        writer.Write(npc.IsHomeless);
                        writer.Write(npc.HomeTile.X);
                        writer.Write(npc.HomeTile.Y);
                    }
                    

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
