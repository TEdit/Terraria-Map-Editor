using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TEdit.Common.Structures;
using TEdit.RenderWorld;

namespace TEdit.TerrariaWorld
{
    public partial class World
    {
        private bool _isUsingIo = true;
        private bool _isValid;
        private bool _isSaved; 

        public bool IsSaved
        {
            get { return _isSaved; }
            set { SetProperty(ref _isSaved, ref value, "IsSaved"); }
        }
        public bool IsValid
        {
            get { return _isValid; }
            set { SetProperty(ref _isValid, ref value, "IsValid"); }
        }
        public bool IsUsingIo
        {
            get { return _isUsingIo; }
            set{ SetProperty(ref _isUsingIo, ref value, "IsUsingIo");}
        }

        public event ProgressChangedEventHandler ProgressChanged;

        protected void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }

        public void NewWorld(int width, int height, int seed = -1)
        {
            var genRand = seed <= 0 ? new Random((int)DateTime.Now.Ticks) : new Random(seed);
            IsValid = false;
            IsUsingIo = true;
            IsSaved = false;

            Header.FileVersion = CompatableVersion;
            Header.FileName = "";
            Header.WorldName = "TEdit World";
            Header.WorldId = genRand.Next(int.MaxValue);

            Header.WorldBounds = new RectF(0, width, 0, height);
            Header.MaxTiles = new PointInt32(width, height);
            ClearWorld();
            Header.SpawnTile = new PointInt32(width / 2, height / 3);
            Header.WorldSurface = height / 3;
            Header.WorldRockLayer = 2 * height / 3;
            Header.Time = 13500;
            Header.IsDayTime = true;
            Header.MoonPhase = 0;
            Header.IsBloodMoon = false;
            Header.DungeonEntrance = new PointInt32(width / 5, height / 3);
            Header.IsBossDowned1 = false;
            Header.IsBossDowned2 = false;
            Header.IsBossDowned3 = false;
            Header.IsShadowOrbSmashed = false;
            Header.IsSpawnMeteor = false;
            Header.ShadowOrbCount = 0;
            Header.InvasionDelay = 0;
            Header.InvasionSize = 0;
            Header.InvasionType = 0;
            Header.InvasionX = 0;
            ClearWorld();
            ResetTime();

            for (int x = 0; x < Header.MaxTiles.X; x++)
            {
                OnProgressChanged(this,
                                  new ProgressChangedEventArgs((int)((double)x / Header.MaxTiles.X * 100.0),
                                                               "Loading Tiles"));

                for (int y = 0; y < Header.MaxTiles.Y; y++)
                {
                    Tiles[x, y] = new Tile();
                }
            }
            IsValid = true;
            IsUsingIo = false;
        }
        
        private bool ValidatePlacement(PointInt32 location, PointShort size, FramePlacement frame)
        {
            // TODO: Implement frame validations
            return false;
        }

        private bool ValidateTorch(int x, int y)
        {
            int left = x - 1;
            int right = x + 1;
            int bottom = y + 1;

            if (left >= 0)
                if (WorldSettings.Tiles[Tiles[left, y].Type].IsSolid && Tiles[left, y].IsActive)
                    return true;

            if (right < Header.MaxTiles.X)
                if (WorldSettings.Tiles[Tiles[right, y].Type].IsSolid && Tiles[left, y].IsActive)
                    return true;

            if (bottom < Header.MaxTiles.Y)
                if ((WorldSettings.Tiles[Tiles[x, bottom].Type].IsSolid || WorldSettings.Tiles[Tiles[x, bottom].Type].IsSolidTop) && Tiles[left, y].IsActive)
                    return true;

            return false;
        }

