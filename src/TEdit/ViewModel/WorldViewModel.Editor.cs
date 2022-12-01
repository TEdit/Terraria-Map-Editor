using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TEdit.Geometry.Primitives;
using TEdit.Utility;
using Microsoft.Xna.Framework;
using TEdit.Terraria;
using TEdit.Editor;
using TEdit.Render;
using System.Windows;
using TEdit.MvvmLight.Threading;
using System.IO;

namespace TEdit.ViewModel
{
    public partial class WorldViewModel
    {
        public void EditDelete()
        {
            if (Selection.IsActive)
            {
                for (int x = Selection.SelectionArea.Left; x < Selection.SelectionArea.Right; x++)
                {
                    for (int y = Selection.SelectionArea.Top; y < Selection.SelectionArea.Bottom; y++)
                    {
                        UndoManager.SaveTile(x, y);
                        CurrentWorld.Tiles[x, y].Reset();

                        Color curBgColor = GetBackgroundColor(y);
                        PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
                    }
                }

                foreach (var te in CurrentWorld.TileEntities.Where(te => Selection.SelectionArea.Contains(te.PosX, te.PosY)).ToList())
                {
                    CurrentWorld.TileEntities.Remove(te);
                }
                foreach (var chest in CurrentWorld.Chests.Where(item => Selection.SelectionArea.Contains(item.X, item.Y)).ToList())
                {
                    CurrentWorld.Chests.Remove(chest);
                }
                foreach (var sign in CurrentWorld.Signs.Where(item => Selection.SelectionArea.Contains(item.X, item.Y)).ToList())
                {
                    CurrentWorld.Signs.Remove(sign);
                }

                UndoManager.SaveUndo();
            }
        }

        public void EditCopy()
        {
            if (!CanCopy())
                return;
            _clipboard.Buffer = _clipboard.GetSelectionBuffer();
            _clipboard.LoadedBuffers.Add(_clipboard.Buffer);
        }

