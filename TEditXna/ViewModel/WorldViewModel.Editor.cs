using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TEdit.Geometry.Primitives;
using TEdit.Utility;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;
using TEditXna.Editor;
using TEditXna.Render;
using TEditXNA.Terraria.Objects;

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
                        CurrentWorld.Tiles[x, y].Reset();

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
                        SetPixelAutomatic(curTile, actuator: TilePicker.Actuator, actuatorInActive: TilePicker.ActuatorInActive);
                    break;
                case PaintMode.Wire:
                    if (TilePicker.RedWireActive)
                        SetPixelAutomatic(curTile, wire: !isErase);
                    if (TilePicker.BlueWireActive)
                        SetPixelAutomatic(curTile, wire2: !isErase);
                    if (TilePicker.GreenWireActive)
                        SetPixelAutomatic(curTile, wire3: !isErase);
                    break;
                case PaintMode.Liquid:
                    SetPixelAutomatic(curTile, liquid: isErase ? (byte)0 : (byte)255, liquidType: TilePicker.LiquidType);
                    break;
            }


            // curTile.BrickStyle = TilePicker.BrickStyle;

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
                (TilePicker.TileMaskMode == MaskMode.Match && curTile.Type == TilePicker.TileMask && curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.NotMatching && curTile.Type != TilePicker.TileMask || !curTile.IsActive))
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
                                       LiquidType? liquidType = null,
                                       bool? wire = null,
                                       short? u = null,
                                       short? v = null,
                                       bool? wire2 = null,
                                       bool? wire3 = null,
                                       BrickStyle? brickStyle = null,
                                       bool? actuator = null, bool? actuatorInActive = null,
                                       int? tileColor = null,
                                       int? wallColor = null)
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
                    curTile.Type = (ushort)tile;
                    curTile.IsActive = true;
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
                curTile.Wall = (byte)wall;

            if (liquid != null)
            {
                curTile.LiquidAmount = (byte)liquid;
            }

            if (liquidType != null)
            {
                curTile.LiquidType = (LiquidType)liquidType;
            }

            if (wire != null)
                curTile.WireRed = (bool)wire;

            if (wire2 != null)
                curTile.WireGreen = (bool)wire2;

            if (wire3 != null)
                curTile.WireBlue = (bool)wire3;

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

            if (curTile.IsActive)
                if (World.TileProperties[curTile.Type].IsSolid)
                    curTile.LiquidAmount = 0;
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