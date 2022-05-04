﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.Terraria.Objects;
using TEdit.MvvmLight.Threading;
using TEdit.Editor;

namespace TEdit.Terraria
{
    public partial class World : ObservableObject, ITileData
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
            TileFrameImportant = SettingsTileFrameImportant.ToArray(); // clone for "new" world. Loaded worlds will replace this with file data
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

        public static void Save(World world, string filename, bool resetTime = false, uint versionOverride = 0)
        {
            ErrorLogging.TelemetryClient?.TrackEvent(nameof(Save));

            try
            {
                OnProgressChanged(world, new ProgressChangedEventArgs(0, "Validating World..."));
                world.Validate();
            }
            catch (ArgumentOutOfRangeException err)
            {
                string msg = "There is a problem in your world.\r\n" +
                             $"{err.ParamName}\r\n" +
                             $"This world may not open in Terraria\r\n" +
                             "Would you like to save anyways??\r\n";
                if (MessageBox.Show(msg, "World Error", MessageBoxButton.YesNo, MessageBoxImage.Error) !=
                    MessageBoxResult.Yes)
                    return;
            }
            catch (Exception ex)
            {
                string msg = "There is a problem in your world.\r\n" +
                             $"{ex.Message}\r\n" +
                             "This world may not open in Terraria\r\n" +
                             "Would you like to save anyways??\r\n";

                if (MessageBox.Show(msg, "World Error", MessageBoxButton.YesNo, MessageBoxImage.Error) !=
                    MessageBoxResult.Yes)
                    return;
            }

            lock (_fileLock)
            {
                uint currentWorldVersion = world.Version;
                try
                {
                    // set the world version for this save
                    if (versionOverride > 0) { world.Version = versionOverride; }

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
                finally
                {
                    // restore the version
                    if (versionOverride > 0) { world.Version = currentWorldVersion; }
                }
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
                        string twldPath = Path.Combine(
                            Path.GetDirectoryName(filename),
                            Path.GetFileNameWithoutExtension(filename) +
                            ".twld");

                        w.IsTModLoader = File.Exists(twldPath);

                        w.Version = b.ReadUInt32();

                        if (w.Version < World.CompatibleVersion && w.IsTModLoader)
                        {
                            string message = $"You are loading a legacy TModLoader world version: {w.Version}.\r\n" +
                                $"1. Editing legacy files is a BETA feature.\r\n" +
                                $"2. Editing modded worlds is unsupported.\r\n" +
                                "Please make a backup as you may experience world file corruption.\r\n" +
                                "Do you wish to continue?";
                            if (MessageBox.Show(message, "Convert File?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                return null;
                            }
                        }
                        else if (w.Version < World.CompatibleVersion)
                        {
                            string message = $"You are loading a legacy world version: {w.Version}.\r\n" +
                                $"Editing legacy files is a BETA feature.\r\n" +
                                "Please make a backup as you may experience world file corruption.\r\n" +
                                "Do you wish to continue?";
                            if (MessageBox.Show(message, "Convert File?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                return null;
                            }
                        }
                        else if (w.IsTModLoader)
                        {
                            string message = $"You are loading a TModLoader world." +
                                $"Editing modded worlds is unsupported.\r\n" +
                                "Please make a backup as you may experience world file corruption.\r\n" +
                                "Do you wish to continue?";
                            if (MessageBox.Show(message, "Load Mod World?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                return null;
                            }
                        }

                        curVersion = w.Version;
                        if (w.Version > 87)
                        {
                            LoadV2(b, w);
                        }
                        else
                            LoadV1(b, filename, w);
                        w.UpgradeLegacyTileEntities();
                    }
                    w.LastSave = File.GetLastWriteTimeUtc(filename);
                }
            }
            catch (Exception err)
            {
                ErrorLogging.LogException(err);
                string msg =
                    "There was an error reading the world file.\r\n" +
                    "This is usually caused by a corrupt save file or a world version newer than supported.\r\n\r\n" +
                    $"TEdit v{TEdit.App.Version}\r\n" +
                    $"TEdit Max World: {CompatibleVersion}\r\n" +
                    $"Current World: {curVersion}\r\n\r\n" +
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

        public Chest GetChestAtTile(int x, int y, bool findOrigin = false)
        {
            Vector2Int32 anchor = findOrigin ? GetAnchor(x, y) : new Vector2Int32(x, y);
            return Chests.FirstOrDefault(c => (c.X == anchor.X) && (c.Y == anchor.Y));
        }

        public Sign GetSignAtTile(int x, int y, bool findOrigin = false)
        {
            Vector2Int32 anchor = findOrigin ? GetAnchor(x, y) : new Vector2Int32(x, y);
            return Signs.FirstOrDefault(c => (c.X == anchor.X) && (c.Y == anchor.Y));
        }

        public TileEntity GetTileEntityAtTile(int x, int y, bool findOrigin = false)
        {
            Vector2Int32 anchor = findOrigin ? GetAnchor(x, y) : new Vector2Int32(x, y);
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

        public bool IsAnchor(int x, int y)
        {
            var anchor = GetAnchor(x, y);
            return anchor.X == x && anchor.Y == y;
        }

        // find upper left corner of sprites
        public Vector2Int32 GetAnchor(int x, int y)
        {
            Tile tile = Tiles[x, y];
            TileProperty tileprop = TileProperties[tile.Type];
            var size = tileprop.FrameSize[0];
            if (tileprop.IsFramed && (size.X > 1 || size.Y > 1 || tileprop.FrameSize.Length > 1))
            {
                if (tile.U == 0 && tile.V == 0)
                {
                    new Vector2Int32(x, y);
                }

                var sprite = World.Sprites2.FirstOrDefault(s => s.Tile == tile.Type);
                var style = sprite?.GetStyleFromUV(tile.GetUV());

                var sizeTiles = style?.Value?.SizeTiles ?? sprite?.SizeTiles?.FirstOrDefault() ?? tileprop.FrameSize.FirstOrDefault();


                int xShift = tile.U % ((tileprop.TextureGrid.X + 2) * sizeTiles.X) / (tileprop.TextureGrid.X + 2);
                int yShift = tile.V % ((tileprop.TextureGrid.Y + 2) * sizeTiles.Y) / (tileprop.TextureGrid.Y + 2);
                return new Vector2Int32(x - xShift, y - yShift);
            }
            else
                return new Vector2Int32(x, y);
        }

        public void Validate()
        {
            var t = TaskFactoryHelper.UiTaskFactory.StartNew(() =>
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
            });

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
                var anchor = GetAnchor(x, y);
                if (!Tiles[anchor.X, anchor.Y].IsActive || !Tile.IsTileEntity(Tiles[anchor.X, anchor.Y].Type))
                {
                    TaskFactoryHelper.ExecuteUiTask(() => TileEntities.Remove(tileEntity));
                }
            }

            OnProgressChanged(this,
                    new ProgressChangedEventArgs(0, "Validating Complete..."));

            if (Chests.Count > World.MaxChests)
                throw new ArgumentOutOfRangeException($"Chest Count is {Chests.Count} which is greater than {World.MaxChests}.");
            if (Signs.Count > World.MaxSigns)
                throw new ArgumentOutOfRangeException($"Sign Count is {Signs.Count} which is greater than {World.MaxSigns}.");
        }

        private void ValSpecial(int x, int y)
        {
            Tile curTile = Tiles[x, y];
            //validate chest entry exists
            if (Tile.IsChest(curTile.Type))
            {
                if (IsAnchor(x, y) && GetChestAtTile(x, y, true) == null)
                {
                    Chests.Add(new Chest(x, y));
                }
            }
            //validate sign entry exists
            else if (Tile.IsSign(curTile.Type))
            {
                if (IsAnchor(x, y) && GetSignAtTile(x, y, true) == null)
                {
                    Signs.Add(new Sign(x, y, string.Empty));
                }
            }
            //validate TileEntity
            else if (Tile.IsTileEntity(curTile.Type))
            {
                if (IsAnchor(x, y) && GetTileEntityAtTile(x, y, true) == null)
                {
                    var TE = TileEntity.CreateForTile(curTile, x, y, TileEntities.Count);
                    TileEntities.Add(TE);
                }
            }
        }

        public void UpgradeLegacyTileEntities()
        {
            // don't upgrade legacy worlds
            if (this.Version < World.CompatibleVersion) return;

            Tile curTile = null;
            for (int x = 0; x < TilesWide; x++)
            {
                for (int y = 0; y < TilesHigh; y++)
                {
                    curTile = Tiles[x, y];

                    if (curTile.Type == (ushort)TileType.MannequinLegacy || curTile.Type == (ushort)TileType.WomannequinLegacy)
                    {
                        var anchor = GetAnchor(x, y);
                        int headId = (Tiles[anchor.X, anchor.Y].U - (Tiles[anchor.X, anchor.Y].U % 100)) / 100;
                        int torsoId = (Tiles[anchor.X, anchor.Y + 1].U - (Tiles[anchor.X, anchor.Y].U % 100)) / 100;
                        int feetId = (Tiles[anchor.X, anchor.Y + 2].U - (Tiles[anchor.X, anchor.Y].U % 100)) / 100;

                        var entity = TileEntity.CreateForTile(curTile, anchor.X, y, TileEntities.Count);
                        entity.Items[0].Id = (short)headId;
                        entity.Items[1].Id = (short)torsoId;
                        entity.Items[2].Id = (short)feetId;

                        TileEntity.PlaceEntity(entity, this);
                    }
                    if (curTile.Type == (ushort)TileType.WeaponRackLegacy)
                    {
                        var anchor = GetAnchor(x, y);
                        int itemId = (Tiles[anchor.X, anchor.Y + 1].U % 5000) - 100;
                        int prefix = (Tiles[anchor.X, anchor.Y + 1].U % 5000);

                        var entity = TileEntity.CreateForTile(curTile, anchor.X, anchor.Y, TileEntities.Count);
                        entity.NetId = (short)itemId;
                        entity.Prefix = (byte)prefix;
                        entity.StackSize = 1;

                        TileEntity.PlaceEntity(entity, this);
                    }
                }
            }
        }

        private void FixChand()
        {
            // don't upgrade legacy worlds
            if (this.Version < World.CompatibleVersion) return;

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
            // don't upgrade legacy worlds
            if (this.Version < World.CompatibleVersion) return;

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
            TEdit.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(
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

        public bool SlopeCheck(Vector2Int32 a, Vector2Int32 b)
        {
            if (a.X < 0 || a.X >= Size.X) return false;
            if (b.X < 0 || b.X >= Size.X) return false;
            if (a.Y < 0 || a.Y >= Size.Y) return false;
            if (b.Y < 0 || b.Y >= Size.Y) return false;

            var ta = Tiles[a.X, a.Y];
            var tb = Tiles[b.X, b.Y];


            var tpa = World.GetTileProperties(ta.Type);
            var tpb = World.GetTileProperties(tb.Type);

            if (ta.IsActive == tb.IsActive && !tpa.IsFramed && !tpb.IsFramed) return true;


            return false;
        }
    }
}
