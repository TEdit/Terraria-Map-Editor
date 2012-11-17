using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BCCL.Geometry.Primitives;
using BCCL.Utility;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;
using TEditXna.Editor;
using TEditXna.Render;

namespace TEditXna.ViewModel
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
                        var curTile = CurrentWorld.Tiles[x, y];
                        curTile.IsActive = false;
                        curTile.Wall = 0;
                        curTile.HasWire = false;
                        curTile.Liquid = 0;
                        curTile.IsLava = false;

                        Color curBgColor = GetBackgroundColor(y);
                        PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
                    }
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
            Tile curTile = CurrentWorld.Tiles[x, y];
            PaintMode curMode = mode ?? TilePicker.PaintMode;
            bool isErase = erase ?? TilePicker.IsEraser;

            switch (curMode)
            {
                case PaintMode.Tile:
                    SetTile(curTile, isErase);
                    break;
                case PaintMode.Wall:
                    SetWall(curTile, isErase);
                    break;
                case PaintMode.TileAndWall:
                    SetTile(curTile, isErase);
                    SetWall(curTile, isErase);
                    break;
                case PaintMode.Wire:
                    SetPixelAutomatic(curTile, wire: !isErase);
                    break;
                case PaintMode.Liquid:
                    SetPixelAutomatic(curTile, liquid: isErase ? (byte)0 : (byte)255, isLava: TilePicker.IsLava);
                    break;
            }

            Color curBgColor = GetBackgroundColor(y);
            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
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
                                PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
                            }
                        }
                    }
                    OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete"));
                });
        }

        public void UpdateRenderPixel(Vector2Int32 p)
        {
            UpdateRenderPixel(p.X, p.Y);
        }
        public void UpdateRenderPixel(int x, int y)
        {
            Color curBgColor = GetBackgroundColor(y);
            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
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
                            PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
                        }
                    }
                }
                OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete"));
            });
        }

        private void SetWall(Tile curTile, bool erase)
        {
            if (TilePicker.WallMaskMode == MaskMode.Off ||
                (TilePicker.WallMaskMode == MaskMode.Tile && curTile.Wall == TilePicker.WallMask) ||
                (TilePicker.WallMaskMode == MaskMode.Empty && curTile.Wall == 0))
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
                (TilePicker.TileMaskMode == MaskMode.Tile && curTile.Type == TilePicker.TileMask && curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive))
            {
                if (erase)
                    SetPixelAutomatic(curTile, tile: -1);
                else
                    SetPixelAutomatic(curTile, tile: TilePicker.Tile);
            }
        }

        private void SetPixelAutomatic(Tile curTile,
                                       int? tile = null,
                                       int? wall = null,
                                       byte? liquid = null,
                                       bool? isLava = null,
                                       bool? wire = null,
                                       short? u = null,
                                       short? v = null)
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
                }
                else
                {
                    curTile.Type = (byte)tile;
                    curTile.IsActive = true;
                }
            }

            if (wall != null)
                curTile.Wall = (byte)wall;

            if (liquid != null)
                curTile.Liquid = (byte)liquid;

            if (isLava != null)
                curTile.IsLava = (bool)isLava;

            if (wire != null)
                curTile.HasWire = (bool)wire;

            if (curTile.IsActive)
                if (World.TileProperties[curTile.Type].IsSolid)
                    curTile.Liquid = 0;
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
                            throw new IndexOutOfRangeException(string.Format("Error with world format tile [{0},{1}] is not a valid location. World file version: {2}", x, y, CurrentWorld.Version));
                        pixels.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showWires));
                    }
                }
            }
            OnProgressChanged(this, new ProgressChangedEventArgs(0, "Render Complete"));
            return pixels;
        }

        public Color GetBackgroundColor(int y)
        {
            if (y < 80)
                return World.GlobalColors["Space"];
            if (y > CurrentWorld.TilesHigh - 192)
                return World.GlobalColors["Hell"];
            if (y > CurrentWorld.RockLevel)
                return World.GlobalColors["Rock"];
            if (y > CurrentWorld.GroundLevel)
                return World.GlobalColors["Earth"];

            return World.GlobalColors["Sky"];
        }
    }
}