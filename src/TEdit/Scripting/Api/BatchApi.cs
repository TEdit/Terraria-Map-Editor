using System;
using System.Collections.Generic;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Scripting.Engine;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class BatchApi
{
    private readonly World _world;
    private readonly ISelection _selection;
    private readonly IUndoManager _undo;
    private readonly ScriptExecutionContext _context;

    public BatchApi(World world, ISelection selection, IUndoManager undo, ScriptExecutionContext context)
    {
        _world = world;
        _selection = selection;
        _undo = undo;
        _context = context;
    }

    public void ForEachTile(Action<int, int> callback)
    {
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        long total = (long)w * h;
        long count = 0;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                callback(x, y);

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
    }

    public void ForEachInSelection(Action<int, int> callback)
    {
        if (!_selection.IsActive) return;

        var area = _selection.SelectionArea;
        long total = (long)area.Width * area.Height;
        long count = 0;

        for (int x = area.Left; x < area.Right; x++)
        {
            for (int y = area.Top; y < area.Bottom; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();
                if (_world.ValidTileLocation(x, y))
                    callback(x, y);

                count++;
                if (count % 50_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
    }

    public List<Dictionary<string, int>> FindTiles(Func<int, int, bool> predicate)
    {
        var results = new List<Dictionary<string, int>>();
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        long total = (long)w * h;
        long count = 0;
        const int maxResults = 10_000;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                if (predicate(x, y))
                {
                    results.Add(new Dictionary<string, int> { { "x", x }, { "y", y } });
                    if (results.Count >= maxResults)
                    {
                        _context.OnWarn?.Invoke($"FindTiles hit max result limit ({maxResults})");
                        return results;
                    }
                }

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return results;
    }

    public int ReplaceTile(int fromType, int toType)
    {
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        int replaced = 0;
        long total = (long)w * h;
        long count = 0;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                var tile = _world.Tiles[x, y];
                if (tile.IsActive && tile.Type == fromType)
                {
                    _undo.SaveTile(_world, x, y);
                    tile.Type = (ushort)toType;
                    replaced++;
                }

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return replaced;
    }

    public List<Dictionary<string, int>> FindTilesByType(int tileType)
    {
        var results = new List<Dictionary<string, int>>();
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        long total = (long)w * h;
        long count = 0;
        const int maxResults = 10_000;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                var tile = _world.Tiles[x, y];
                if (tile.IsActive && tile.Type == tileType)
                {
                    results.Add(new Dictionary<string, int> { { "x", x }, { "y", y } });
                    if (results.Count >= maxResults)
                    {
                        _context.OnWarn?.Invoke($"FindTilesByType hit max result limit ({maxResults})");
                        return results;
                    }
                }

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return results;
    }

    public List<Dictionary<string, int>> FindTilesByWall(int wallType)
    {
        var results = new List<Dictionary<string, int>>();
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        long total = (long)w * h;
        long count = 0;
        const int maxResults = 10_000;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                if (_world.Tiles[x, y].Wall == wallType)
                {
                    results.Add(new Dictionary<string, int> { { "x", x }, { "y", y } });
                    if (results.Count >= maxResults)
                    {
                        _context.OnWarn?.Invoke($"FindTilesByWall hit max result limit ({maxResults})");
                        return results;
                    }
                }

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return results;
    }

    public int ClearTilesByType(int tileType)
    {
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        int cleared = 0;
        long total = (long)w * h;
        long count = 0;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                var tile = _world.Tiles[x, y];
                if (tile.IsActive && tile.Type == tileType)
                {
                    _undo.SaveTile(_world, x, y);
                    tile.Reset();
                    cleared++;
                }

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return cleared;
    }

    public int ReplaceTileInSelection(int fromType, int toType)
    {
        if (!_selection.IsActive) return 0;

        var area = _selection.SelectionArea;
        int replaced = 0;
        long total = (long)area.Width * area.Height;
        long count = 0;

        for (int x = area.Left; x < area.Right; x++)
        {
            for (int y = area.Top; y < area.Bottom; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                if (_world.ValidTileLocation(x, y))
                {
                    var tile = _world.Tiles[x, y];
                    if (tile.IsActive && tile.Type == fromType)
                    {
                        _undo.SaveTile(_world, x, y);
                        tile.Type = (ushort)toType;
                        replaced++;
                    }
                }

                count++;
                if (count % 50_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return replaced;
    }

    public int ReplaceWall(int fromType, int toType)
    {
        int w = _world.TilesWide;
        int h = _world.TilesHigh;
        int replaced = 0;
        long total = (long)w * h;
        long count = 0;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                var tile = _world.Tiles[x, y];
                if (tile.Wall == fromType)
                {
                    _undo.SaveTile(_world, x, y);
                    tile.Wall = (ushort)toType;
                    replaced++;
                }

                count++;
                if (count % 100_000 == 0)
                    _context.OnProgress?.Invoke((double)count / total);
            }
        }
        _context.OnProgress?.Invoke(1.0);
        return replaced;
    }
}
