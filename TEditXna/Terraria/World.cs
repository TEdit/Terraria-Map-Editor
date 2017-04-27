using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using TEditXNA.Terraria.Objects;

namespace TEditXNA.Terraria
{
    public partial class World : ObservableObject
    {
        private static readonly object _fileLock = new object();
        /// <summary>
        ///     Triggered when an operation reports progress.
        /// </summary>
        public static event ProgressChangedEventHandler ProgressChanged;

        private static void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(sender, e);
        }

        public World()
        {
            NPCs.Clear();
            Signs.Clear();
            Chests.Clear();
            CharacterNames.Clear();
        }

        public World(int height, int width, string title, int seed = -1)
            : this()
        {
            TilesWide = width;
            TilesHigh = height;
            Title = title;
            Random r = seed <= 0 ? new Random((int)DateTime.Now.Ticks) : new Random(seed);
            WorldId = r.Next(int.MaxValue);
            Guid = Guid.NewGuid();
            Seed = "";
            _npcs.Clear();
            _signs.Clear();
            _chests.Clear();
            _charNames.Clear();
        }

        public static void Save(World world, string filename, bool resetTime = false)
        {
            try
            {
                OnProgressChanged(world, new ProgressChangedEventArgs(0, "Validating World..."));
                world.Validate();
            }
            catch (ArgumentOutOfRangeException err)
            {
                string msg = "There is a problem in your world.\r\n" +
                             $"{err.ParamName}\r\nThis world will not open in Terraria\r\n" +
                             "Would you like to save anyways??\r\n";
                if (MessageBox.Show(msg, "World Error", MessageBoxButton.YesNo, MessageBoxImage.Error) !=
                    MessageBoxResult.Yes)
                    return;
            }
            lock (_fileLock)
            {


                if (resetTime)
                {
                    OnProgressChanged(world, new ProgressChangedEventArgs(0, "Resetting Time..."));
                    world.ResetTime();
                }

                if (filename == null)
                    return;

                string temp = filename + ".tmp";
                using (var fs = new FileStream(temp, FileMode.Create))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        if (world.Version > 87)
                            SaveV2(world, bw);
                        else
                            SaveV1(world, bw);

                        bw.Close();
                        fs.Close();

                        // make a backup of current file if it exists
                        if (File.Exists(filename))
                        {
                            string backup = filename + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".TEdit";
                            File.Copy(filename, backup, true);
                        }
                        // replace actual file with temp save file
                        File.Copy(temp, filename, true);
                        // delete temp save file
                        File.Delete(temp);
                        OnProgressChanged(null, new ProgressChangedEventArgs(0, "World Save Complete."));
                    }
                }

