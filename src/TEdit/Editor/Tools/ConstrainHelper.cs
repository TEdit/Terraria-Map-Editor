using System;
using TEdit.Geometry;

namespace TEdit.Editor.Tools;

/// <summary>
/// Shared constraint logic for snapping drawing to horizontal, vertical, or 45° diagonal lines.
/// </summary>
public static class ConstrainHelper
{
    public const int Horizontal = 0;
    public const int Vertical = 1;
    public const int Diagonal = 2;

    /// <summary>
    /// Determines constraint direction from movement deltas.
    /// Divides angle space into 3 zones of 30° each centered on 0°, 45°, and 90°.
    /// Threshold: tan(22.5°) ≈ 0.4142, so diagonal when 0.4142 &lt; dy/dx &lt; 2.4142.
    /// Using integer math: diagonal when dx * 5 &gt; dy * 2 AND dy * 5 &gt; dx * 2.
    /// </summary>
    public static int DetectDirection(int dx, int dy)
    {
        // Use ratio comparison to avoid floating point:
        // tan(22.5°) ≈ 0.414, approximate as 2/5 = 0.4
        // tan(67.5°) ≈ 2.414, approximate as 5/2 = 2.5
        if (dy * 5 > dx * 2 && dx * 5 > dy * 2)
            return Diagonal;  // angle is between ~22° and ~68°

        return dx < dy ? Vertical : Horizontal;
    }

    /// <summary>
    /// Snaps a tile position to the constrained direction from an anchor point.
    /// </summary>
    public static Vector2Int32 Snap(Vector2Int32 tile, Vector2Int32 anchor, int direction)
    {
        switch (direction)
        {
            case Vertical:
                return new Vector2Int32(anchor.X, tile.Y);
            case Diagonal:
                // Project onto nearest 45° line: |dx| == |dy|
                int dx = tile.X - anchor.X;
                int dy = tile.Y - anchor.Y;
                int dist = (Math.Abs(dx) + Math.Abs(dy)) / 2;
                int sx = dx >= 0 ? 1 : -1;
                int sy = dy >= 0 ? 1 : -1;
                return new Vector2Int32(anchor.X + dist * sx, anchor.Y + dist * sy);
            default: // Horizontal
                return new Vector2Int32(tile.X, anchor.Y);
        }
    }
}
