using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TEdit.Common;
using TEdit.Configuration;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Framework.Threading;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Utility;

namespace TEdit.ViewModel;

public partial class WorldViewModel
{
    private WorldEditor _worldEditor;

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

                    var curBgColor = new Color(GetBackgroundColor(y).PackedValue);
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

        _clipboard.CopySelection(CurrentWorld, Selection.SelectionArea);
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
        Task.Factory.StartNew((Func<World>)(() =>
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

            int borderTop = addBorders ? 41 : 0;
            int borderLeft = addBorders ? 41 : 0;
            int borderRight = addBorders ? 42 : 0;
            int borderBottom = addBorders ? 42 : 0;

            var tileArea = new Rectangle(borderLeft, borderTop, (int)selectionArea.Width, selectionArea.Height);

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
            double rockLevel = w.RockLevel - topOffset;

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
                    if (tileArea.Contains(x, y))
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
        }))
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

    private WorldEditor WorldEditor
    {
        get => _worldEditor;
        set
        {
            _worldEditor = value;
            RaisePropertyChanged("TilePicker");
        }
    }

    public void SetPixel(int x, int y, PaintMode? mode = null, bool? erase = null)
    {
        if (CurrentWorld == null) return;
        if (TilePicker == null) return;

        WorldEditor.SetPixel(x, y, mode, erase);
        UpdateRenderPixel(x, y);
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
                        Color curBgColor = new Color(GetBackgroundColor(y).PackedValue);
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
        Color curBgColor = new Color(GetBackgroundColor(y).PackedValue);
        PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
    }

    public void UpdateRenderRegion(RectangleInt32 area)
    {
        Task.Factory.StartNew(
        (Action)(() =>
        {
            var bounded = new RectangleInt32(Math.Max(area.Left, 0),
                                              Math.Max(area.Top, 0),
                                              Math.Min((int)area.Width, CurrentWorld.TilesWide - Math.Max(area.Left, 0)),
                                              Math.Min(area.Height, CurrentWorld.TilesHigh - Math.Max(area.Top, 0)));
            if (CurrentWorld != null)
            {
                for (int y = bounded.Top; y < bounded.Bottom; y++)
                {
                    Color curBgColor = new Color(GetBackgroundColor(y).PackedValue);
                    OnProgressChanged(this, new ProgressChangedEventArgs(y.ProgressPercentage(CurrentWorld.TilesHigh), "Calculating Colors..."));
                    for (int x = bounded.Left; x < bounded.Right; x++)
                    {
                        PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
                    }
                }
            }
            OnProgressChanged(this, new ProgressChangedEventArgs(100, "Render Complete"));
        }));
    }



    private PixelMapManager RenderEntireWorld()
    {
        var pixels = new PixelMapManager();
        if (CurrentWorld != null)
        {
            pixels.InitializeBuffers(CurrentWorld.TilesWide, CurrentWorld.TilesHigh);

            for (int y = 0; y < CurrentWorld.TilesHigh; y++)
            {
                Color curBgColor = new Color(GetBackgroundColor(y).PackedValue);
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

    public TEditColor GetBackgroundColor(int y)
    {
        if (y < 80)
            return WorldConfiguration.GlobalColors["Space"];
        else if (y > CurrentWorld.TilesHigh - 192)
            return WorldConfiguration.GlobalColors["Hell"];
        else if (y > CurrentWorld.RockLevel)
            return WorldConfiguration.GlobalColors["Rock"];
        else if (y > CurrentWorld.GroundLevel)
            return WorldConfiguration.GlobalColors["Earth"];
        else
            return WorldConfiguration.GlobalColors["Sky"];
    }
}
