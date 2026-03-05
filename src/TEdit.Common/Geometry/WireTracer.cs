using System;
using System.Collections.Generic;

namespace TEdit.Geometry;

/// <summary>
/// BFS flood-fill tracer for connected wire networks.
/// Follows 4-connected tiles (N/E/S/W) with a matching wire color.
/// Junction boxes (tile 424) restrict propagation based on entry/exit direction.
/// </summary>
public static class WireTracer
{
    // Directions: 0=down(S), 1=up(N), 2=right(E), 3=left(W)
    // Matches Terraria's Wiring.HitWire direction encoding.
    private static readonly int[] Dx = { 0, 0, 1, -1 };
    private static readonly int[] Dy = { 1, -1, 0, 0 };

    /// <summary>
    /// Returns the junction box type at (x,y): -1 = not a junction box,
    /// 0 = Normal (straight-through), 1 = Left, 2 = Right.
    /// </summary>
    public delegate int JunctionDetector(int x, int y);

    public static HashSet<Vector2Int32> Trace(
        Func<int, int, bool> hasWireAt,
        int tilesWide, int tilesHigh,
        int startX, int startY,
        JunctionDetector getJunction = null)
    {
        var result = new HashSet<Vector2Int32>();
        if (!hasWireAt(startX, startY)) return result;

        if (getJunction == null)
        {
            // No junction info — fall back to simple BFS
            return TraceSimple(hasWireAt, tilesWide, tilesHigh, startX, startY);
        }

        // Direction-aware BFS: state is (x, y, entryDirection)
        // entryDirection = -1 means "start tile, can exit in all directions"
        var queue = new Queue<(int x, int y, int entryDir)>();
        var visited = new HashSet<(int x, int y, int dir)>();

        var start = new Vector2Int32(startX, startY);
        result.Add(start);

        // Enqueue start with all 4 entry directions so it can propagate everywhere
        for (int d = 0; d < 4; d++)
        {
            queue.Enqueue((startX, startY, d));
            visited.Add((startX, startY, d));
        }

        while (queue.Count > 0)
        {
            var (x, y, entryDir) = queue.Dequeue();
            int junction = getJunction(x, y);

            for (int exitDir = 0; exitDir < 4; exitDir++)
            {
                // If current tile is a junction box, check if this exit is allowed
                if (junction >= 0 && !IsJunctionExitAllowed(junction, entryDir, exitDir))
                    continue;

                int nx = x + Dx[exitDir];
                int ny = y + Dy[exitDir];

                if (nx < 0 || ny < 0 || nx >= tilesWide || ny >= tilesHigh) continue;
                if (!hasWireAt(nx, ny)) continue;

                // The entry direction into the neighbor is the same as our exit direction
                var state = (nx, ny, exitDir);
                if (visited.Contains(state)) continue;

                visited.Add(state);
                result.Add(new Vector2Int32(nx, ny));
                queue.Enqueue(state);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a junction box allows exiting in <paramref name="exitDir"/>
    /// when entered from <paramref name="entryDir"/>.
    /// Junction types: 0=Normal (straight), 1=Left, 2=Right.
    /// Directions: 0=down, 1=up, 2=right, 3=left.
    /// </summary>
    private static bool IsJunctionExitAllowed(int junctionType, int entryDir, int exitDir)
    {
        return junctionType switch
        {
            // Normal: straight-through only (continue same direction)
            0 => exitDir == entryDir,
            // Left: swaps down↔left (0↔3) and up↔right (1↔2)
            1 => (entryDir == 0 && exitDir == 3) ||
                 (entryDir == 3 && exitDir == 0) ||
                 (entryDir == 1 && exitDir == 2) ||
                 (entryDir == 2 && exitDir == 1),
            // Right: swaps down↔right (0↔2) and up↔left (1↔3)
            2 => (entryDir == 0 && exitDir == 2) ||
                 (entryDir == 2 && exitDir == 0) ||
                 (entryDir == 1 && exitDir == 3) ||
                 (entryDir == 3 && exitDir == 1),
            _ => true
        };
    }

    /// <summary>
    /// Simple BFS without direction tracking (legacy fallback).
    /// </summary>
    private static HashSet<Vector2Int32> TraceSimple(
        Func<int, int, bool> hasWireAt,
        int tilesWide, int tilesHigh,
        int startX, int startY)
    {
        var result = new HashSet<Vector2Int32>();
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