                world._lastSave = File.GetLastWriteTimeUtc(filename);
            }
        }

        public static World LoadWorld(string filename)
        {
            var w = new World();
            uint curVersion = 0;
            try
            {
                lock (_fileLock)
                {
                    using (var b = new BinaryReader(File.OpenRead(filename)))
                    {
                        w.Version = b.ReadUInt32();
                        curVersion = w.Version;
                        if (w.Version > 87)
                            LoadV2(b, filename, w);
                        else
                            LoadV1(b, filename, w);
                    }
                    w.LastSave = File.GetLastWriteTimeUtc(filename);
                }
            }
            catch (Exception err)
            {

                string msg =
                    "There was an error reading the world file. This is usually caused by a corrupt save file or a world version newer than supported.\r\n\r\n" +
                    $"TEdit v{TEditXna.App.Version.FileVersion}\r\n" +
                    $"TEdit Max World: {CompatibleVersion}    Current World: {curVersion}\r\n\r\n" +
                    "Do you wish to force it to load anyway?\r\n\r\n" +
                    "WARNING: This may have unexpected results including corrupt world files and program crashes.\r\n\r\n" +
                    $"The error is :\r\n{err.Message}\r\n\r\n{err}\r\n";
                if (MessageBox.Show(msg, "World File Error", MessageBoxButton.YesNo, MessageBoxImage.Error) !=
                    MessageBoxResult.Yes)
                    return null;
            }
            return w;
        }

        public void ResetTime()
        {
            DayTime = true;
            Time = 13500.0;
            MoonPhase = 0;
            BloodMoon = false;
        }

        public bool ValidTileLocation(Vector2Int32 v)
        {
            return ValidTileLocation(v.X, v.Y);
        }


        public bool ValidTileLocation(int x, int y)
        {
            return (x >= 0 && y >= 0 && y < _tilesHigh && x < _tilesWide);
        }


        public Chest GetChestAtTile(int x, int y)
        {
            Vector2Int32 anchor = GetAnchor(x,y);
            return Chests.FirstOrDefault(c => (c.X == anchor.X) && (c.Y == anchor.Y));
        }

        public Sign GetSignAtTile(int x, int y)
        {
            Vector2Int32 anchor = GetAnchor(x,y);
            return Signs.FirstOrDefault(c => (c.X == anchor.X) && (c.Y == anchor.Y));
        }

        public TileEntity GetTileEntityAtTile(int x, int y)
        {
            Vector2Int32 anchor = GetAnchor(x,y);
            return TileEntities.FirstOrDefault(c => (c.PosX == anchor.X) && (c.PosY == anchor.Y));
        }

        public Vector2Int32 GetMannequin(int x, int y)
        {
            Tile tile = Tiles[x, y];
            x -= (tile.U % 100) % 36 / 18;
            y -= tile.V / 18;
            return new Vector2Int32(x, y);
        }

        public Vector2Int32 GetRack(int x, int y)
        {
            Tile tile = Tiles[x, y];
            if (tile.U >= 5000)
            {
                x -= ((tile.U / 5000) - 1) % 3;
            }
            else
            {
                x -= tile.U % 54 / 18;
            }
            y -= tile.V / 18;
            return new Vector2Int32(x, y);
        }

        public Vector2Int32 GetXmas(int x, int y)
        {
            Tile tile = Tiles[x, y];
            if (tile.U < 10)
            {
                x -= tile.U;
                y -= tile.V;
            }
            return new Vector2Int32(x, y);
        }

        // find upper left corner of sprites
        public Vector2Int32 GetAnchor(int x, int y)
        {
            Tile tile = Tiles[x, y];
            TileProperty tileprop = TileProperties[tile.Type];
            if (tileprop.IsFramed && (tileprop.FrameSize.X > 1 || tileprop.FrameSize.Y > 1))
            {
                int xShift = tile.U % ((tileprop.TextureGrid.X + 2) * tileprop.FrameSize.X) / (tileprop.TextureGrid.X + 2);
                int yShift = tile.V % ((tileprop.TextureGrid.Y + 2) * tileprop.FrameSize.Y) / (tileprop.TextureGrid.Y + 2);
                return new Vector2Int32(x - xShift, y - yShift);
            }
            else
                return new Vector2Int32(x, y);
        }

        public void Validate()
        {
            for (int x = 0; x < TilesWide; x++)
            {
                OnProgressChanged(this,
                    new ProgressChangedEventArgs((int)(x / (float)TilesWide * 100.0), "Validating World..."));

                for (int y = 0; y < TilesHigh; y++)
                {
                    Tile curTile = Tiles[x, y];

                    if (curTile.Type == (int)TileType.IceByRod)
                        curTile.IsActive = false;

                    ValSpecial(x, y);

                }
            }

            foreach (Chest chest in Chests.ToArray())
            {
                bool removed = false;
                for (int x = chest.X; x < chest.X + 1; x++)
                {
                    for (int y = chest.Y; y < chest.Y + 1; y++)
                    {
                        if (!Tiles[x, y].IsActive || !Tile.IsChest(Tiles[x, y].Type))
                        {
                            Chests.Remove(chest);
                            removed = true;
                            break;
                        }
                    }
                    if (removed) break;
                }
            }

            foreach (Sign sign in Signs.ToArray())
            {
                if (sign.Text == null)
                {
                    Signs.Remove(sign);
                    continue;
                }

                bool removed = false;
                for (int x = sign.X; x < sign.X + 1; x++)
                {
                    for (int y = sign.Y; y < sign.Y + 1; y++)
                    {
                        if (!Tiles[x, y].IsActive || !Tile.IsSign(Tiles[x, y].Type))
                        {
                            Signs.Remove(sign);
                            removed = true;
                            break;
                        }
                    }
                    if (removed) break;
                }
            }

            foreach (TileEntity tileEntity in TileEntities.ToArray())
            {
                int x = tileEntity.PosX;
                int y = tileEntity.PosY;
                if (!Tiles[x, y].IsActive || !Tile.IsTileEntity(Tiles[x, y].Type))
                    TileEntities.Remove(tileEntity);
            }

            OnProgressChanged(this,
                    new ProgressChangedEventArgs(0, "Validating Complete..."));
            if (Chests.Count > 1000)
                throw new ArgumentOutOfRangeException($"Chest Count is {Chests.Count} which is greater than 1000");
            if (Signs.Count > 1000)
                throw new ArgumentOutOfRangeException($"Sign Count is {Signs.Count} which is greater than 1000");
        }
        public void ValSpecial(int x, int y)
        {
            Tile curTile = Tiles[x, y];
            //validate chest entry exists
            if (Tile.IsChest(curTile.Type))
            {
                if (GetChestAtTile(x, y) == null)
                {
                    Chests.Add(new Chest(x, y));
                }
            }
            //validate sign entry exists
            else if (Tile.IsSign(curTile.Type))
            {
                if (GetSignAtTile(x, y) == null)
                {
                    Signs.Add(new Sign(x, y, string.Empty));
                }
            }
            //validate TileEntity
            else if (Tile.IsTileEntity(curTile.Type))
            {
                if (GetTileEntityAtTile(x, y) == null)
                {
                    TileEntity TE = new TileEntity();
                    TE.PosX = (short)x;
                    TE.PosY = (short)y;
                    TE.Id = TileEntities.Count;
                    if (curTile.Type == (int)TileType.Dummy)
                    {
                        TE.Type = 0;
                        TE.Npc = -1;
                    }
                    else if (curTile.Type == (int)TileType.ItemFrame)
                    {
                        TE.Type = 1;
                        TE.NetId = 0;
                        TE.Prefix = 0;
                        TE.StackSize = 0;
                    }
                    else
                    {
                        TE.Type = 2;
                        TE.On = false;
                        TE.LogicCheck = (byte)(curTile.V / 18 + 1);
                    }
                    TileEntities.Add(TE);
                }
            }
        }
        private void FixChand()
        {
            for (int x = 5; x < TilesWide - 5; x++)
            {
                for (int y = 5; y < TilesHigh - 5; y++)
                {
                    if (Tiles[x, y].IsActive)
                    {
                        int tileType = Tiles[x, y].Type;
                        if (Tiles[x, y].IsActive &&
                            (tileType == 35 || tileType == 36 || tileType == 170 || tileType == 171 || tileType == 172))
                        {
                            FixChand(x, y);
                        }
                    }
                }
            }
        }

        public void FixChand(int x, int y)
        {
            int newPosition = 0;
            int type = Tiles[x, y].Type;
            if (Tiles[x, y].IsActive)
            {
                if (type == 35)
                {
                    newPosition = 1;
                }
                if (type == 36)
                {
                    newPosition = 2;
                }
                if (type == 170)
                {
                    newPosition = 3;
                }
                if (type == 171)
                {
                    newPosition = 4;
                }
                if (type == 172)
                {
                    newPosition = 5;
                }
            }
            if (newPosition > 0)
            {
                int xShift = x;
                int yShift = y;
                xShift = Tiles[x, y].U / 18;
                while (xShift >= 3)
                {
                    xShift = xShift - 3;
                }
                if (xShift >= 3)
                {
                    xShift = xShift - 3;
                }
                xShift = x - xShift;
                yShift = yShift + Tiles[x, y].V / 18 * -1;
                for (int x1 = xShift; x1 < xShift + 3; x1++)
                {
                    for (int y1 = yShift; y1 < yShift + 3; y1++)
                    {
                        if (Tiles[x1, y1] == null)
                        {
                            Tiles[x1, y1] = new Tile();
                        }
                        if (Tiles[x1, y1].IsActive && Tiles[x1, y1].Type == type)
                        {
                            Tiles[x1, y1].Type = (int)TileType.Chandelier;
                            Tiles[x1, y1].V = (short)(Tiles[x1, y1].V + newPosition * 54);
                        }
                    }
                }
            }
        }

        public void FixNpcs()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    int[] npcids = { 17, 18, 19, 20, 22, 54, 38, 107, 108, 124, 160, 178, 207, 208, 209, 227, 228, 229, 353, 369, 441 };

                    foreach (int npcid in npcids)
                    {
                        if (CharacterNames.All(c => c.Id != npcid))
                            CharacterNames.Add(GetNewNpc(npcid));
                    }
                });
        }

        private void FixSunflowers()
        {
            for (int i = 5; i < TilesWide - 5; ++i)
            {
                for (int j = 5; (double)j < GroundLevel; ++j)
                {
                    if (Tiles[i, j].IsActive && Tiles[i, j].Type == (int)TileType.Sunflower)
                    {
                        int u = Tiles[i, j].U / 18;
                        int v = j + Tiles[i, j].V / 18 * -1;
                        while (u > 1)
                            u -= 2;
                        int xStart = u * -1 + i;
                        int uStart = Rand.Next(3) * 36;
                        int uShift = 0;
                        for (int xx = xStart; xx < xStart + 2; ++xx)
                        {
                            for (int yy = v; yy < v + 4; ++yy)
                                Tiles[xx, yy].U = (short)(uShift + uStart);
                            uShift += 18;
                        }
                    }
                }
            }
        }
    }
}
