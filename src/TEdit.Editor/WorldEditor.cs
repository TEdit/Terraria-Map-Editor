using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEdit.Configuration;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Editor;

public interface ISelection
{
    RectangleInt32 SelectionArea { get; set; }
    bool IsActive { get; set; }
    bool IsValid(int x, int y);
    bool IsValid(Vector2Int32 xy);
    void SetRectangle(Vector2Int32 p1, Vector2Int32 p2);
}

public delegate void NotifyTileChanged(int x, int y, int height = 1, int width = 1);
public delegate Task NotifyTileChangedAsync(int x, int y, int height = 1, int width = 1);

public interface INotifyTileChanged
{
    void UpdateTile(int x, int y, int height = 1, int width = 1);
}

public class WorldEditor : IDisposable
{
    private readonly World _world;
    private readonly ISelection _selection;
    private readonly IUndoManager _undo;
    private readonly NotifyTileChanged? _notifyTileChanged;
    public bool[] _checkTiles;
    public TilePicker TilePicker { get; }

    public WorldEditor(
        TilePicker tilePicker,
        World world,
        ISelection selection,
        IUndoManager undo,
        NotifyTileChanged? notifyTileChanged)
    {
        _world = world;
        _selection = selection;
        _undo = undo;
        _notifyTileChanged = notifyTileChanged;
        _checkTiles = new bool[_world.TilesWide * _world.TilesHigh];
        TilePicker = tilePicker;
    }

    public async Task BeginOperationAsync()
    {
        _checkTiles = new bool[_world.TilesWide * _world.TilesHigh];
        //await _undo.StartUndoAsync();
    }

    public async Task EndOperationAsync()
    {
        if (_undo == null) return;

        await _undo.SaveUndoAsync();

    }

    public BrushSettings Brush { get; set; } = new BrushSettings();

