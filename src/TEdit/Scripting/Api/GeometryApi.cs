using System.Collections.Generic;
using System.Linq;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class GeometryApi
{
    private readonly World _world;
    private readonly IUndoManager _undo;

    public GeometryApi(World world, IUndoManager undo)
    {
        _world = world;
        _undo = undo;
    }

    // Coordinate generation (returns lists of {x,y} for script iteration)
    public List<Dictionary<string, int>> Line(int x1, int y1, int x2, int y2)
    {
        return Shape.DrawLineTool(new Vector2Int32(x1, y1), new Vector2Int32(x2, y2))
            .Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } })
            .ToList();
    }

    public List<Dictionary<string, int>> Rect(int x, int y, int w, int h)
    {
        return Shape.DrawRectangle(new Vector2Int32(x, y), new Vector2Int32(w, h))
            .Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } })
            .ToList();
    }

    public List<Dictionary<string, int>> Ellipse(int cx, int cy, int rx, int ry)
    {
        return Shape.DrawEllipseCentered(new Vector2Int32(cx, cy), new Vector2Int32(rx, ry))
            .Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } })
            .ToList();
    }

    public List<Dictionary<string, int>> FillRect(int x, int y, int w, int h)
    {
        return Fill.FillRectangle(new Vector2Int32(x, y), new Vector2Int32(w, h))
            .Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } })
            .ToList();
    }

    public List<Dictionary<string, int>> FillEllipse(int cx, int cy, int rx, int ry)
    {
        return Fill.FillEllipseCentered(new Vector2Int32(cx, cy), new Vector2Int32(rx, ry))
            .Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } })
            .ToList();
    }

    // Higher-level: set tile type for coordinates
    public void SetTiles(int tileType, int x, int y, int w, int h)
    {
        var coords = Fill.FillRectangle(new Vector2Int32(x, y), new Vector2Int32(w, h));
        foreach (var p in coords)
        {
            if (!_world.ValidTileLocation(p)) continue;
            _undo.SaveTile(_world, p);
            var tile = _world.Tiles[p.X, p.Y];
            tile.Type = (ushort)tileType;
            tile.IsActive = true;
        }
    }

    public void SetWalls(int wallType, int x, int y, int w, int h)
    {
        var coords = Fill.FillRectangle(new Vector2Int32(x, y), new Vector2Int32(w, h));
        foreach (var p in coords)
        {
            if (!_world.ValidTileLocation(p)) continue;
            _undo.SaveTile(_world, p);
            _world.Tiles[p.X, p.Y].Wall = (ushort)wallType;
        }
    }

    public void ClearTiles(int x, int y, int w, int h)
    {
        var coords = Fill.FillRectangle(new Vector2Int32(x, y), new Vector2Int32(w, h));
        foreach (var p in coords)
        {
            if (!_world.ValidTileLocation(p)) continue;
            _undo.SaveTile(_world, p);
            _world.Tiles[p.X, p.Y].Reset();
        }
    }
}
