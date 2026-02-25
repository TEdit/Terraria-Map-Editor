using System;
using System.Collections.Generic;

namespace TEdit.Geometry;

/// <summary>
/// Wire routing style for CAD wire drawing.
/// </summary>
public enum WireRoutingMode
{
    /// <summary>90-degree elbow: straight run then 90° turn.</summary>
    Elbow90,
    /// <summary>45-degree miter: straight run, staircase diagonal, straight run.</summary>
    Miter45
}

/// <summary>
/// Generates 4-connected wire routing paths for CAD-style wire drawing.
/// All paths guarantee side-adjacency only (no diagonal/corner connections).
/// </summary>
public static class WireRouter
{
    /// <summary>
    /// Auto-detect whether to route vertical-first based on the angle from start to end.
    /// Returns true if |dy| > |dx| (steeper than 45°).
    /// </summary>
    public static bool DetectVerticalFirst(Vector2Int32 start, Vector2Int32 end)
    {
        int absDx = Math.Abs(end.X - start.X);
        int absDy = Math.Abs(end.Y - start.Y);
        return absDy > absDx;
    }

    /// <summary>
    /// Route a 90-degree elbow path from start to end.
    /// Produces an L-shaped path: straight on primary axis, then 90° turn to destination.
    /// All consecutive tiles are 4-connected (share an edge).
    /// </summary>
    public static List<Vector2Int32> Route90(Vector2Int32 start, Vector2Int32 end, bool verticalFirst)
    {
        var path = new List<Vector2Int32>();

        if (start.X == end.X && start.Y == end.Y)
        {
            path.Add(start);
            return path;
        }

        int sx = Math.Sign(end.X - start.X);
        int sy = Math.Sign(end.Y - start.Y);

        if (verticalFirst)
        {
            // Vertical leg first
            if (sy != 0)
            {
                for (int y = start.Y; y != end.Y; y += sy)
                    path.Add(new Vector2Int32(start.X, y));
            }
            // Horizontal leg (includes the corner tile and end tile)
            if (sx != 0)
            {
                for (int x = start.X; x != end.X; x += sx)
                    path.Add(new Vector2Int32(x, end.Y));
            }
            path.Add(end);
        }
        else
        {
            // Horizontal leg first
            if (sx != 0)
            {
                for (int x = start.X; x != end.X; x += sx)
                    path.Add(new Vector2Int32(x, start.Y));
            }
            // Vertical leg (includes the corner tile and end tile)
            if (sy != 0)
            {
                for (int y = start.Y; y != end.Y; y += sy)
                    path.Add(new Vector2Int32(end.X, y));
            }
            path.Add(end);
        }

        // Deduplicate adjacent duplicates (corner tile may be emitted twice)
        for (int i = path.Count - 1; i > 0; i--)
        {
            if (path[i].X == path[i - 1].X && path[i].Y == path[i - 1].Y)
                path.RemoveAt(i);
        }

        return path;
    }