        public void CropWorld()
        {
            if (CurrentWorld == null) return;
            if (!CanCopy())
                return;

            bool addBorders = false;

            if (MessageBox.Show(
                "This will generate a new world with a selected region. Any progress done to this world will be lost, Continue?",
                "Crop World?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            if (MessageBox.Show(
                "Add \"edge of world\" boundaries?",
                "Crop World:",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes)
            { addBorders = true; };

            // Create clipboard
            //_clipboard.Buffer = _clipboard.GetSelectionBuffer();
            //_clipboard.LoadedBuffers.Add(_clipboard.Buffer);

            // addborders
            // 41 block buffer left and top for "off-screen" on all sides.
            // 42 block buffer right and bottom for "off-screen" on all sides. 

            // Generate New Worlds
            _loadTimer.Reset();
            _loadTimer.Start();
            _saveTimer.Stop();

            World w = new World();
            Task.Factory.StartNew(() =>
            {
                var selectionArea = Selection.SelectionArea;
                Selection.IsActive = false;
                // TODO: header data needs to be copied
                using (var memStream = new MemoryStream())
                {
                    var writer = new BinaryWriter(memStream);
                    World.SaveV2(CurrentWorld, writer);
                    memStream.Flush();
                    var reader = new BinaryReader(memStream);
                    w.Version = CurrentWorld.Version;
                    World.LoadV2(reader, w);
                }


                int borderTop    = addBorders ? 41 : 0;
                int borderLeft   = addBorders ? 41 : 0;
                int borderRight  = addBorders ? 42 : 0;
                int borderBottom = addBorders ? 42 : 0;

                var tileArea = new Rectangle(borderLeft, borderTop, selectionArea.Width, selectionArea.Height);

                int worldWidth = selectionArea.Width + borderLeft + borderRight;
                int worldHeight = selectionArea.Height + borderTop + borderBottom;


                //w.ResetTime();
                w.CreationTime = System.DateTime.Now.ToBinary();
                w.TilesHigh = worldHeight;
                w.TilesWide = worldWidth;

                // calc new ground heights
                int topOffset = selectionArea.Top - borderTop;
                int leftOffset = selectionArea.Left - borderLeft;

                double groundLevel = w.GroundLevel - topOffset;
                double rockLevel =   w.RockLevel - topOffset;

                if (groundLevel < 0 || groundLevel >= worldHeight) { groundLevel = Math.Min(350, worldHeight); }
                if (rockLevel < 0 || rockLevel >= worldHeight) { rockLevel = Math.Min(480, worldHeight); }

                w.GroundLevel = groundLevel;
                w.RockLevel = rockLevel;


                // shift spawn point
                int spawnX = w.SpawnX - leftOffset;
                int spawnY = w.SpawnY - topOffset;

                // check out of bounds, and move
                if (spawnX < tileArea.Left || spawnX > tileArea.Right) { spawnX = worldWidth / 2; };
                if (spawnY < tileArea.Top || spawnY > tileArea.Bottom) { spawnY = (int)groundLevel / 2; };

                w.SpawnX = spawnX;
                w.SpawnY = spawnY;

                // shift dungeon point
                int dungeonX = w.DungeonX - leftOffset;
                int dungeonY = w.DungeonY - topOffset;

                // check out of bounds, and move
                if (dungeonX < tileArea.Left || dungeonX > tileArea.Right) { dungeonX = worldWidth / 2; };
                if (dungeonY < tileArea.Top || dungeonY > tileArea.Bottom) { dungeonY = (int)groundLevel / 4; };

                w.DungeonX = dungeonX;
                w.DungeonY = dungeonY;

                // calc size
                w.BottomWorld = w.TilesHigh * 16;
                w.RightWorld = w.TilesWide * 16;

                // generate empty tiles
                var tiles = new Tile[w.TilesWide, w.TilesHigh];
                var tile = new Tile(); // empty tile
                for (int y = 0; y < w.TilesHigh; y++)
                {
                    OnProgressChanged(w, new ProgressChangedEventArgs(Calc.ProgressPercentage(y, w.TilesHigh), "Cloning World..."));
                    // Generate No Extra Tiles
                    for (int x = 0; x < w.TilesWide; x++)
                    {
                        if (tileArea.Contains(x,y))
                        {
                            tiles[x, y] = w.Tiles[x + leftOffset, y + topOffset];
                        }
                        else
                        {
                            tiles[x, y] = (Tile)tile.Clone();
                        }
                    }
                }

                w.Tiles = tiles;

                // move NPCs
                foreach (var npc in w.NPCs.ToList()) // to list since we are removing out of bounds NPCs below
                {
                    npc.Home -= new Vector2Int32(leftOffset, topOffset);
                    if (!tileArea.Contains(npc.Home.X, npc.Home.Y))
                    {
                        if (npc.Name == "Old Man")
                        {
                            npc.Home = new Vector2Int32(w.DungeonX, w.DungeonY); // move homeless old man to dungeon
                        }
                        else
                        {
                            npc.Home = new Vector2Int32(w.SpawnX, w.SpawnY); // move homeless npc to spawn
                            npc.IsHomeless = true;
                        }
                    }
                }

                // move mobs
                foreach (var mob in w.Mobs.ToList()) // to list since we are removing out of bounds NPCs below
                {
                    mob.Home -= new Vector2Int32(leftOffset, topOffset);
                    if (!tileArea.Contains(mob.Home.X, mob.Home.Y))
                    {
                        w.Mobs.Remove(mob); // remove out of bounds
                    }
                }

                // move chests
                foreach (var chest in w.Chests.ToList())
                {
                    chest.X -= leftOffset;
                    chest.Y -= topOffset;
                    if (!tileArea.Contains(chest.X, chest.Y))
                    {
                        w.Chests.Remove(chest); // remove out of bounds
                    }
                }

                // move signs
                foreach (var sign in w.Signs.ToList())
                {
                    sign.X -= leftOffset;
                    sign.Y -= topOffset;
                    if (!tileArea.Contains(sign.X, sign.Y))
                    {
                        w.Signs.Remove(sign); // remove out of bounds
                    }
                }

                // move tile entities
                foreach (var te in w.TileEntities.ToList())
                {
                    te.PosX -= (short)leftOffset;
                    te.PosY -= (short)topOffset;
                    if (!tileArea.Contains(te.PosX, te.PosY))
                    {
                        w.TileEntities.Remove(te); // remove out of bounds
                    }
                }

                // move pressure plates
                foreach (var item in w.PressurePlates.ToList()) // to list since we are removing out of bounds NPCs below
                {
                    item.PosX -= leftOffset;
                    item.PosY -= topOffset;
                    if (!tileArea.Contains(item.PosX, item.PosY))
                    {
                        w.PressurePlates.Remove(item); // remove out of bounds
                    }
                }


                // Move Town manager items 
                foreach (var room in w.PlayerRooms.ToList())
                {
                    room.Home -= new Vector2Int32(leftOffset, topOffset);
                    if (!tileArea.Contains(room.Home.X, room.Home.Y))
                    {
                        w.PlayerRooms.Remove(room); // remove out of bounds
                    }
                }

                return w;
            })
                .ContinueWith(t => CurrentWorld = t.Result, TaskFactoryHelper.UiTaskScheduler)
                .ContinueWith(t => RenderEntireWorld())
                .ContinueWith(t =>
                {
                    CurrentFile = null;
                    PixelMap = t.Result;
                    UpdateTitle();

                    Points.Clear();
                    Points.Add("Spawn");
                    Points.Add("Dungeon");
                    foreach (NPC npc in CurrentWorld.NPCs)
                    {
                        Points.Add(npc.Name);
                    }

                    MinimapImage = RenderMiniMap.Render(CurrentWorld);

                    // if (addBounderies)
                    // {
                    //     Clipboard.PasteBufferIntoWorld(new Vector2Int32(41, 41));
                    //     UpdateRenderRegion(new Rectangle(41, 41, _clipboard.Buffer.Size.X + 42, _clipboard.Buffer.Size.Y + 42)); // Update Map
                    // } // Paste Clipboard Buffer Offset
                    // else
                    // {
                    //     Clipboard.PasteBufferIntoWorld(new Vector2Int32(0, 0));
                    //     UpdateRenderRegion(new Rectangle(0, 0, _clipboard.Buffer.Size.X, _clipboard.Buffer.Size.Y)); // Update Map
                    // } // Paste Clipboard Without Offset
                    // Clipboard.Remove(_clipboard.LoadedBuffers.Last()); // Remove Buffer
                    // 
                    _loadTimer.Stop();
                    OnProgressChanged(this, new ProgressChangedEventArgs(0,
                         $"World loaded in {_loadTimer.Elapsed.TotalSeconds} seconds."));
                    _saveTimer.Start();

                }, TaskFactoryHelper.UiTaskScheduler);
        }

        public void EditPaste()
        {
            if (!CanPaste())
                return;

            var pasteTool = Tools.FirstOrDefault(t => t.Name == "Paste");
            if (pasteTool != null)
            {
                SetActiveTool(pasteTool);
                PreviewChange();
            }
        }

        public void SetPixel(int x, int y, PaintMode? mode = null, bool? erase = null)
        {
            if (CurrentWorld == null) return;
            if (TilePicker == null) return;

            Tile curTile = CurrentWorld.Tiles[x, y];
            if (curTile == null) return;

            PaintMode curMode = mode ?? TilePicker.PaintMode;
            bool isErase = erase ?? TilePicker.IsEraser;

            switch (curMode)
            {
                case PaintMode.Sprites:
                    if (CurrentWorld.TileFrameImportant[curTile.Type])
                        SetTile(curTile, isErase);
                    break;
                case PaintMode.TileAndWall:
                    if (TilePicker.TileStyleActive)
                        SetTile(curTile, isErase);
                    if (TilePicker.WallStyleActive)
                        SetWall(curTile, isErase);
                    if (TilePicker.BrickStyleActive && TilePicker.ExtrasActive)
                        SetPixelAutomatic(curTile, brickStyle: TilePicker.BrickStyle);
                    if (TilePicker.TilePaintActive)
                        SetPixelAutomatic(curTile, tileColor: isErase ? 0 : TilePicker.TileColor);
                    if (TilePicker.WallPaintActive)
                        SetPixelAutomatic(curTile, wallColor: isErase ? 0 : TilePicker.WallColor);
                    if (TilePicker.ExtrasActive)
                        SetPixelAutomatic(curTile, actuator: isErase ? false : TilePicker.Actuator, actuatorInActive: isErase ? false : TilePicker.ActuatorInActive);
                    if (TilePicker.EnableTileCoating) 
                        SetPixelAutomatic(curTile, tileEchoCoating: TilePicker.TileCoatingEcho, tileIlluminantCoating: TilePicker.TileCoatingIlluminant);
                    if (TilePicker.EnableWallCoating)
                        SetPixelAutomatic(curTile, tileEchoCoating: TilePicker.WallCoatingEcho, tileIlluminantCoating: TilePicker.WallCoatingIlluminant);
                    break;
                case PaintMode.Wire:
                    // Is Replace Mode Active?
                    bool WireReplaceMode = TilePicker.WireReplaceActive;

                    if (!WireReplaceMode)
                    {
                        // paint all wires in one call
                        SetPixelAutomatic(curTile,
                            wireRed: TilePicker.RedWireActive ? !isErase : null,
                            wireBlue: TilePicker.BlueWireActive ? !isErase : null,
                            wireGreen: TilePicker.GreenWireActive ? !isErase : null,
                            wireYellow: TilePicker.YellowWireActive ? !isErase : null
                            );
                    }
                    else
                    {
                        WireReplaceMode curWireBits = Editor.WireReplaceMode.Off;
                        if (curTile.WireRed) { curWireBits |= Editor.WireReplaceMode.Red; }
                        if (curTile.WireBlue) { curWireBits |= Editor.WireReplaceMode.Blue; }
                        if (curTile.WireGreen) { curWireBits |= Editor.WireReplaceMode.Green; }
                        if (curTile.WireYellow) { curWireBits |= Editor.WireReplaceMode.Yellow; }


                        WireReplaceMode turnOnWires = Editor.WireReplaceMode.Off;
                        WireReplaceMode turnOffWires = Editor.WireReplaceMode.Off;

                        if (TilePicker.WireReplaceRed && curTile.WireRed)
                        {
                            turnOffWires |= Editor.WireReplaceMode.Red;   // remove red
                            turnOnWires |= TilePicker.WireReplaceModeRed; // add back red's replacement
                        }

                        if (TilePicker.WireReplaceBlue && curTile.WireBlue)
                        {
                            turnOffWires |= Editor.WireReplaceMode.Blue;   // remove blue
                            turnOnWires |= TilePicker.WireReplaceModeBlue; // add back blue's replacement
                        }

                        if (TilePicker.WireReplaceGreen && curTile.WireGreen)
                        {
                            turnOffWires |= Editor.WireReplaceMode.Green;   // remove Green
                            turnOnWires |= TilePicker.WireReplaceModeGreen; // add back Green's replacement
                        }

                        if (TilePicker.WireReplaceYellow && curTile.WireYellow)
                        {
                            turnOffWires |= Editor.WireReplaceMode.Yellow;   // remove Yellow
                            turnOnWires |= TilePicker.WireReplaceModeYellow; // add back Yellow's replacement
                        }

                        // apply off, then on
                        curWireBits = curWireBits & ~turnOffWires;
                        curWireBits |= turnOnWires;

                        SetPixelAutomatic(curTile, 
                            wireRed: curWireBits.HasFlag(Editor.WireReplaceMode.Red),
                            wireBlue: curWireBits.HasFlag(Editor.WireReplaceMode.Blue),
                            wireGreen: curWireBits.HasFlag(Editor.WireReplaceMode.Green),
                            wireYellow: curWireBits.HasFlag(Editor.WireReplaceMode.Yellow));
                    }

                    // stack on junction boxes
                    if (TilePicker.JunctionBoxMode != JunctionBoxMode.None)
                    {
                        if (isErase &&
                            curTile.Type == (int)TileType.JunctionBox &&
                            curTile.U == (short)TilePicker.JunctionBoxMode)
                        {
                            // erase junction box matching selection only. Set tile also checks masks
                            SetTile(curTile, true);
                        }
                        else if (!isErase)
                        {
                            SetPixelAutomatic(curTile, tile: (int)TileType.JunctionBox, u: (short)TilePicker.JunctionBoxMode, v: 0);
                        }
                    }

                    break;
                case PaintMode.Liquid:
                    SetPixelAutomatic(
                        curTile,
                        liquid: (isErase || TilePicker.LiquidType == LiquidType.None) ? (byte)0 : (byte)255,
                        liquidType: TilePicker.LiquidType);
                    break;
                case PaintMode.Track:
                    SetTrack(x, y, curTile, isErase, (TilePicker.TrackMode == TrackMode.Hammer), true);
                    break;
            }


            // curTile.BrickStyle = TilePicker.BrickStyle;

            Color curBgColor = GetBackgroundColor(y);
            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
        }

        private void UpdateRenderWorld()
        {
            Task.Factory.StartNew(
                () =>
                {
                    if (CurrentWorld != null)
                    {
                        for (int y = 0; y < CurrentWorld.TilesHigh; y++)
                        {
                            Color curBgColor = GetBackgroundColor(y);
                            OnProgressChanged(this, new ProgressChangedEventArgs(y.ProgressPercentage(CurrentWorld.TilesHigh), "Calculating Colors..."));
                            for (int x = 0; x < CurrentWorld.TilesWide; x++)
                            {
                                PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
                            }
                        }
                    }
                    OnProgressChanged(this, new ProgressChangedEventArgs(100, "Render Complete"));
                });
        }

        public void UpdateRenderPixel(Vector2Int32 p)
        {
            UpdateRenderPixel(p.X, p.Y);
        }
        public void UpdateRenderPixel(int x, int y)
        {
            Color curBgColor = GetBackgroundColor(y);
            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
        }

        public void UpdateRenderRegion(Rectangle area)
        {
            Task.Factory.StartNew(
            () =>
            {
                var bounded = new Rectangle(Math.Max(area.Left, 0),
                                                  Math.Max(area.Top, 0),
                                                  Math.Min(area.Width, CurrentWorld.TilesWide - Math.Max(area.Left, 0)),
                                                  Math.Min(area.Height, CurrentWorld.TilesHigh - Math.Max(area.Top, 0)));
                if (CurrentWorld != null)
                {
                    for (int y = bounded.Top; y < bounded.Bottom; y++)
                    {
                        Color curBgColor = GetBackgroundColor(y);
                        OnProgressChanged(this, new ProgressChangedEventArgs(y.ProgressPercentage(CurrentWorld.TilesHigh), "Calculating Colors..."));
                        for (int x = bounded.Left; x < bounded.Right; x++)
                        {
                            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
                        }
                    }
                }
                OnProgressChanged(this, new ProgressChangedEventArgs(100, "Render Complete"));
            });
        }

        private void SetWall(Tile curTile, bool erase)
        {
            if (TilePicker.WallMaskMode == MaskMode.Off ||
                (TilePicker.WallMaskMode == MaskMode.Match && curTile.Wall == TilePicker.WallMask) ||
                (TilePicker.WallMaskMode == MaskMode.Empty && curTile.Wall == 0) ||
                (TilePicker.WallMaskMode == MaskMode.NotMatching && curTile.Wall != TilePicker.WallMask))
            {
                if (erase)
                    SetPixelAutomatic(curTile, wall: 0);
                else
                    SetPixelAutomatic(curTile, wall: TilePicker.Wall);
            }
        }

        private void SetTile(Tile curTile, bool erase)
        {
            if (TilePicker.TileMaskMode == MaskMode.Off ||
                (TilePicker.TileMaskMode == MaskMode.Match && TilePicker.TileMask > 0 && curTile.Type == TilePicker.TileMask && curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.Match && TilePicker.TileMask == -1 && !curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.NotMatching && TilePicker.TileMask > 0 && (curTile.Type != TilePicker.TileMask || !curTile.IsActive)) ||
                (TilePicker.TileMaskMode == MaskMode.NotMatching && (TilePicker.TileMask == -1 && curTile.IsActive))
                )
            {
                if (erase)
                    SetPixelAutomatic(curTile, tile: -1);
                else
                    SetPixelAutomatic(curTile, tile: TilePicker.Tile);
            }
        }

        private void SetTrack(int x, int y, Tile curTile, bool erase, bool hammer, bool check)
        {
            if (x <= 0 || y <= 0 || x >= this.CurrentWorld.TilesWide - 1 || y >= this.CurrentWorld.TilesHigh - 1)
            {
                return; // tracks not allowed on border of map.
            }


            if (TilePicker.TrackMode == TrackMode.Pressure)
            {
                if (erase)
                    if (curTile.V == 21)
                        curTile.V = 1;
                    else
                    {
                        if (curTile.U >= 20 && curTile.U <= 23)
                            curTile.U -= 20;
                    }
                else
                {
                    if (curTile.V == 1)
                        curTile.V = 21;
                    else
                    {
                        if (curTile.U >= 0 && curTile.U <= 3)
                            curTile.U += 20;
                        if (curTile.U == 14 || curTile.U == 24)
                            curTile.U += 22;
                        if (curTile.U == 15 || curTile.U == 25)
                            curTile.U += 23;
                    }
                }
            }
            else if (TilePicker.TrackMode == TrackMode.Booster)
            {
                if (erase)
                {
                    if (curTile.U == 30 || curTile.U == 31)
                        curTile.U = 1;
                    if (curTile.U == 32 || curTile.U == 34)
                        curTile.U = 8;
                    if (curTile.U == 33 || curTile.U == 35)
                        curTile.U = 9;
                }
                else
                {
                    if (curTile.U == 1)
                        curTile.U = 30;
                    if (curTile.U == 8)
                        curTile.U = 32;
                    if (curTile.U == 9)
                        curTile.U = 33;
                }
            }
            else
            {
                if (erase)
                {
                    int num1 = curTile.U;
                    int num2 = curTile.V;
                    SetPixelAutomatic(curTile, tile: -1, u: 0, v: 0);
                    if (num1 > 0)
                    {
                        switch (Minecart.LeftSideConnection[num1])
                        {
                            case 0: SetTrack(x - 1, y - 1, CurrentWorld.Tiles[x - 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x - 1, y, CurrentWorld.Tiles[x - 1, y], false, false, false); break;
                            case 2: SetTrack(x - 1, y + 1, CurrentWorld.Tiles[x - 1, y + 1], false, false, false); break;
                        }
                        switch (Minecart.RightSideConnection[num1])
                        {
                            case 0: SetTrack(x + 1, y - 1, CurrentWorld.Tiles[x + 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x + 1, y, CurrentWorld.Tiles[x + 1, y], false, false, false); break;
                            case 2: SetTrack(x + 1, y + 1, CurrentWorld.Tiles[x + 1, y + 1], false, false, false); break;
                        }
                    }
                    if (num2 > 0)
                    {
                        switch (Minecart.LeftSideConnection[num2])
                        {
                            case 0: SetTrack(x - 1, y - 1, CurrentWorld.Tiles[x - 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x - 1, y, CurrentWorld.Tiles[x - 1, y], false, false, false); break;
                            case 2: SetTrack(x - 1, y + 1, CurrentWorld.Tiles[x - 1, y + 1], false, false, false); break;
                        }
                        switch (Minecart.RightSideConnection[num2])
                        {
                            case 0: SetTrack(x + 1, y - 1, CurrentWorld.Tiles[x + 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x + 1, y, CurrentWorld.Tiles[x + 1, y], false, false, false); break;
                            case 2: SetTrack(x + 1, y + 1, CurrentWorld.Tiles[x + 1, y + 1], false, false, false); break;
                        }
                    }
                }
                else
                {
                    int num = 0;
                    if (CurrentWorld.Tiles[x - 1, y - 1] != null && CurrentWorld.Tiles[x - 1, y - 1].Type == 314)
                        num++;
                    if (CurrentWorld.Tiles[x - 1, y] != null && CurrentWorld.Tiles[x - 1, y].Type == 314)
                        num += 2;
                    if (CurrentWorld.Tiles[x - 1, y + 1] != null && CurrentWorld.Tiles[x - 1, y + 1].Type == 314)
                        num += 4;
                    if (CurrentWorld.Tiles[x + 1, y - 1] != null && CurrentWorld.Tiles[x + 1, y - 1].Type == 314)
                        num += 8;
                    if (CurrentWorld.Tiles[x + 1, y] != null && CurrentWorld.Tiles[x + 1, y].Type == 314)
                        num += 16;
                    if (CurrentWorld.Tiles[x + 1, y + 1] != null && CurrentWorld.Tiles[x + 1, y + 1].Type == 314)
                        num += 32;
                    int Front = curTile.U;
                    int Back = curTile.V;
                    int num4;
                    if (Front >= 0 && Front < Minecart.TrackType.Length)
                        num4 = Minecart.TrackType[Front];
                    else
                        num4 = 0;
                    int num5 = -1;
                    int num6 = -1;
                    int[] array = Minecart.TrackSwitchOptions[num];
                    if (!hammer)
                    {
                        if (curTile.Type != 314)
                        {
                            curTile.Type = (ushort)314;
                            curTile.IsActive = true;
                            Front = 0;
                            Back = -1;
                        }
                        int num7 = -1;
                        int num8 = -1;
                        bool flag = false;
                        for (int k = 0; k < array.Length; k++)
                        {
                            int num9 = array[k];
                            if (Back == array[k])
                                num6 = k;
                            if (Minecart.TrackType[num9] == num4)
                            {
                                if (Minecart.LeftSideConnection[num9] == -1 || Minecart.RightSideConnection[num9] == -1)
                                {
                                    if (Front == array[k])
                                    {
                                        num5 = k;
                                        flag = true;
                                    }
                                    if (num7 == -1)
                                        num7 = k;
                                }
                                else
                                {
                                    if (Front == array[k])
                                    {
                                        num5 = k;
                                        flag = false;
                                    }
                                    if (num8 == -1)
                                        num8 = k;
                                }
                            }
                        }
                        if (num8 != -1)
                        {
                            if (num5 == -1 || flag)
                                num5 = num8;
                        }
                        else
                        {
                            if (num5 == -1)
                            {
                                if (num4 == 2 || num4 == 1)
                                    return;
                                num5 = num7;
                            }
                            num6 = -1;
                        }
                    }
                    else if (hammer && curTile.Type == 314)
                    {
                        for (int l = 0; l < array.Length; l++)
                        {
                            if (Front == array[l])
                                num5 = l;
                            if (Back == array[l])
                                num6 = l;
                        }
                        int num10 = 0;
                        int num11 = 0;
                        for (int m = 0; m < array.Length; m++)
                        {
                            if (Minecart.TrackType[array[m]] == num4)
                            {
                                if (Minecart.LeftSideConnection[array[m]] == -1 || Minecart.RightSideConnection[array[m]] == -1)
                                    num11++;
                                else
                                    num10++;
                            }
                        }
                        if (num10 < 2 && num11 < 2)
                            return;
                        bool flag2 = num10 == 0;
                        bool flag3 = false;
                        if (!flag2)
                        {
                            while (!flag3)
                            {
                                num6++;
                                if (num6 >= array.Length)
                                {
                                    num6 = -1;
                                    break;
                                }
                                if ((Minecart.LeftSideConnection[array[num6]] != Minecart.LeftSideConnection[array[num5]] || Minecart.RightSideConnection[array[num6]] != Minecart.RightSideConnection[array[num5]]) && Minecart.TrackType[array[num6]] == num4 && Minecart.LeftSideConnection[array[num6]] != -1 && Minecart.RightSideConnection[array[num6]] != -1)
                                    flag3 = true;
                            }
                        }
                        if (!flag3)
                        {
                            while (true)
                            {
                                num5++;
                                if (num5 >= array.Length)
                                    break;
                                if (Minecart.TrackType[array[num5]] == num4 && (Minecart.LeftSideConnection[array[num5]] == -1 || Minecart.RightSideConnection[array[num5]] == -1) == flag2)
                                    goto IL_100;
                            }
                            num5 = -1;
                            while (true)
                            {
                                num5++;
                                if (Minecart.TrackType[array[num5]] == num4)
                                {
                                    if ((Minecart.LeftSideConnection[array[num5]] == -1 || Minecart.RightSideConnection[array[num5]] == -1) == flag2)
                                        break;
                                }
                            }
                        }
                    }
                IL_100:
                    if (num5 == -1)
                        curTile.U = 0;
                    else
                    {
                        curTile.U = (short)array[num5];
                        if (check)
                        {
                            switch (Minecart.LeftSideConnection[curTile.U])
                            {
                                case 0: SetTrack(x - 1, y - 1, CurrentWorld.Tiles[x - 1, y - 1], false, false, false); break;
                                case 1: SetTrack(x - 1, y, CurrentWorld.Tiles[x - 1, y], false, false, false); break;
                                case 2: SetTrack(x - 1, y + 1, CurrentWorld.Tiles[x - 1, y + 1], false, false, false); break;
                            }
                            switch (Minecart.RightSideConnection[curTile.U])
                            {
                                case 0: SetTrack(x + 1, y - 1, CurrentWorld.Tiles[x + 1, y - 1], false, false, false); break;
                                case 1: SetTrack(x + 1, y, CurrentWorld.Tiles[x + 1, y], false, false, false); break;
                                case 2: SetTrack(x + 1, y + 1, CurrentWorld.Tiles[x + 1, y + 1], false, false, false); break;
                            }
                        }
                    }
                    if (num6 == -1)
                        curTile.V = -1;
                    else
                    {
                        curTile.V = (short)array[num6];
                        if (check)
                        {
                            switch (Minecart.LeftSideConnection[curTile.V])
                            {
                                case 0: SetTrack(x - 1, y - 1, CurrentWorld.Tiles[x - 1, y - 1], false, false, false); break;
                                case 1: SetTrack(x - 1, y, CurrentWorld.Tiles[x - 1, y], false, false, false); break;
                                case 2: SetTrack(x - 1, y + 1, CurrentWorld.Tiles[x - 1, y + 1], false, false, false); break;
                            }
                            switch (Minecart.RightSideConnection[curTile.V])
                            {
                                case 0: SetTrack(x + 1, y - 1, CurrentWorld.Tiles[x + 1, y - 1], false, false, false); break;
                                case 1: SetTrack(x + 1, y, CurrentWorld.Tiles[x + 1, y], false, false, false); break;
                                case 2: SetTrack(x + 1, y + 1, CurrentWorld.Tiles[x + 1, y + 1], false, false, false); break;
                            }
                        }
                    }
                }
            }
        }

        private void SetPixelAutomatic(Tile curTile,
                                       int? tile = null,
                                       int? wall = null,
                                       byte? liquid = null,
                                       LiquidType? liquidType = null,
                                       bool? wireRed = null,
                                       short? u = null,
                                       short? v = null,
                                       bool? wireBlue = null,
                                       bool? wireGreen = null,
                                       bool? wireYellow = null,
                                       BrickStyle? brickStyle = null,
                                       bool? actuator = null, bool? actuatorInActive = null,
                                       int? tileColor = null,
                                       int? wallColor = null,
                                       bool? wallEchoCoating = null,
                                       bool? wallIlluminantCoating = null,
                                       bool? tileEchoCoating = null,
                                       bool? tileIlluminantCoating = null)
        {
            // Set Tile Data
            if (u != null)
                curTile.U = (short)u;
            if (v != null)
                curTile.V = (short)v;

            if (tile != null)
            {
                if (tile == -1)
                {
                    curTile.Type = 0;
                    curTile.IsActive = false;
                    curTile.InActive = false;
                    curTile.Actuator = false;
                    curTile.BrickStyle = BrickStyle.Full;
                    curTile.U = 0;
                    curTile.V = 0;
                }
                else
                {
                    curTile.Type = (ushort)tile;
                    curTile.IsActive = true;
                    if (World.TileProperties[curTile.Type].IsSolid)
                    {
                        curTile.U = -1;
                        curTile.V = -1;
                    }
                }
            }

            if (actuator != null && curTile.IsActive)
            {
                curTile.Actuator = (bool)actuator;
            }

            if (actuatorInActive != null && curTile.IsActive)
            {
                curTile.InActive = (bool)actuatorInActive;
            }

            if (brickStyle != null && TilePicker.BrickStyleActive)
            {
                curTile.BrickStyle = (BrickStyle)brickStyle;
            }

            if (wall != null)
                curTile.Wall = (ushort)wall;

            if (liquidType == LiquidType.None)
            {
                curTile.LiquidAmount = 0;
            }
            else if (liquid != null && liquidType != null)
            {
                curTile.LiquidAmount = (byte)liquid;
                curTile.LiquidType = (LiquidType)liquidType;
            }
            else
            {
                // do nothing with liquid
            }

            if (wireRed != null)
                curTile.WireRed = (bool)wireRed;

            if (wireBlue != null)
                curTile.WireBlue = (bool)wireBlue;

            if (wireGreen != null)
                curTile.WireGreen = (bool)wireGreen;

            if (wireYellow != null)
                curTile.WireYellow = (bool)wireYellow;

            if (tileColor != null)
            {
                if (curTile.IsActive)
                {
                    curTile.TileColor = (byte)tileColor;
                }
                else
                {
                    curTile.TileColor = (byte)0;
                }
            }

            if (wallColor != null)
            {
                if (curTile.Wall != 0)
                {
                    curTile.WallColor = (byte)wallColor;
                }
                else
                {
                    curTile.WallColor = (byte)0;
                }
            }

            // clear liquids for solid tiles
            if (curTile.IsActive)
            {
                if (World.TileProperties[curTile.Type].IsSolid &&
                    !curTile.InActive &&
                    !World.TileProperties[curTile.Type].IsPlatform &&
                    curTile.Type != 52 && // Exclude Vines
                    curTile.Type != 62 && // Exclude Jungle Vines
                    curTile.Type != 115 && // Exclude Hallowed Vines, 
                    curTile.Type != 205 && // Exclude Crimson Vines, 
                    curTile.Type != 353 && // Exclude Vine Rope
                    curTile.Type != 382 && // Exclude Vine Flowers
                    curTile.Type != 365 && // Exclude Silk Rope
                    curTile.Type != 366) // Exclude Web Rope
                {
                    curTile.LiquidAmount = 0;
                }
            }

            // handle coatings
            if (wallEchoCoating != null && curTile.Wall != 0)
            {
                curTile.InvisibleWall = (bool)wallEchoCoating;
            }

            if (wallIlluminantCoating != null && curTile.Wall != 0)
            {
                curTile.FullBrightWall = (bool)wallIlluminantCoating;
            }

            if (tileEchoCoating != null && curTile.IsActive)
            {
                curTile.InvisibleBlock = (bool)tileEchoCoating;
            }

            if (tileIlluminantCoating != null && curTile.IsActive)
            {
                curTile.FullBrightBlock = (bool)tileIlluminantCoating;
            }
        }

        private PixelMapManager RenderEntireWorld()
        {
            var pixels = new PixelMapManager();
            if (CurrentWorld != null)
            {
                pixels.InitializeBuffers(CurrentWorld.TilesWide, CurrentWorld.TilesHigh);

                for (int y = 0; y < CurrentWorld.TilesHigh; y++)
                {
                    Color curBgColor = GetBackgroundColor(y);
                    OnProgressChanged(this, new ProgressChangedEventArgs(y.ProgressPercentage(CurrentWorld.TilesHigh), "Calculating Colors..."));
                    for (int x = 0; x < CurrentWorld.TilesWide; x++)
                    {
                        if (y > CurrentWorld.TilesHigh || x > CurrentWorld.TilesWide)
                            throw new IndexOutOfRangeException(
                                $"Error with world format tile [{x},{y}] is not a valid location. World file version: {CurrentWorld.Version}");
                        pixels.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
                    }
                }
            }
            OnProgressChanged(this, new ProgressChangedEventArgs(100, "Render Complete"));
            return pixels;
        }

        public Color GetBackgroundColor(int y)
        {
            if (y < 80)
                return World.GlobalColors["Space"];
            else if (y > CurrentWorld.TilesHigh - 192)
                return World.GlobalColors["Hell"];
            else if (y > CurrentWorld.RockLevel)
                return World.GlobalColors["Rock"];
            else if (y > CurrentWorld.GroundLevel)
                return World.GlobalColors["Earth"];
            else
                return World.GlobalColors["Sky"];
        }
    }
}
