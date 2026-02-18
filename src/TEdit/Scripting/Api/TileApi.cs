using System;
using TEdit.Editor.Undo;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class TileApi
{
    private readonly World _world;
    private readonly IUndoManager _undo;

    public TileApi(World world, IUndoManager undo)
    {
        _world = world;
        _undo = undo;
    }

    private void Validate(int x, int y)
    {
        if (!_world.ValidTileLocation(x, y))
            throw new ArgumentException($"Tile location ({x}, {y}) is out of bounds (world is {_world.TilesWide}x{_world.TilesHigh})");
    }

    private void SaveUndo(int x, int y) => _undo.SaveTile(_world, x, y);

    // Read operations
    public bool IsActive(int x, int y) { Validate(x, y); return _world.Tiles[x, y].IsActive; }
    public int GetTileType(int x, int y) { Validate(x, y); return _world.Tiles[x, y].Type; }
    public int GetWall(int x, int y) { Validate(x, y); return _world.Tiles[x, y].Wall; }
    public int GetPaint(int x, int y) { Validate(x, y); return _world.Tiles[x, y].TileColor; }
    public int GetWallPaint(int x, int y) { Validate(x, y); return _world.Tiles[x, y].WallColor; }
    public int GetLiquidAmount(int x, int y) { Validate(x, y); return _world.Tiles[x, y].LiquidAmount; }
    public int GetLiquidType(int x, int y) { Validate(x, y); return (int)_world.Tiles[x, y].LiquidType; }
    public int GetFrameU(int x, int y) { Validate(x, y); return _world.Tiles[x, y].U; }
    public int GetFrameV(int x, int y) { Validate(x, y); return _world.Tiles[x, y].V; }
    public string GetSlope(int x, int y) { Validate(x, y); return _world.Tiles[x, y].BrickStyle.ToString(); }

    public bool GetWire(int x, int y, int color)
    {
        Validate(x, y);
        var t = _world.Tiles[x, y];
        return color switch
        {
            1 => t.WireRed,
            2 => t.WireBlue,
            3 => t.WireGreen,
            4 => t.WireYellow,
            _ => false
        };
    }

    // Write operations
    public void SetActive(int x, int y, bool active)
    {
        Validate(x, y); SaveUndo(x, y);
        _world.Tiles[x, y].IsActive = active;
    }

    public void SetType(int x, int y, int type)
    {
        Validate(x, y); SaveUndo(x, y);
        var t = _world.Tiles[x, y];
        t.Type = (ushort)type;
        t.IsActive = true;
    }

    public void SetWall(int x, int y, int wallType)
    {
        Validate(x, y); SaveUndo(x, y);
        _world.Tiles[x, y].Wall = (ushort)wallType;
    }

    public void SetPaint(int x, int y, int color)
    {
        Validate(x, y); SaveUndo(x, y);
        _world.Tiles[x, y].TileColor = (byte)color;
    }

    public void SetWallPaint(int x, int y, int color)
    {
        Validate(x, y); SaveUndo(x, y);
        _world.Tiles[x, y].WallColor = (byte)color;
    }

    public void SetLiquid(int x, int y, int amount, int type)
    {
        Validate(x, y); SaveUndo(x, y);
        var t = _world.Tiles[x, y];
        t.LiquidAmount = (byte)amount;
        t.LiquidType = (LiquidType)type;
    }

    public void SetWire(int x, int y, int color, bool enabled)
    {
        Validate(x, y); SaveUndo(x, y);
        var t = _world.Tiles[x, y];
        switch (color)
        {
            case 1: t.WireRed = enabled; break;
            case 2: t.WireBlue = enabled; break;
            case 3: t.WireGreen = enabled; break;
            case 4: t.WireYellow = enabled; break;
        }
    }

    public void SetSlope(int x, int y, string slope)
    {
        Validate(x, y); SaveUndo(x, y);
        if (Enum.TryParse<BrickStyle>(slope, true, out var style))
            _world.Tiles[x, y].BrickStyle = style;
    }

    public void SetFrameUV(int x, int y, int u, int v)
    {
        Validate(x, y); SaveUndo(x, y);
        _world.Tiles[x, y].U = (short)u;
        _world.Tiles[x, y].V = (short)v;
    }

    public void Clear(int x, int y)
    {
        Validate(x, y); SaveUndo(x, y);
        _world.Tiles[x, y].Reset();
    }

    public void Copy(int fromX, int fromY, int toX, int toY)
    {
        Validate(fromX, fromY);
        Validate(toX, toY);
        SaveUndo(toX, toY);
        var source = _world.Tiles[fromX, fromY];
        var dest = _world.Tiles[toX, toY];

        dest.IsActive = source.IsActive;
        dest.Type = source.Type;
        dest.U = source.U;
        dest.V = source.V;
        dest.Wall = source.Wall;
        dest.TileColor = source.TileColor;
        dest.WallColor = source.WallColor;
        dest.LiquidAmount = source.LiquidAmount;
        dest.LiquidType = source.LiquidType;
        dest.WireRed = source.WireRed;
        dest.WireBlue = source.WireBlue;
        dest.WireGreen = source.WireGreen;
        dest.WireYellow = source.WireYellow;
        dest.BrickStyle = source.BrickStyle;
        dest.Actuator = source.Actuator;
        dest.InActive = source.InActive;
    }
}