    /// <summary>
    /// Route a 45-degree miter path from start to end.
    /// Produces a path with: straight run, staircase (alternating H+V steps), straight run.
    /// The staircase is 4-connected: each step moves one axis then the other,
    /// so consecutive tiles share an edge (not just a corner).
    /// </summary>
    public static List<Vector2Int32> RouteMiter(Vector2Int32 start, Vector2Int32 end, bool verticalFirst)
    {
        var path = new List<Vector2Int32>();

        if (start.X == end.X && start.Y == end.Y)
        {
            path.Add(start);
            return path;
        }

        int dx = end.X - start.X;
        int dy = end.Y - start.Y;
        int absDx = Math.Abs(dx);
        int absDy = Math.Abs(dy);
        int sx = dx >= 0 ? 1 : -1;
        int sy = dy >= 0 ? 1 : -1;
        int diag = Math.Min(absDx, absDy);

        if (verticalFirst)
        {
            // Straight vertical portion first, then staircase, then straight horizontal
            int straightV = absDy - diag;
            int straightH = absDx - diag;
            int cx = start.X;
            int cy = start.Y;

            // Emit straight vertical run
            for (int i = 0; i < straightV; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cy += sy;
            }

            // Emit staircase: V step then H step for each diagonal unit
            for (int i = 0; i < diag; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cy += sy;
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
            }

            // Emit straight horizontal remainder
            for (int i = 0; i < straightH; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
            }

            // Final point
            path.Add(new Vector2Int32(cx, cy));
        }
        else
        {
            // Straight horizontal portion first, then staircase, then straight vertical
            int straightH = absDx - diag;
            int straightV = absDy - diag;
            int cx = start.X;
            int cy = start.Y;

            // Emit straight horizontal run
            for (int i = 0; i < straightH; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
            }

            // Emit staircase: H step then V step for each diagonal unit
            for (int i = 0; i < diag; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
                path.Add(new Vector2Int32(cx, cy));
                cy += sy;
            }

            // Emit straight vertical remainder
            for (int i = 0; i < straightV; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cy += sy;
            }

            // Final point
            path.Add(new Vector2Int32(cx, cy));
        }

        // Deduplicate adjacent duplicates at transitions
        for (int i = path.Count - 1; i > 0; i--)
        {
            if (path[i].X == path[i - 1].X && path[i].Y == path[i - 1].Y)
                path.RemoveAt(i);
        }

        return path;
    }

    /// <summary>
    /// Route a 45-degree miter path using thin (1-tile-per-step) diagonals.
    /// For tracks/platforms where diagonal tiles occupy a single cell
    /// instead of the 2-cell staircase pattern used by wires.
    /// </summary>
    public static List<Vector2Int32> RouteMiterThin(Vector2Int32 start, Vector2Int32 end, bool verticalFirst)
    {
        var path = new List<Vector2Int32>();

        if (start.X == end.X && start.Y == end.Y)
        {
            path.Add(start);
            return path;
        }

        int dx = end.X - start.X;
        int dy = end.Y - start.Y;
        int absDx = Math.Abs(dx);
        int absDy = Math.Abs(dy);
        int sx = dx >= 0 ? 1 : -1;
        int sy = dy >= 0 ? 1 : -1;
        int diag = Math.Min(absDx, absDy);

        if (verticalFirst)
        {
            int straightV = absDy - diag;
            int straightH = absDx - diag;
            int cx = start.X;
            int cy = start.Y;

            for (int i = 0; i < straightV; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cy += sy;
            }

            // Thin diagonal: one tile per step, moving both axes simultaneously
            for (int i = 0; i < diag; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
                cy += sy;
            }

            for (int i = 0; i < straightH; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
            }

            path.Add(new Vector2Int32(cx, cy));
        }
        else
        {
            int straightH = absDx - diag;
            int straightV = absDy - diag;
            int cx = start.X;
            int cy = start.Y;

            for (int i = 0; i < straightH; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
            }

            // Thin diagonal: one tile per step, moving both axes simultaneously
            for (int i = 0; i < diag; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cx += sx;
                cy += sy;
            }

            for (int i = 0; i < straightV; i++)
            {
                path.Add(new Vector2Int32(cx, cy));
                cy += sy;
            }

            path.Add(new Vector2Int32(cx, cy));
        }

        // Deduplicate adjacent duplicates at transitions
        for (int i = path.Count - 1; i > 0; i--)
        {
            if (path[i].X == path[i - 1].X && path[i].Y == path[i - 1].Y)
                path.RemoveAt(i);
        }

        return path;
    }

