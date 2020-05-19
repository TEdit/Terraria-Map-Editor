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
                        PixelMap.SetPixelColor(x, y, Render.PixelMap.GetTileColor(CurrentWorld.Tiles[x, y], curBgColor, _showWalls, _showTiles, _showLiquid, _showRedWires, _showBlueWires, _showGreenWires, _showYellowWires));
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
                        SetPixelAutomatic(curTile, actuator: isErase ? false : TilePicker.Actuator, actuatorInActive: isErase ? false : TilePicker.ActuatorInActive);
                    break;
                case PaintMode.Wire:
                    if (TilePicker.RedWireActive)
                        SetPixelAutomatic(curTile, wire: !isErase);
                    if (TilePicker.BlueWireActive)
                        SetPixelAutomatic(curTile, wire2: !isErase);
                    if (TilePicker.GreenWireActive)
                        SetPixelAutomatic(curTile, wire3: !isErase);
                    if (TilePicker.YellowWireActive)
                        SetPixelAutomatic(curTile, wire4: !isErase);
                    break;
                case PaintMode.Liquid:
                    SetPixelAutomatic(curTile, liquid: isErase ? (byte)0 : (byte)255, liquidType: TilePicker.LiquidType);
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
                (TilePicker.TileMaskMode == MaskMode.Match && curTile.Type == TilePicker.TileMask && curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive) ||
                (TilePicker.TileMaskMode == MaskMode.NotMatching && (curTile.Type != TilePicker.TileMask || !curTile.IsActive)))
            {
                if (erase)
                    SetPixelAutomatic(curTile, tile: -1);
                else
                    SetPixelAutomatic(curTile, tile: TilePicker.Tile);
            }
        }
        
        private void SetTrack(int x, int y, Tile curTile, bool erase, bool hammer, bool check)
        {
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
                if(erase)
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
                                       bool? wire = null,
                                       short? u = null,
                                       short? v = null,
                                       bool? wire2 = null,
                                       bool? wire3 = null,
                                       bool? wire4 = null,
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
                curTile.WireBlue = (bool)wire2;

            if (wire3 != null)
                curTile.WireGreen = (bool)wire3;

            if (wire4 != null)
                curTile.WireYellow = (bool)wire4;

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
                if (World.TileProperties[curTile.Type].IsSolid && !curTile.InActive && !World.TileProperties[curTile.Type].IsPlatform)
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
 