    public void SetPixel(int x, int y, PaintMode? mode = null, bool? erase = null)
    {
        if (_world == null) return;
        if (TilePicker == null) return;

        int index = GetTileIndex(x, y);
        if (_checkTiles[index]) { return; }
        // else { _checkTiles[index] = true; }

        Tile curTile = _world.Tiles[x, y];
        if (curTile == null) return;

        PaintMode curMode = mode ?? TilePicker.PaintMode;
        bool isErase = erase ?? TilePicker.IsEraser;

        switch (curMode)
        {
            case PaintMode.Sprites:
                if (_world.TileFrameImportant[curTile.Type])
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
                    SetPixelAutomatic(curTile, wallEchoCoating: TilePicker.WallCoatingEcho, wallIlluminantCoating: TilePicker.WallCoatingIlluminant);
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
                    liquid: (isErase || TilePicker.LiquidType == LiquidType.None) ? (byte)0 : (byte)TilePicker.LiquidAmountMode,
                    liquidType: TilePicker.LiquidType);
                break;
            case PaintMode.Track:
                SetTrack(x, y, curTile, isErase, (TilePicker.TrackMode == TrackMode.Hammer), true);
                break;
        }
    }

    private int GetTileIndex(int x, int y)
    {
        return x + y * _world.TilesWide;
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
            (TilePicker.TileMaskMode == MaskMode.Match && TilePicker.TileMask >= 0 && curTile.Type == TilePicker.TileMask && curTile.IsActive) ||
            (TilePicker.TileMaskMode == MaskMode.Match && TilePicker.TileMask == -1 && !curTile.IsActive) ||
            (TilePicker.TileMaskMode == MaskMode.Empty && !curTile.IsActive) ||
            (TilePicker.TileMaskMode == MaskMode.NotMatching && TilePicker.TileMask >= 0 && (curTile.Type != TilePicker.TileMask || !curTile.IsActive)) ||
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
        if (x <= 0 || y <= 0 || x >= this._world.TilesWide - 1 || y >= this._world.TilesHigh - 1)
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
                int u = curTile.U;
                int v = curTile.V;
                SetPixelAutomatic(curTile, tile: -1, u: 0, v: 0);
                if (u > 0)
                {
                    switch (Minecart.LeftSideConnection[u])
                    {
                        case 0: SetTrack(x - 1, y - 1, _world.Tiles[x - 1, y - 1], false, false, false); break;
                        case 1: SetTrack(x - 1, y, _world.Tiles[x - 1, y], false, false, false); break;
                        case 2: SetTrack(x - 1, y + 1, _world.Tiles[x - 1, y + 1], false, false, false); break;
                    }
                    switch (Minecart.RightSideConnection[u])
                    {
                        case 0: SetTrack(x + 1, y - 1, _world.Tiles[x + 1, y - 1], false, false, false); break;
                        case 1: SetTrack(x + 1, y, _world.Tiles[x + 1, y], false, false, false); break;
                        case 2: SetTrack(x + 1, y + 1, _world.Tiles[x + 1, y + 1], false, false, false); break;
                    }
                }
                if (v > 0)
                {
                    switch (Minecart.LeftSideConnection[v])
                    {
                        case 0: SetTrack(x - 1, y - 1, _world.Tiles[x - 1, y - 1], false, false, false); break;
                        case 1: SetTrack(x - 1, y, _world.Tiles[x - 1, y], false, false, false); break;
                        case 2: SetTrack(x - 1, y + 1, _world.Tiles[x - 1, y + 1], false, false, false); break;
                    }
                    switch (Minecart.RightSideConnection[v])
                    {
                        case 0: SetTrack(x + 1, y - 1, _world.Tiles[x + 1, y - 1], false, false, false); break;
                        case 1: SetTrack(x + 1, y, _world.Tiles[x + 1, y], false, false, false); break;
                        case 2: SetTrack(x + 1, y + 1, _world.Tiles[x + 1, y + 1], false, false, false); break;
                    }
                }
            }
            else
            {
                int num = 0;
                if (_world.Tiles[x - 1, y - 1] != null && _world.Tiles[x - 1, y - 1].Type == 314)
                    num++;
                if (_world.Tiles[x - 1, y] != null && _world.Tiles[x - 1, y].Type == 314)
                    num += 2;
                if (_world.Tiles[x - 1, y + 1] != null && _world.Tiles[x - 1, y + 1].Type == 314)
                    num += 4;
                if (_world.Tiles[x + 1, y - 1] != null && _world.Tiles[x + 1, y - 1].Type == 314)
                    num += 8;
                if (_world.Tiles[x + 1, y] != null && _world.Tiles[x + 1, y].Type == 314)
                    num += 16;
                if (_world.Tiles[x + 1, y + 1] != null && _world.Tiles[x + 1, y + 1].Type == 314)
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
                            if ((Minecart.LeftSideConnection[array[num6]] != Minecart.LeftSideConnection[array[num5]] ||
                                Minecart.RightSideConnection[array[num6]] != Minecart.RightSideConnection[array[num5]]) &&
                                Minecart.TrackType[array[num6]] == num4 && Minecart.LeftSideConnection[array[num6]] != -1 &&
                                Minecart.RightSideConnection[array[num6]] != -1)
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
                            if (Minecart.TrackType[array[num5]] == num4 &&
                                (Minecart.LeftSideConnection[array[num5]] == -1 ||
                                Minecart.RightSideConnection[array[num5]] == -1) == flag2)
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
                            case 0: SetTrack(x - 1, y - 1, _world.Tiles[x - 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x - 1, y, _world.Tiles[x - 1, y], false, false, false); break;
                            case 2: SetTrack(x - 1, y + 1, _world.Tiles[x - 1, y + 1], false, false, false); break;
                        }
                        switch (Minecart.RightSideConnection[curTile.U])
                        {
                            case 0: SetTrack(x + 1, y - 1, _world.Tiles[x + 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x + 1, y, _world.Tiles[x + 1, y], false, false, false); break;
                            case 2: SetTrack(x + 1, y + 1, _world.Tiles[x + 1, y + 1], false, false, false); break;
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
                            case 0: SetTrack(x - 1, y - 1, _world.Tiles[x - 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x - 1, y, _world.Tiles[x - 1, y], false, false, false); break;
                            case 2: SetTrack(x - 1, y + 1, _world.Tiles[x - 1, y + 1], false, false, false); break;
                        }
                        switch (Minecart.RightSideConnection[curTile.V])
                        {
                            case 0: SetTrack(x + 1, y - 1, _world.Tiles[x + 1, y - 1], false, false, false); break;
                            case 1: SetTrack(x + 1, y, _world.Tiles[x + 1, y], false, false, false); break;
                            case 2: SetTrack(x + 1, y + 1, _world.Tiles[x + 1, y + 1], false, false, false); break;
                        }
                    }
                }
            }
        }
    }

    private void SetPixelAutomatic(
        Tile curTile,
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
                if (WorldConfiguration.TileProperties[curTile.Type].IsSolid)
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
            if (WorldConfiguration.TileProperties[curTile.Type].IsSolid &&
                !curTile.InActive &&
                !WorldConfiguration.TileProperties[curTile.Type].IsPlatform &&
                curTile.Type != 52 && // Exclude Vines
                curTile.Type != 62 && // Exclude Jungle Vines
                curTile.Type != 115 && // Exclude Hallowed Vines, 
                curTile.Type != 205 && // Exclude Crimson Vines, 
                curTile.Type != 353 && // Exclude Vine Rope
                curTile.Type != 382 && // Exclude Vine Flowers
                curTile.Type != 365 && // Exclude Silk Rope
                curTile.Type != 366) // Exclude Web Rope
            {
                // curTile.LiquidAmount = 0;
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

    public void DrawLine(
        Vector2Int32 start,
        Vector2Int32 end)
    {
        var line = Shape.DrawLineTool(start, end).ToList();
        if (Brush.Shape == BrushShape.Square || Brush.Height <= 1 || Brush.Width <= 1)
        {
            for (int i = 1; i < line.Count; i++)
            {
                FillRectangleLine(line[i - 1], line[i]);
            }
        }
        else if (Brush.Shape == BrushShape.Round)
        {
            foreach (Vector2Int32 point in line)
            {
                FillRound(point);
            }
        }
        else if (Brush.Shape == BrushShape.Right || Brush.Shape == BrushShape.Left)
        {
            foreach (Vector2Int32 point in line)
            {
                FillSlope(point);
            }
        }
    }

    protected void FillRectangleLine(Vector2Int32 start, Vector2Int32 end)
    {
        var area = Fill.FillRectangleVectorCenter(
            start,
            end,
            new Vector2Int32(Brush.Width, Brush.Height))
            .ToList();
        FillSolid(area);
    }

    protected void FillRectangle(Vector2Int32 point)
    {
        var area = Fill.FillRectangleCentered(point, new Vector2Int32(Brush.Width, Brush.Height)).ToList();
        if (Brush.IsOutline)
        {
            var interrior = Fill.FillRectangleCentered(
                point,
                new Vector2Int32(
                    Brush.Width - Brush.Outline * 2,
                    Brush.Height - Brush.Outline * 2)).ToList();
            FillHollow(area, interrior);
        }
        else
        {
            FillSolid(area);
        }
    }

    protected void FillRound(Vector2Int32 point)
    {
        var area = Fill.FillEllipseCentered(point, new Vector2Int32(Brush.Width / 2, Brush.Height / 2)).ToList();
        if (Brush.IsOutline)
        {
            var interrior = Fill.FillEllipseCentered(point, new Vector2Int32(
                Brush.Width / 2 - Brush.Outline * 2,
                Brush.Height / 2 - Brush.Outline * 2)).ToList();
            FillHollow(area, interrior);
        }
        else
        {
            FillSolid(area);
        }
    }

    public void FillSolid(IList<Vector2Int32> area)
    {
        foreach (Vector2Int32 pixel in area)
        {
            if (!_world.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * _world.TilesWide;
            if (!_checkTiles[index])
            {
                _checkTiles[index] = true;
                if (_selection.IsValid(pixel))
                {
                    _undo.SaveTile(_world, pixel);
                    SetPixel(pixel.X, pixel.Y);

                    _notifyTileChanged?.Invoke(pixel.X, pixel.Y, 1, 1);
                }
            }
        }
    }

    public void FillHollow(IList<Vector2Int32> area, IList<Vector2Int32> interrior)
    {
        IEnumerable<Vector2Int32> border = area.Except(interrior).ToList();

        // Draw the border
        if (TilePicker.TileStyleActive)
        {
            foreach (Vector2Int32 pixel in border)
            {
                if (!_world.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _world.TilesWide;

                if (!_checkTiles[index])
                {
                    _checkTiles[index] = true;
                    if (_selection.IsValid(pixel))
                    {
                        _undo.SaveTile(_world, pixel);
                        if (TilePicker.WallStyleActive)
                        {
                            TilePicker.WallStyleActive = false;
                            SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                            TilePicker.WallStyleActive = true;
                        }
                        else
                            SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);

                        // BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                        _notifyTileChanged?.Invoke(pixel.X, pixel.Y, 1, 1);
                    }
                }
            }
        }

        // Draw the wall in the interrior, exclude the border so no overlaps
        foreach (Vector2Int32 pixel in interrior)
        {
            if (!_world.ValidTileLocation(pixel)) continue;

            if (_selection.IsValid(pixel))
            {
                _undo.SaveTile(_world, pixel);
                SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall, erase: true);

                if (TilePicker.WallStyleActive)
                {
                    if (TilePicker.TileStyleActive)
                    {
                        TilePicker.TileStyleActive = false;
                        SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                        TilePicker.TileStyleActive = true;
                    }
                    else
                        SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                }

                /* Heathtech */
                _notifyTileChanged?.Invoke(pixel.X, pixel.Y, 1, 1);
            }
        }
    }

    private void FillSlope(Vector2Int32 point)
    {
        Vector2Int32 leftPoint;
        Vector2Int32 rightPoint;

        if (Brush.Shape == BrushShape.Right)
        {
            leftPoint = new Vector2Int32(point.X - Brush.Width / 2, point.Y + Brush.Height / 2);
            rightPoint = new Vector2Int32(point.X + Brush.Width / 2, point.Y - Brush.Height / 2);
        }
        else
        {
            leftPoint = new Vector2Int32(point.X - Brush.Width / 2, point.Y - Brush.Height / 2);
            rightPoint = new Vector2Int32(point.X + Brush.Width / 2, point.Y + Brush.Height / 2);
        }
        var area = Shape.DrawLine(leftPoint, rightPoint).ToList();
        FillSolid(area);
    }

    public void Dispose()
    {
        _undo.Dispose();
    }
}
