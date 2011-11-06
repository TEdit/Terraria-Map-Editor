using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TEdit.Common.Structures;
using TEdit.RenderWorld;
using TEdit.Common;

namespace TEdit.TerrariaWorld
{
    using FP = FramePlacement;
    using System.Collections.ObjectModel;

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
            set { SetProperty(ref _isUsingIo, ref value, "IsUsingIo"); }
        }

        public event ProgressChangedEventHandler ProgressChanged;

        // TODO: This should probably be in its own module, as this is an exact copy of the one in WorldRenderer.cs
        private string lastProgressMsg = String.Empty;
        protected virtual void OnProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }
        protected virtual void OnProgressChanged(object sender, int p = 0, int pTtl = 100, string msg = null) {
            if (msg == null) msg = lastProgressMsg;
            else lastProgressMsg = msg;

            if (ProgressChanged != null)
                ProgressChanged(sender, new ProgressChangedEventArgs(
                    (int)(p / (double)pTtl * 100.0),
                    msg)
                );
        }
        protected virtual void OnProgressChanged(object sender, string msg) { OnProgressChanged(sender, 0, 100, msg); }

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

            Header.WorldBounds = new RectI(0, width - 1, 0, height - 1);
            var wb = Header.WorldBounds;

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

            for (int x = wb.Left; x <= wb.Right; x++) {
                OnProgressChanged(this, x, wb.Right, "Loading Tiles...");

                for (int y = wb.Top; y <= wb.Bottom; y++) {
                    Tiles[x, y] = new Tile();
                }
            }
            IsValid = true;
            IsUsingIo = false;
        }

        private bool ValidatePlacement(PointInt32 location, PointShort size, FramePlacement frame)  // using FP = FramePlacement;
        {
            // TODO: Support for attachesTo with placement

            // quick short-circuits
            if (frame.Is(FP.None))   return false;   // hey, if it can't attach to anything, it's invalid
            if (frame.Is(FP.Any))    return true;
            if (frame.Has(FP.Float)) return true;  // ...for now; this behavior may change if we actually get a "float only" object

            /// Look at surrounding area for objects ///
            FramePlacement area   = FP.Float;
            RectI areaRect = new RectI(location - 1, location + size);
            // (only used for x/y loop)
            RectI areaRectBnd = new RectI(areaRect);
            
            // skip tests that don't apply
            // (frame = multiples)
            if (frame.HasNo(FP.Ceiling))      areaRectBnd.Top++;
            if (frame.HasNo(FP.FloorSurface)) areaRectBnd.Bottom--;
            if (frame.HasNo(FP.Wall)) {
                areaRectBnd.Left++;
                areaRectBnd.Right--;
            }

            // (shorter cuts for singular terms; using HasNoneOf to factor in MustHaveAll)
            if (frame.HasNoneOf(FP.WallCeiling))      areaRectBnd.Top    = areaRect.Bottom;  // Floor/Surface/both
            if (frame.HasNoneOf(FP.WallFloorSurface)) areaRectBnd.Bottom = areaRect.Top;     // Ceiling
                                                                                             // Wall already covered in multiples checks
            
            // boundary checks
            if ( areaRectBnd.Left  < 0)                  areaRectBnd.Left   = 0;
            if ( areaRectBnd.Top   < 0)                  areaRectBnd.Top    = 0;
            if (areaRectBnd.Right  > Header.WorldBounds.Right)  areaRectBnd.Right  = Header.WorldBounds.Right;
            if (areaRectBnd.Bottom > Header.WorldBounds.Bottom) areaRectBnd.Bottom = Header.WorldBounds.Bottom;

            for (int y = areaRectBnd.Top; y <= areaRectBnd.Bottom; y++)
            {
                for (int x = areaRectBnd.Left; x <= areaRectBnd.Right; x++)
                {
                    // skip dead zone (the item itself) & corners (wow, xor use...)
                    bool valid = (x == areaRect.Left || x == areaRect.Right) ^ (y == areaRect.Top || y == areaRect.Bottom);
                    if (!valid) continue;

                    var t = Tiles[x, y];
                    var w = WorldSettings.Tiles[t.Type];
                    
                    // skip non-solid objects
                    if (! (t.IsActive && w.IsSolid || w.IsSolidTop)) continue;
                    
                    // FIXME: Assuming that a single tile will hold the object //
                    // (Maybe this is true in Terraria as well....)

                    // at this point, only one of these will hit
                    if      (y ==  areaRect.Top) {
                        area = area.Add(FP.Ceiling);
                        break;  // done with this Y-axis
                    }
                    else if (x ==  areaRect.Left || x == areaRect.Right) {
                        area = area.Add(FP.Wall);
                        y = areaRect.Bottom - 1;  // -1 = for loop will re-adjust, or kick out if LRB was truncated
                        break;  // done with all except bottom
                    }
                    else if (y == areaRect.Bottom) {  // special case for floor/surface
                        if (w.IsSolidTop) area = area.Add(FP.Surface);
                        else              area = area.Add(FP.Floor);
                        if (area.HasAllOf(FP.FloorSurface)) break;  // done with everything else
                    }
                }
            }
            
            // Now let's compare the object in question
            if (frame.Has(FP.MustHaveAll)) {
                area = area.Add(FP.MustHaveAll);  // add bit for bitwise math to work
                if (area.Filter(frame) != frame) return false;
            }
            else {
                if (frame.HasNoneOf(area))       return false;
            }

            return true;
        }

        public void Validate()
        {
            List<string> log = new List<string>();
            IsUsingIo = true;
            
            var wb = Header.WorldBounds;
            short[,] deadSpace = new short[wb.Width, wb.Height];
            for (int y = wb.Top; y <= wb.Bottom; y++) {
                OnProgressChanged(this, y, wb.Bottom, "Validating Tiles...");
                
                for (int x = wb.Left; x <= wb.Right; x++) {
                    // skip anything in the dead space
                    if (deadSpace[x,y] > 0)
                    {
                        x += deadSpace[x,y] - 1;
                        continue;
                    }
                    
                    // TODO: Need Frames support //
                    // (All tiles have the size/placement properties, but this may change in the future...) //
                    var tile  = Tiles[x, y];
                    if (!tile.IsActive) continue;  // immediate short-circuit
                    var type  = tile.Type;
                    var prop  = WorldSettings.Tiles[type];
                    var place = prop.Placement;
                    if (prop.AttachesTo.Count > 0) continue;  // can't really handle these yet...

                    if (!ValidatePlacement(new PointInt32(x,y), prop.Size, place))  // validation found a problem
                    {
                        log.Add(string.Format("Tile [{2}] at [{0},{1}] must be placed on {3} {4}", x, y, prop.Name,
                            place.Has(FP.MustHaveAll) ? "all of:" : (place.IsSingular() ? "a" : "any of:"),
                            place.Remove(FP.MustHaveAll)));
                    }

                    // validate chest/sign/NPC entries exist
                    switch (type)
                    {
                        case 21:
                            // Validate Chest
                            if (GetChestAtTile(x, y) == null)
                            {
                                var c = new Chest(new PointInt32(x, y));
                                for (int i = 0; i < 20; i++)
                                    c.Items.Add(new Item(0, "[empty]"));
                                
                                Chests.Add(c);
                                  
                                log.Add(string.Format("added empty chest content [{0},{1}]", x, y));
                            }
                            break;
                        case 55:
                        case 85:
                            // Validate Sign/Tombstone
                            if (GetSignAtTile(x, y) == null)
                            {
                                Signs.Add(new Sign("", new PointInt32(x, y)));
                                log.Add(string.Format("added blank sign text [{0},{1}]", x, y));
                            }
                    
                    break;
                    }

                    // TODO: validate the frame exists completely //

                    // assuming the left-right scan, it should hit the top-left corner first
                    // thus, we skip around the rest of the frame for the x-axis

                    // y-axis is a little bit more difficult... (and it requires that x stay put for a bit)
                    if (prop.Size.Y > 1) {
                        for (int s = 1; s < prop.Size.Y; s++) { deadSpace[x,y+s] = prop.Size.X; }
                    }
                    if (prop.Size.X > 1) x += prop.Size.X - 1;
                }
            }

            var p = 0;
            OnProgressChanged(this, "Validating Chests...");
            foreach (var chest in Chests.ToList())
            {
                var loc = chest.Location;
                int locType = Tiles[loc.X, loc.Y].Type;

                if (locType != 21)
                {
                    Chests.Remove(chest);
                    log.Add(string.Format("removed missing chest {0}", loc));
                }
                OnProgressChanged(this, ++p, Chests.Count);
            }

            p = 0;
            OnProgressChanged(this, "Validating Signs...");
            foreach (var sign in Signs.ToList())
            {
                var loc = sign.Location;
                int locType = Tiles[loc.X, loc.Y].Type;

                if (locType != 55 && locType != 85)
                {
                    Signs.Remove(sign);
                    log.Add(string.Format("removed missing sign {0}", loc));
                }
                OnProgressChanged(this, ++p, Signs.Count);
            }

            foreach (NPC npc in Npcs)
            {
                // no validation yet...
                // (SS: Okay, this is now in the XML; just need to port that stuff over)
            }
            IsUsingIo = false;
            OnProgressChanged(this, 100, 100, "Validation Complete.");

            log.Add("FINISHED with Validation!");
            ErrorLogging.Log(string.Join(Environment.NewLine, log.ToArray()));
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
                    
                    // WorldBounds (within the file) is measured in frame units (1/2 of a pixel), while MaxTiles is measured in tiles
                    // We force everything into whole tiles, and ditch the redundant MaxTiles
                    // (Also, WorldBounds actually uses W/H, so it is really -1 for right/bottom, in an inclusive-based XY system.)
                    Header.WorldBounds = new RectI(reader.ReadInt32(), reader.ReadInt32() - 1, reader.ReadInt32(), reader.ReadInt32() - 1)
                                       / new RectI(16, 16, new SizeInt32(16, 16));
                    reader.ReadInt32();  // max Y
                    reader.ReadInt32();  // max X
                    var wb = Header.WorldBounds;

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

                    for (int x = wb.Left; x <= wb.Right; x++) {
                        OnProgressChanged(this, x, wb.Right, "Loading Tiles...");

                        for (int y = wb.Top; y <= wb.Bottom; y++) {
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
                        OnProgressChanged(this, chestIndex, MaxChests, "Loading Chest Data...");

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
                                    item.ItemName = itemName;
                                    item.StackSize = stackSize;
                                }
                                chest.Items.Add(item);
                            }

                            Chests.Add(chest);
                        }
                    }
                    for (int signIndex = 0; signIndex < MaxSigns; signIndex++)
                    {
                        OnProgressChanged(this, signIndex, MaxSigns, "Loading Sign Data...");

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

                                Signs.Add(sign);
                            }
                        }
                    }

                    bool isNpcActive = reader.ReadBoolean();
                    for (int npcIndex = 0; isNpcActive; npcIndex++)
                    {
                        OnProgressChanged(this, npcIndex, MaxNpcs, "Loading NPCs...");
                        var npc = new NPC();

                        npc.Name = reader.ReadString();
                        npc.Position = new PointFloat(reader.ReadSingle(), reader.ReadSingle());
                        npc.IsHomeless = reader.ReadBoolean();
                        npc.HomeTile = new PointInt32(reader.ReadInt32(), reader.ReadInt32());

                        Npcs.Add(npc);
                        isNpcActive = reader.ReadBoolean();
                    }

                    if (Header.FileVersion > 7)
                    {
                        OnProgressChanged(this, 100, 100, "Checking format...");
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
            OnProgressChanged(this, 100, 100, "Loading Complete.");
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
                    writer.Write(Header.WorldBounds.Left * 16);
                    writer.Write(Header.WorldBounds.Width * 16);
                    writer.Write(Header.WorldBounds.Top * 16);
                    writer.Write(Header.WorldBounds.Height * 16);
                    writer.Write(Header.WorldBounds.Height);
                    writer.Write(Header.WorldBounds.Width);
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

                    var wb = Header.WorldBounds;
                    for (int x = wb.Left; x <= wb.Right; x++) {
                        OnProgressChanged(this, x, wb.Right, "Saving Tiles...");

                        for (int y = wb.Top; y <= wb.Bottom; y++) {
                            writer.Write(Tiles[x, y].IsActive);
                            if (Tiles[x, y].IsActive)
                            {
                                writer.Write(Tiles[x, y].Type);
                                if (WorldSettings.Tiles[Tiles[x, y].Type].IsFramed)
                                {
                                    writer.Write(Tiles[x, y].Frame.X);
                                    writer.Write(Tiles[x, y].Frame.Y);

                                    // TODO: Let Validate handle these
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
                        OnProgressChanged(this, chestIndex, MaxChests, "Saving Chest Data...");
                        
                        if (chestIndex >= Chests.Count) writer.Write(false);
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
                                        writer.Write(Chests[chestIndex].Items[slot].ItemName);
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
                        OnProgressChanged(this, signIndex, MaxSigns, "Saving Sign Data...");

                        if (signIndex >= Signs.Count) writer.Write(false);
                        else if (string.IsNullOrWhiteSpace(Signs[signIndex].Text)) writer.Write(false);
                        else
                        {
                            writer.Write(true);
                            writer.Write(Signs[signIndex].Text);
                            writer.Write(Signs[signIndex].Location.X);
                            writer.Write(Signs[signIndex].Location.Y);
                        }
                    }
                    var p = 0;
                    foreach (NPC npc in Npcs)
                    {
                        OnProgressChanged(this, ++p, Npcs.Count, "Saving NPCs...");

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
            OnProgressChanged(this, 100, 100, "Saving Complete.");
        }
    }
}