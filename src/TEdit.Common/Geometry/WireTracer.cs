using System;
using System.Collections.Generic;

namespace TEdit.Geometry;

/// <summary>
/// BFS flood-fill tracer for connected wire networks.
/// Follows 4-connected tiles (N/E/S/W) with a matching wire color.
/// </summary>
public static class WireTracer
{
    public static HashSet<Vector2Int32> Trace(
        Func<int, int, bool> hasWireAt,
        int tilesWide, int tilesHigh,
        int startX, int startY)
    {
        var result = new HashSet<Vector2Int32>();
        if (!hasWireAt(startX, startY)) return result;

        var queue = new Queue<Vector2Int32>();
        var start = new Vector2Int32(startX, startY);
        queue.Enqueue(start);
        result.Add(start);

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            TryEnqueue(pos.X, pos.Y - 1);
            TryEnqueue(pos.X + 1, pos.Y);
            TryEnqueue(pos.X, pos.Y + 1);
            TryEnqueue(pos.X - 1, pos.Y);
        }

        return result;

        void TryEnqueue(int x, int y)
        {
            if (x < 0 || y < 0 || x >= tilesWide || y >= tilesHigh) return;
            var v = new Vector2Int32(x, y);
            if (result.Contains(v)) return;
            if (!hasWireAt(x, y)) return;
            result.Add(v);
            queue.Enqueue(v);
        }
    }
}