        public void Validate()
        {
            List<string> log = new List<string>();
            IsUsingIo = true;
            for (int x = 0; x < Header.MaxTiles.X; x++)
            {
                OnProgressChanged(this,new ProgressChangedEventArgs((int)(x / (double)Header.MaxTiles.X * 100.0),"Validating Tiles"));

                for (int y = 0; y < Header.MaxTiles.Y; y++)
                {
                    //validate chest entry exists
                    var curType = Tiles[x, y].Type;

                    switch (curType)
                    {
                        case 4:
                            // torch validation
                            if (!ValidateTorch(x, y))
                                Tiles[x, y].IsActive = false;
                            break;
                        case 21:
                            // Validate Chest
                            if (GetChestAtTile(x, y) == null) Chests.Add(new Chest(new PointInt32(x, y)));
                            log.Add(string.Format("added empty chest content [{0},{1}]",x,y));
                            break;
                        case 55:
                        case 85:
                            // Validate Sign/Tombstone
                            if (GetSignAtTile(x, y) == null) Signs.Add(new Sign("", new PointInt32(x, y)));
                            log.Add(string.Format("added blank sign text [{0},{1}]", x, y));
                            break;
                    }
                }
            }

            OnProgressChanged(this, new ProgressChangedEventArgs(33, "Validating Chests"));
            foreach (var chest in Chests.ToList())
            {
                //if (Chests[chestIndex] == null)
                var loc = chest.Location;
                int locType = Tiles[loc.X, loc.Y].Type;

                if (locType != 21)
                {
                    Chests.Remove(chest);
                    log.Add(string.Format("removed missing chest {0}", loc));
                }
            }

            OnProgressChanged(this, new ProgressChangedEventArgs(66, "Validating Signs"));
            foreach (var sign in Signs.ToList())
            {
                //if (Chests[chestIndex] == null)
                var loc = sign.Location;
                int locType = Tiles[loc.X, loc.Y].Type;

                if (locType != 55 && locType != 85)
                {
                    Signs.Remove(sign);
                    log.Add(string.Format("removed missing sign {0}", loc));
                }
            }

            foreach (NPC npc in Npcs)
            {
                // no validation yet...
            }
            IsUsingIo = false;
            OnProgressChanged(this, new ProgressChangedEventArgs(0, "Validation Complete."));
        }