    /// <summary>
    /// Route a bus of parallel wires using 90° elbow routing.
    /// Wires are spaced 2 tiles apart (1-tile gap) to prevent side contact.
    /// Wire start positions are at the opposite edge of the brush from the movement direction.
    /// </summary>
    public static List<Vector2Int32> RouteBus90(
        Vector2Int32 anchor, Vector2Int32 cursor,
        int brushWidth, int brushHeight, bool verticalFirst)
    {
        return RouteBus(anchor, cursor, brushWidth, brushHeight, verticalFirst, spacing: 2, miter: false);
    }

    /// <summary>
    /// Route a bus of parallel wires using 45° miter routing.
    /// Wires are spaced 2 tiles apart (1-tile gap), same as 90° routing.
    /// Diagonal-touching wires don't connect, so no extra spacing is needed.
    /// </summary>
    public static List<Vector2Int32> RouteBusMiter(
        Vector2Int32 anchor, Vector2Int32 cursor,
        int brushWidth, int brushHeight, bool verticalFirst)
    {
        return RouteBus(anchor, cursor, brushWidth, brushHeight, verticalFirst, spacing: 2, miter: true);
    }

    /// <summary>
    /// Compute centered wire offsets for a given brush dimension and spacing.
    /// E.g. dimension=5, spacing=2 → [-2, 0, 2] (3 wires).
    /// </summary>
    public static int[] ComputeWireOffsets(int brushDimension, int spacing)
    {
        int numWires = Math.Max(1, (brushDimension + spacing - 1) / spacing);
        var offsets = new int[numWires];
        int totalSpan = (numWires - 1) * spacing;
        int firstOffset = -totalSpan / 2;
        for (int i = 0; i < numWires; i++)
            offsets[i] = firstOffset + i * spacing;
        return offsets;
    }

    private static List<Vector2Int32> RouteBus(
        Vector2Int32 anchor, Vector2Int32 cursor,
        int brushWidth, int brushHeight, bool verticalFirst,
        int spacing, bool miter)
    {
        // Compute wire count limited by both brush dimensions
        int wiresFromW = Math.Max(1, (brushWidth + spacing - 1) / spacing);
        int wiresFromH = Math.Max(1, (brushHeight + spacing - 1) / spacing);
        int numWires = Math.Min(wiresFromW, wiresFromH);

        // Centered offsets
        int totalSpan = (numWires - 1) * spacing;
        int firstOffset = -totalSpan / 2;

        // Movement direction
        int dx = cursor.X - anchor.X;
        int dy = cursor.Y - anchor.Y;
        int sx = dx > 0 ? 1 : (dx < 0 ? -1 : 0);
        int sy = dy > 0 ? 1 : (dy < 0 ? -1 : 0);

        // Edge offset: start wires at opposite edge of brush from movement
        int edgeX = brushWidth / 2;
        int edgeY = brushHeight / 2;

        var result = new List<Vector2Int32>();

        for (int i = 0; i < numWires; i++)
        {
            int offset = firstOffset + i * spacing;
            Vector2Int32 start, end;

            if (verticalFirst)
            {
                // First leg vertical → wires spread along X at start
                // Second leg horizontal → wires spread along Y at end
                // Start at opposite Y edge from movement
                // End uses -sy*sx*offset to prevent overlap at turns in all quadrants
                start = new Vector2Int32(
                    anchor.X + offset,
                    anchor.Y - sy * edgeY);
                end = new Vector2Int32(
                    cursor.X,
                    cursor.Y - sy * sx * offset);
            }
            else
            {
                // First leg horizontal → wires spread along Y at start
                // Second leg vertical → wires spread along X at end
                // Start at opposite X edge from movement
                // End uses -sx*sy*offset to prevent overlap at turns in all quadrants
                start = new Vector2Int32(
                    anchor.X - sx * edgeX,
                    anchor.Y + offset);
                end = new Vector2Int32(
                    cursor.X - sx * sy * offset,
                    cursor.Y);
            }

            var wirePath = miter
                ? RouteMiter(start, end, verticalFirst)
                : Route90(start, end, verticalFirst);

            result.AddRange(wirePath);
        }

        return result;
    }
}
