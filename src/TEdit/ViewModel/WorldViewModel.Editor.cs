using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using TEdit.Common;
using TEdit.Configuration;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Framework.Threading;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.Terraria;
using TEdit.Utility;
using TEdit.View.Popups;

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
        this.SelectedTabIndex = 3; // Open the clipboard tab.
    }

    public async void CropWorld()
    {
        if (CurrentWorld == null) return;
        if (!CanCopy())
            return;

        bool addBorders = false;

        if (MessageBox.Show(
            "This will generate a new world within the selected region.\nAll progress outside of the cropped zone will be lost., Continue?",
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

        var area = Selection.SelectionArea;
        var sel = new RectangleInt32(
                      (int)area.Left,
                      (int)area.Top,
                      (int)area.Width,
                      (int)area.Height);

        int bL = addBorders ? 41 : 0;
        int bT = addBorders ? 41 : 0;
        int bR = addBorders ? 42 : 0;
        int bB = addBorders ? 42 : 0;

        // Remove existing selection.
        Selection.IsActive = false;

        // Generate New Worlds
        _loadTimer.Restart(); _loadTimer.Start(); _saveTimer.Stop();
        var uiProgress = new Progress<ProgressChangedEventArgs>(e => OnProgressChanged(this, e));
        var newWorld = await TransformWorldAsync(CurrentWorld, sel, bL, bT, bR, bB, uiProgress)
                              .ConfigureAwait(true);

        CurrentWorld = newWorld;
        FinalizeLoad();
    }

    public async void ExpandWorld()
    {
        if (CurrentWorld == null) return;

        var dlg = new ExpandWorldView(
            currentWidth: CurrentWorld.TilesWide,
            currentHeight: CurrentWorld.TilesHigh
        )
        {
            // Owner = Application.Current.MainWindow
        };

        if (dlg.ShowDialog() != true)
            return;

        int newWidth = dlg._newWidth;
        int newHeight = dlg._newHeight;

        // Calculates the inset (border) values required to center a rectangle of size.
        var sel = new RectangleInt32(0, 0, CurrentWorld.TilesWide, CurrentWorld.TilesHigh);

        int deltaW = newWidth - sel.Width;
        int deltaH = newHeight - sel.Height;

        int bL = deltaW / 2;
        int bR = deltaW - bL;
        int bT = deltaH / 2;
        int bB = deltaH - bT;

        // Generate New Worlds
        _loadTimer.Restart(); _loadTimer.Start(); _saveTimer.Stop();
        var uiProgress = new Progress<ProgressChangedEventArgs>(e => OnProgressChanged(this, e));
        var newWorld = await TransformWorldAsync(CurrentWorld, sel, bL, bT, bR, bB, uiProgress)
                              .ConfigureAwait(true);

        CurrentWorld = newWorld;
        FinalizeLoad();
    }

    private Task<World> TransformWorldAsync(
        World sourceWorld,
        RectangleInt32 selectionArea, // The region you want to keep.
        int borderLeft,
        int borderTop,
        int borderRight,
        int borderBottom,
        IProgress<ProgressChangedEventArgs> progress = null)
    {
        return Task.Factory.StartNew(() =>
        {
            World w = new World();

            // TODO: Header data needs to be copied.
            using (var memStream = new MemoryStream())
            {
                var writer = new BinaryWriter(memStream);
                World.SaveV2(sourceWorld, writer);
                memStream.Flush();
                var reader = new BinaryReader(memStream);
                w.Version = sourceWorld.Version;
                World.LoadV2(reader, w);
            }

            var tileArea = new Rectangle(borderLeft, borderTop, (int)selectionArea.Width, selectionArea.Height);

            int worldWidth = selectionArea.Width + borderLeft + borderRight;
            int worldHeight = selectionArea.Height + borderTop + borderBottom;

            // w.ResetTime();
            w.CreationTime = System.DateTime.Now.ToBinary();
            w.TilesHigh = worldHeight;
            w.TilesWide = worldWidth;

            // Calc new ground heights.
            int topOffset = selectionArea.Top - borderTop;
            int leftOffset = selectionArea.Left - borderLeft;

            double groundLevel = w.GroundLevel - topOffset;
            double rockLevel = w.RockLevel - topOffset;

            if (groundLevel < 0 || groundLevel >= worldHeight) { groundLevel = Math.Min(350, worldHeight); }
            if (rockLevel < 0 || rockLevel >= worldHeight) { rockLevel = Math.Min(480, worldHeight); }

            w.GroundLevel = groundLevel;
            w.RockLevel = rockLevel;


            // Shift spawn point.
            int spawnX = w.SpawnX - leftOffset;
            int spawnY = w.SpawnY - topOffset;

            // Check out of bounds, and move.
            if (spawnX < tileArea.Left || spawnX > tileArea.Right) { spawnX = worldWidth / 2; };
            if (spawnY < tileArea.Top || spawnY > tileArea.Bottom) { spawnY = (int)groundLevel / 2; };

            w.SpawnX = spawnX;
            w.SpawnY = spawnY;

            // Shift dungeon point.
            int dungeonX = w.DungeonX - leftOffset;
            int dungeonY = w.DungeonY - topOffset;

            // Check out of bounds, and move.
            if (dungeonX < tileArea.Left || dungeonX > tileArea.Right) { dungeonX = worldWidth / 2; };
            if (dungeonY < tileArea.Top || dungeonY > tileArea.Bottom) { dungeonY = (int)groundLevel / 4; };

            w.DungeonX = dungeonX;
            w.DungeonY = dungeonY;

            // Calc size.
            w.BottomWorld = w.TilesHigh * 16;
            w.RightWorld = w.TilesWide * 16;

            // Generate empty tiles.
            var tiles = new Tile[w.TilesWide, w.TilesHigh];
            var tile = new Tile(); // Empty tile.
            for (int y = 0; y < w.TilesHigh; y++)
            {
                int percent = Calc.ProgressPercentage(y, w.TilesHigh);
                progress?.Report(new ProgressChangedEventArgs(percent, "Cloning World..."));

                // Generate no extra tiles.
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

            // Move NPCs.
            foreach (var npc in w.NPCs.ToList()) // To list since we are removing out of bounds NPCs below.
            {
                npc.Home -= new Vector2Int32(leftOffset, topOffset);
                if (!tileArea.Contains(npc.Home.X, npc.Home.Y))
                {
                    if (npc.Name == "Old Man")
                    {
                        npc.Home = new Vector2Int32(w.DungeonX, w.DungeonY); // Move homeless old man to dungeon.
                    }
                    else
                    {
                        npc.Home = new Vector2Int32(w.SpawnX, w.SpawnY); // Move homeless npc to spawn.
                        npc.IsHomeless = true;
                    }
                }
            }

            // Move mobs.
            foreach (var mob in w.Mobs.ToList()) // To list since we are removing out of bounds NPCs below.
            {
                mob.Home -= new Vector2Int32(leftOffset, topOffset);
                if (!tileArea.Contains(mob.Home.X, mob.Home.Y))
                {
                    w.Mobs.Remove(mob); // Remove out of bounds.
                }
            }

            // Move chests.
            foreach (var chest in w.Chests.ToList())
            {
                chest.X -= leftOffset;
                chest.Y -= topOffset;
                if (!tileArea.Contains(chest.X, chest.Y))
                {
                    w.Chests.Remove(chest); // Remove out of bounds.
                }
            }

            // Move signs.
            foreach (var sign in w.Signs.ToList())
            {
                sign.X -= leftOffset;
                sign.Y -= topOffset;
                if (!tileArea.Contains(sign.X, sign.Y))
                {
                    w.Signs.Remove(sign); // Remove out of bounds.
                }
            }

            // Move tile entities.
            foreach (var te in w.TileEntities.ToList())
            {
                te.PosX -= (short)leftOffset;
                te.PosY -= (short)topOffset;
                if (!tileArea.Contains(te.PosX, te.PosY))
                {
                    w.TileEntities.Remove(te); // Remove out of bounds.
                }
            }

            // Move pressure plates.
            foreach (var item in w.PressurePlates.ToList()) // To list since we are removing out of bounds NPCs below.
            {
                item.PosX -= leftOffset;
                item.PosY -= topOffset;
                if (!tileArea.Contains(item.PosX, item.PosY))
                {
                    w.PressurePlates.Remove(item); // Remove out of bounds.
                }
            }


            // Move Town manager items.
            foreach (var room in w.PlayerRooms.ToList())
            {
                room.Home -= new Vector2Int32(leftOffset, topOffset);
                if (!tileArea.Contains(room.Home.X, room.Home.Y))
                {
                    w.PlayerRooms.Remove(room); // Remove out of bounds.
                }
            }

            return w;
        });
    }

    private void FinalizeLoad()
    {
        CurrentFile = null;
        PixelMap = RenderEntireWorld();
        UpdateTitle();

        Points.Clear();
        Points.Add("Spawn");
        Points.Add("Dungeon");
        foreach (NPC npc in CurrentWorld.NPCs)
        {
            Points.Add(npc.Name);
        }

        MinimapImage = RenderMiniMap.Render(CurrentWorld);

        _loadTimer.Stop();
        OnProgressChanged(this, new ProgressChangedEventArgs(0,
             $"World loaded in {_loadTimer.Elapsed.TotalSeconds} seconds."));
        _saveTimer.Start();
    }

    public void EditPaste()
    {
        if (!CanPaste())
            return;

        this.SelectedTabIndex = 3; // Open the clipboard tab.
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

    public void UpdateRenderWorld()
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
    // OPTION: This can simply be combined with 'UpdateRenderWorld'.
    public void UpdateRenderWorldUsingFilter()
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
                            // Define defualt bools.
                            bool showWalls      = _showWalls;
                            bool showTiles      = _showTiles;
                            bool showLiquids    = _showLiquid;
                            bool showRedWire    = _showRedWires;
                            bool showBlueWire   = _showBlueWires;
                            bool showGreenWire  = _showGreenWires;
                            bool showYellowWire = _showYellowWires;

                            bool wallGrayscale       = false;
                            bool tileGrayscale       = false;
                            bool liquidGrayscale     = false;
                            bool redWireGrayscale    = false;
                            bool blueWireGrayscale   = false;
                            bool greenWireGrayscale  = false;
                            bool yellowWireGrayscale = false;

                            // Test the the filter for walls, tiles, liquids, wires, and sprites. 
                            if (FilterManager.WallIsNotAllowed(CurrentWorld.Tiles[x, y].Wall))                                                      // Check if this wall is not in the list.
                                if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)               showWalls = false;              // Hide walls not in list.
                                else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale)     wallGrayscale = true;           // Grayscale walls not in list.

                            if (FilterManager.TileIsNotAllowed(CurrentWorld.Tiles[x, y].Type)                                                       // Since sprites are under the tile denomination, we combine them.
                                && FilterManager.SpriteIsNotAllowed(CurrentWorld.Tiles[x, y].Type))                                                 // Check if this block / sprite is not in the list.
                                if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)               showTiles = false;              // Hide blocks not in list.
                                else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale)     tileGrayscale = true;           // Grayscale blocks not in list.

                            if (FilterManager.LiquidIsNotAllowed(CurrentWorld.Tiles[x, y].LiquidType))                                              // Check if this liquid is not in the list.
                                if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)               showLiquids = false;            // Hide liquids not in list.
                                else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale)     liquidGrayscale = true;         // Grayscale liquids not in list.

                            // Use the HasWire bool to save on processing speed.
                            if (CurrentWorld.Tiles[x, y].HasWire)
                            {
                                if (CurrentWorld.Tiles[x, y].WireRed)                                                                               // Check if this tile contains a red wire.
                                    if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Red))                                                 // Check if this wire is not in the list.
                                        if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)           showRedWire = false;        // Hide wires not in list.
                                        else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale) redWireGrayscale = true;    // Grayscale wires not in list.

                                if (CurrentWorld.Tiles[x, y].WireBlue)                                                                              // Check if this tile contains a red wire.
                                    if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Blue))                                                // Check if this wire is not in the list.
                                        if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)           showBlueWire = false;       // Hide wires not in list.
                                        else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale) blueWireGrayscale = true;   // Grayscale wires not in list.

                                if (CurrentWorld.Tiles[x, y].WireGreen)                                                                             // Check if this tile contains a red wire.
                                    if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Green))                                               // Check if this wire is not in the list.
                                        if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)           showGreenWire = false;      // Hide wires not in list.
                                        else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale) greenWireGrayscale = true;  // Grayscale wires not in list.

                                if (CurrentWorld.Tiles[x, y].WireYellow)                                                                            // Check if this tile contains a red wire.
                                    if (FilterManager.WireIsNotAllowed(FilterManager.WireType.Yellow))                                              // Check if this wire is not in the list.
                                        if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Hide)           showYellowWire = false;     // Hide wires not in list.
                                        else if (FilterManager.CurrentFilterMode == FilterManager.FilterMode.Grayscale) yellowWireGrayscale = true; // Grayscale wires not in list.
                            }

                            // Test the the filter for custom background (solid color) mode.
                            if (FilterManager.CurrentBackgroundMode == FilterManager.BackgroundMode.Transparent)
                                curBgColor = Color.Transparent;
                            else if (FilterManager.CurrentBackgroundMode == FilterManager.BackgroundMode.Custom)
                                curBgColor = FilterManager.BackgroundModeCustomColor;

                            // Define the color based on the filter results.
                            Color color = Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, showWalls, showTiles, showLiquids, showRedWire, showBlueWire, showGreenWire, showYellowWire,
                                    wallGrayscale: wallGrayscale, tileGrayscale: tileGrayscale, liquidGrayscale: liquidGrayscale,
                                    redWireGrayscale: redWireGrayscale, blueWireGrayscale: blueWireGrayscale, greenWireGrayscale: greenWireGrayscale, yellowWireGrayscale: yellowWireGrayscale);

                            // Set the pixel data.
                            PixelMap.SetPixelColor(x, y, color);
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