        public void Load(string filename)
        {
            string ext = Path.GetExtension(filename);
            if (!(string.Equals(ext, ".wld", StringComparison.CurrentCultureIgnoreCase) ||
                  string.Equals(ext, ".bak", StringComparison.CurrentCultureIgnoreCase) ||
                  string.Equals(ext, ".Tedit", StringComparison.CurrentCultureIgnoreCase)))
                throw new ApplicationException("Invalid file");

            IsUsingIo = true;
            IsValid = false;
            ClearWorld();

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int version = reader.ReadInt32();
                    if (version > CompatableVersion)
                    {
                        // handle version
                    }
                    Header.FileVersion = version;
                    Header.FileName = filename;
                    Header.WorldName = reader.ReadString();
                    Header.WorldId = reader.ReadInt32();
                    Header.WorldBounds = new RectF(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
                                                   reader.ReadInt32());
                    int maxy = reader.ReadInt32();
                    int maxx = reader.ReadInt32();
                    Header.MaxTiles = new PointInt32(maxx, maxy);
                    ClearWorld();
                    Header.SpawnTile = new PointInt32(reader.ReadInt32(), reader.ReadInt32());
                    Header.WorldSurface = reader.ReadDouble();
                    Header.WorldRockLayer = reader.ReadDouble();
                    Header.Time = reader.ReadDouble();
                    Header.IsDayTime = reader.ReadBoolean();
                    Header.MoonPhase = reader.ReadInt32();
                    Header.IsBloodMoon = reader.ReadBoolean();
                    Header.DungeonEntrance = new PointInt32(reader.ReadInt32(), reader.ReadInt32());
                    Header.IsBossDowned1 = reader.ReadBoolean();
                    Header.IsBossDowned2 = reader.ReadBoolean();
                    Header.IsBossDowned3 = reader.ReadBoolean();
                    Header.IsShadowOrbSmashed = reader.ReadBoolean();
                    Header.IsSpawnMeteor = reader.ReadBoolean();
                    Header.ShadowOrbCount = reader.ReadByte();
                    Header.InvasionDelay = reader.ReadInt32();
                    Header.InvasionSize = reader.ReadInt32();
                    Header.InvasionType = reader.ReadInt32();
                    Header.InvasionX = reader.ReadDouble();

                    for (int x = 0; x < Header.MaxTiles.X; x++)
                    {
                        OnProgressChanged(this,
                                          new ProgressChangedEventArgs((int)((double)x / Header.MaxTiles.X * 100.0),
                                                                       "Loading Tiles"));

                        for (int y = 0; y < Header.MaxTiles.Y; y++)
                        {
                            var tile = new Tile();

                            tile.IsActive = reader.ReadBoolean();

                            
                            if (tile.IsActive)
                            {
                                tile.Type = reader.ReadByte();

                                if (WorldSettings.Tiles[tile.Type].IsFramed)
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

                            Tiles[x, y] = tile;
                        }
                    }

                    for (int chestIndex = 0; chestIndex < MaxChests; chestIndex++)
                    {
                        OnProgressChanged(this,
                                          new ProgressChangedEventArgs((int)((double)chestIndex / MaxChests * 100.0),
                                                                       "Loading Chest Data"));

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

                            //Chests[chestIndex] = chest;
                            Chests.Add(chest);
                        }
                    }
                    for (int signIndex = 0; signIndex < MaxSigns; signIndex++)
                    {
                        OnProgressChanged(this,
                                          new ProgressChangedEventArgs((int)((double)signIndex / MaxSigns * 100.0),
                                                                       "Loading Sign Data"));

                        if (reader.ReadBoolean())
                        {
                            string signText = reader.ReadString();
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            if (Tiles[x, y].IsActive && (Tiles[x, y].Type == 55 || Tiles[x, y].Type == 85))
                            // validate tile location
                            {
                                var sign = new Sign();
                                sign.Location = new PointInt32(x, y);
                                sign.Text = signText;

                                //Signs[signIndex] = sign;
                                Signs.Add(sign);
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

                        //Npcs[npcIndex] = npc;
                        Npcs.Add(npc);
                        isNpcActive = reader.ReadBoolean();
                    }

                    if (Header.FileVersion > 7)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs(100, "Checking format"));
                        bool test = reader.ReadBoolean();
                        string worldNameCheck = reader.ReadString();
                        int worldIdCheck = reader.ReadInt32();
                        if (!(test && string.Equals(worldNameCheck, Header.WorldName) && worldIdCheck == Header.WorldId))
                        {
                            // Test FAILED!
                            IsUsingIo = false;
                            reader.Close();
                            throw new ApplicationException("Invalid World File");
                        }
                    }
                    
                    reader.Close();
                }
            }
            IsValid = true;
            IsUsingIo = false;
            IsSaved = true;
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
        }

        public void SaveFile(string filename)
        {
            Validate();

            IsUsingIo = true;
            
            string backupFileName = filename + ".Tedit";
            if (File.Exists(filename))
            {
                File.Copy(filename, backupFileName, true);
            }
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Header.FileVersion);
                    writer.Write(Header.WorldName);
                    writer.Write(Header.WorldId);
                    writer.Write((int)Header.WorldBounds.Left);
                    writer.Write((int)Header.WorldBounds.Right);
                    writer.Write((int)Header.WorldBounds.Top);
                    writer.Write((int)Header.WorldBounds.Bottom);
                    writer.Write(Header.MaxTiles.Y);
                    writer.Write(Header.MaxTiles.X);
                    writer.Write(Header.SpawnTile.X);
                    writer.Write(Header.SpawnTile.Y);
                    writer.Write(Header.WorldSurface);
                    writer.Write(Header.WorldRockLayer);
                    writer.Write(Header.Time);
                    writer.Write(Header.IsDayTime);
                    writer.Write(Header.MoonPhase);
                    writer.Write(Header.IsBloodMoon);
                    writer.Write(Header.DungeonEntrance.X);
                    writer.Write(Header.DungeonEntrance.Y);
                    writer.Write(Header.IsBossDowned1);
                    writer.Write(Header.IsBossDowned2);
                    writer.Write(Header.IsBossDowned3);
                    writer.Write(Header.IsShadowOrbSmashed);
                    writer.Write(Header.IsSpawnMeteor);
                    writer.Write((byte)Header.ShadowOrbCount);
                    writer.Write(Header.InvasionDelay);
                    writer.Write(Header.InvasionSize);
                    writer.Write(Header.InvasionType);
                    writer.Write(Header.InvasionX);

                    for (int x = 0; x < Header.MaxTiles.X; x++)
                    {
                        OnProgressChanged(this,
                                          new ProgressChangedEventArgs((int)(x / (double)Header.MaxTiles.X * 100.0),
                                                                       "Saving World"));
                        //float num2 = ((float) i) / ((float) this.MaxTiles.X);
                        //string statusText = "Saving world data: " + ((int) ((num2 * 100f) + 1f)) + "%";
                        for (int y = 0; y < Header.MaxTiles.Y; y++)
                        {
                            writer.Write(Tiles[x, y].IsActive);
                            if (Tiles[x, y].IsActive)
                            {
                                writer.Write(Tiles[x, y].Type);
                                if (WorldSettings.Tiles[Tiles[x, y].Type].IsFramed)
                                {
                                    writer.Write(Tiles[x, y].Frame.X);
                                    writer.Write(Tiles[x, y].Frame.Y);

                                    //validate chest entry exists
                                    if (Tiles[x, y].Type == 21)
                                    {
                                        if (GetChestAtTile(x, y) == null)
                                        {
                                            Chests.Add(new Chest(new PointInt32(x, y)));
                                        }
                                    }
                                    //validate sign entry exists
                                    else if (Tiles[x, y].Type == 55 || Tiles[x, y].Type == 85)
                                    {
                                        if (GetSignAtTile(x, y) == null)
                                        {
                                            Signs.Add(new Sign("", new PointInt32(x, y)));
                                        }
                                    }
                                }
                            }
                            writer.Write(Tiles[x, y].IsLighted);
                            if (Tiles[x, y].Wall > 0)
                            {
                                writer.Write(true);
                                writer.Write(Tiles[x, y].Wall);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                            if (Tiles[x, y].Liquid > 0)
                            {
                                writer.Write(true);
                                writer.Write(Tiles[x, y].Liquid);
                                writer.Write(Tiles[x, y].IsLava);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                        }
                    }
                    for (int chestIndex = 0; chestIndex < MaxChests; chestIndex++)
                    {
                        //if (Chests[chestIndex] == null)
                        if (chestIndex >= Chests.Count)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Chests[chestIndex].Location.X);
                            writer.Write(Chests[chestIndex].Location.Y);
                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                if (Chests[chestIndex].Items.Count > slot)
                                {
                                    writer.Write((byte)Chests[chestIndex].Items[slot].StackSize);
                                    if (Chests[chestIndex].Items[slot].StackSize > 0)
                                    {
                                        writer.Write(Chests[chestIndex].Items[slot].Name);
                                    }
                                }
                                else
                                {
                                    writer.Write((byte)0);
                                }
                            }
                        }
                    }
                    for (int signIndex = 0; signIndex < MaxSigns; signIndex++)
                    {
                        //if (Signs[signIndex] == null)
                        if (signIndex >= Signs.Count)
                        {
                            writer.Write(false);
                        }
                        else if (string.IsNullOrWhiteSpace(Signs[signIndex].Text))
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Signs[signIndex].Text);
                            writer.Write(Signs[signIndex].Location.X);
                            writer.Write(Signs[signIndex].Location.Y);
                        }
                    }
                    foreach (NPC npc in Npcs)
                    {
                        // removed for list, add for array
                        //if (npc == null)
                        //{
                        //    writer.Write(false);
                        //    break;
                        //}

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
                    writer.Write(Header.WorldName);
                    writer.Write(Header.WorldId);

                    writer.Close();
                }
            }
            IsUsingIo = false;
            IsSaved = true;
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
        }

        #region Test World Compression Methods
        public void SaveFileCompressed(string filename)
        {
            SaveFileCompressed1(filename + ".TEST1");
            SaveFileCompressed2(filename + ".TEST2");

        }
        
        public void SaveFileCompressed1(string filename)
        {
            IsUsingIo = false;
            string backupFileName = filename + ".Tedit";
            if (File.Exists(filename))
            {
                File.Copy(filename, backupFileName, true);
            }
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Header.FileVersion);
                    writer.Write(Header.WorldName);
                    writer.Write(Header.WorldId);
                    writer.Write((int)Header.WorldBounds.Left);
                    writer.Write((int)Header.WorldBounds.Right);
                    writer.Write((int)Header.WorldBounds.Top);
                    writer.Write((int)Header.WorldBounds.Bottom);
                    writer.Write(Header.MaxTiles.Y);
                    writer.Write(Header.MaxTiles.X);
                    writer.Write(Header.SpawnTile.X);
                    writer.Write(Header.SpawnTile.Y);
                    writer.Write(Header.WorldSurface);
                    writer.Write(Header.WorldRockLayer);
                    writer.Write(Header.Time);
                    writer.Write(Header.IsDayTime);
                    writer.Write(Header.MoonPhase);
                    writer.Write(Header.IsBloodMoon);
                    writer.Write(Header.DungeonEntrance.X);
                    writer.Write(Header.DungeonEntrance.Y);
                    writer.Write(Header.IsBossDowned1);
                    writer.Write(Header.IsBossDowned2);
                    writer.Write(Header.IsBossDowned3);
                    writer.Write(Header.IsShadowOrbSmashed);
                    writer.Write(Header.IsSpawnMeteor);
                    writer.Write((byte)Header.ShadowOrbCount);
                    writer.Write(Header.InvasionDelay);
                    writer.Write(Header.InvasionSize);
                    writer.Write(Header.InvasionType);
                    writer.Write(Header.InvasionX);

                    Tile prevTile = null;
                    int repeatCounter = 0;
                    for (int y = 0; y < Header.MaxTiles.Y; y++)
                    {
                        OnProgressChanged(this, new ProgressChangedEventArgs((int)(y / (double)Header.MaxTiles.Y * 100.0), "Saving World"));

                        for (int x = 0; x < Header.MaxTiles.X; x++)
                        {
                            var cacheTile = Tiles[x, y];

                            // == is an overridden comparison on the writeable fields
                            if (cacheTile == prevTile)
                            {
                                repeatCounter++;
                                continue;
                                // reset loop
                            }

                            if (prevTile != null)
                            {
                                // use if to prevent duplicating the first tile;
                                writer.Write(repeatCounter);
                                repeatCounter = 0;
                            }
                            // make prev tile equal to new tile type
                            prevTile = cacheTile;

                            //perform standard tile writing below
                            writer.Write(cacheTile.IsActive);

                            if (cacheTile.IsActive)
                            {
                                writer.Write(cacheTile.Type);
                                if (WorldSettings.Tiles[cacheTile.Type].IsFramed)
                                {
                                    writer.Write(cacheTile.Frame.X);
                                    writer.Write(cacheTile.Frame.Y);

                                    //validate chest entry exists
                                    if (cacheTile.Type == 21)
                                    {
                                        if (GetChestAtTile(x, y) == null)
                                        {
                                            Chests.Add(new Chest(new PointInt32(x, y)));
                                        }
                                    }
                                    //validate sign entry exists
                                    else if (cacheTile.Type == 55 || cacheTile.Type == 85)
                                    {
                                        if (GetSignAtTile(x, y) == null)
                                        {
                                            Signs.Add(new Sign("", new PointInt32(x, y)));
                                        }
                                    }
                                }
                            }
                            writer.Write(cacheTile.IsLighted);
                            if (cacheTile.Wall > 0)
                            {
                                writer.Write(true);
                                writer.Write(cacheTile.Wall);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                            if (cacheTile.Liquid > 0)
                            {
                                writer.Write(true);
                                writer.Write(cacheTile.Liquid);
                                writer.Write(cacheTile.IsLava);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                        }
                    }
                    for (int chestIndex = 0; chestIndex < MaxChests; chestIndex++)
                    {
                        //if (Chests[chestIndex] == null)
                        if (chestIndex >= Chests.Count)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Chests[chestIndex].Location.X);
                            writer.Write(Chests[chestIndex].Location.Y);
                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                if (Chests[chestIndex].Items.Count > slot)
                                {
                                    writer.Write((byte)Chests[chestIndex].Items[slot].StackSize);
                                    if (Chests[chestIndex].Items[slot].StackSize > 0)
                                    {
                                        writer.Write(Chests[chestIndex].Items[slot].Name);
                                    }
                                }
                                else
                                {
                                    writer.Write((byte)0);
                                }
                            }
                        }
                    }
                    for (int signIndex = 0; signIndex < MaxSigns; signIndex++)
                    {
                        //if (Signs[signIndex] == null)
                        if (signIndex >= Signs.Count)
                        {
                            writer.Write(false);
                        }
                        else if (string.IsNullOrWhiteSpace(Signs[signIndex].Text))
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Signs[signIndex].Text);
                            writer.Write(Signs[signIndex].Location.X);
                            writer.Write(Signs[signIndex].Location.Y);
                        }
                    }
                    foreach (NPC npc in Npcs)
                    {
                        // removed for list, add for array
                        //if (npc == null)
                        //{
                        //    writer.Write(false);
                        //    break;
                        //}

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
                    writer.Write(Header.WorldName);
                    writer.Write(Header.WorldId);

                    writer.Close();
                }
            }
            IsUsingIo = true;
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
        }

        public void SaveFileCompressed2(string filename)
        {
            IsUsingIo = false;
            //string backupFileName = filename + ".Tedit1";
            //if (File.Exists(filename))
            //{
            //    File.Copy(filename, backupFileName, true);
            //} 
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Header.FileVersion);
                    writer.Write(Header.WorldName);
                    writer.Write(Header.WorldId);
                    writer.Write((int)Header.WorldBounds.Left);
                    writer.Write((int)Header.WorldBounds.Right);
                    writer.Write((int)Header.WorldBounds.Top);
                    writer.Write((int)Header.WorldBounds.Bottom);
                    writer.Write(Header.MaxTiles.Y);
                    writer.Write(Header.MaxTiles.X);
                    writer.Write(Header.SpawnTile.X);
                    writer.Write(Header.SpawnTile.Y);
                    writer.Write(Header.WorldSurface);
                    writer.Write(Header.WorldRockLayer);
                    writer.Write(Header.Time);
                    writer.Write(Header.IsDayTime);
                    writer.Write(Header.MoonPhase);
                    writer.Write(Header.IsBloodMoon);
                    writer.Write(Header.DungeonEntrance.X);
                    writer.Write(Header.DungeonEntrance.Y);
                    writer.Write(Header.IsBossDowned1);
                    writer.Write(Header.IsBossDowned2);
                    writer.Write(Header.IsBossDowned3);
                    writer.Write(Header.IsShadowOrbSmashed);
                    writer.Write(Header.IsSpawnMeteor);
                    writer.Write((byte)Header.ShadowOrbCount);
                    writer.Write(Header.InvasionDelay);
                    writer.Write(Header.InvasionSize);
                    writer.Write(Header.InvasionType);
                    writer.Write(Header.InvasionX);

                    var index = new Dictionary<Tile, int>();
                    var buffer = new List<Tuple<int, int>>();

                    Tile previousTile = null;
                    int repeatCount = 0;
                    int cacheTileIndex = 0;

                    for (int y = 0; y < Header.MaxTiles.Y; y++)
                    {
                        for (int x = 0; x < Header.MaxTiles.X; x++)
                        {
                            var cacheTile = Tiles[x, y];

                            if (cacheTile.Equals(previousTile))
                            {
                                repeatCount++;
                                continue;
                            }
                            else
                            {
                                // Add to buffer and reset counter
                                buffer.Add(new Tuple<int, int>(cacheTileIndex, repeatCount));
                                repeatCount = 0;
                                previousTile = cacheTile;
                            }

                            //if (TileProperties.TileFrameImportant[cacheTile.Type])
                            //{
                            //////validate chest entry exists
                            ////if (cacheTile.Type == 21)
                            ////{
                            ////    if (GetChestAtTile(x, y) == null)
                            ////    {
                            ////        Chests.Add(new Chest(new PointInt32(x, y)));
                            ////    }
                            ////}
                            //////validate sign entry exists
                            ////else if (cacheTile.Type == 55 || cacheTile.Type == 85)
                            ////{
                            ////    if (GetSignAtTile(x, y) == null)
                            ////    {
                            ////        Signs.Add(new Sign("", new PointInt32(x, y)));
                            ////    }
                            ////}
                            //}

                            if (!index.ContainsKey(cacheTile))
                            {
                                // Add our tile to the index
                                cacheTileIndex = index.Count;
                                index.Add(cacheTile, cacheTileIndex);
                            }
                            else
                            {
                                // Get the index
                                index.TryGetValue(cacheTile, out cacheTileIndex);
                            }
                        }
                    }

                    writer.Write(index.Count);
                    foreach (KeyValuePair<Tile, int> tile in index)
                    {

                        Tile cacheTile = tile.Key;
                        writer.Write(cacheTile.IsActive);

                        if (cacheTile.IsActive)
                        {
                            writer.Write(cacheTile.Type);
                            if (WorldSettings.Tiles[cacheTile.Type].IsFramed)
                            {
                                writer.Write(cacheTile.Frame.X);
                                writer.Write(cacheTile.Frame.Y);
                            }
                        }
                        writer.Write(cacheTile.IsLighted);
                        if (cacheTile.Wall > 0)
                        {
                            writer.Write(true);
                            writer.Write(cacheTile.Wall);
                        }
                        else
                        {
                            writer.Write(false);
                        }
                        if (cacheTile.Liquid > 0)
                        {
                            writer.Write(true);
                            writer.Write(cacheTile.Liquid);
                            writer.Write(cacheTile.IsLava);
                        }
                        else
                        {
                            writer.Write(false);
                        }
                    }

                    writer.Write(buffer.Count);
                    foreach (Tuple<int, int> tileCompressed in buffer)
                    {
                        writer.Write(tileCompressed.Item1); // dictionary index
                        writer.Write(tileCompressed.Item2); // tile repeat count
                    }


                    for (int chestIndex = 0; chestIndex < MaxChests; chestIndex++)
                    {
                        //if (Chests[chestIndex] == null)
                        if (chestIndex >= Chests.Count)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Chests[chestIndex].Location.X);
                            writer.Write(Chests[chestIndex].Location.Y);
                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                if (Chests[chestIndex].Items.Count > slot)
                                {
                                    writer.Write((byte)Chests[chestIndex].Items[slot].StackSize);
                                    if (Chests[chestIndex].Items[slot].StackSize > 0)
                                    {
                                        writer.Write(Chests[chestIndex].Items[slot].Name);
                                    }
                                }
                                else
                                {
                                    writer.Write((byte)0);
                                }
                            }
                        }
                    }
                    for (int signIndex = 0; signIndex < MaxSigns; signIndex++)
                    {
                        //if (Signs[signIndex] == null)
                        if (signIndex >= Signs.Count)
                        {
                            writer.Write(false);
                        }
                        else if (string.IsNullOrWhiteSpace(Signs[signIndex].Text))
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Signs[signIndex].Text);
                            writer.Write(Signs[signIndex].Location.X);
                            writer.Write(Signs[signIndex].Location.Y);
                        }
                    }
                    foreach (NPC npc in Npcs)
                    {
                        // removed for list, add for array
                        //if (npc == null)
                        //{
                        //    writer.Write(false);
                        //    break;
                        //}

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
                    writer.Write(Header.WorldName);
                    writer.Write(Header.WorldId);

                    writer.Close();
                }
            }
            IsUsingIo = true;
            OnProgressChanged(this, new ProgressChangedEventArgs(0, ""));
        }
        #endregion
    }
}