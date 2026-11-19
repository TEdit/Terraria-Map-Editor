using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Terraria.Objects;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

// All coordinates are world tile coordinates. Bounded shapes use inclusive corners.
internal sealed record DrawingSpec(string Shape, int X, int Y, int EndX, int EndY, bool Filled,
    int Thickness, string Layer, int Material, bool SelectionOnly, int MatchId, int Style = 0)
{
    internal const int PixelBudget = 65536;
    internal string? Validate(World world, RectangleInt32? selection, out SpriteItem? sprite)
    {
        sprite = null;
        if (SelectionOnly && selection == null) return "no_selection";
        if (!world.ValidTileLocation(X, Y)) return "invalid_bounds";
        var version = WorldConfiguration.SaveConfiguration.GetData((int)world.Version);
        if (Layer == "wall" && Material > version.MaxWallId ||
            Layer == "tile" && (Material > version.MaxTileId || Material == (int)TileType.IceByRod))
            return "unsupported_material";
        if (Shape == "sprite") return ValidateSprite(world, selection, out sprite);
        if (!world.ValidTileLocation(EndX, EndY)) return "invalid_bounds";
        if (Thickness is < 1 or > 32) return "drawing_limit";
        if (Shape == "line")
        {
            int low = (Thickness - 1) / 2, high = Thickness / 2;
            if (!world.ValidTileLocation(Math.Min(X, EndX) - low, Math.Min(Y, EndY) - low) ||
                !world.ValidTileLocation(Math.Max(X, EndX) + high, Math.Max(Y, EndY) + high)) return "invalid_bounds";
        }
        else
        {
            if (EndX < X || EndY < Y) return "invalid_bounds";
        }
        if (CandidateCells(null) > PixelBudget) return "drawing_limit";
        if (Layer != "tile" && Layer != "wall") return "invalid_layer";
        var limit = Layer == "tile" ? WorldConfiguration.TileProperties.Count : WorldConfiguration.WallProperties.Count;
        if (Material < 0 || Material >= limit || MatchId < -1 || MatchId >= limit) return "invalid_material";
        if (Layer == "tile" && IsFramed(world, Material)) return "use_place_sprite";
        return null;
    }

    internal long CandidateCells(SpriteItem? sprite)
    {
        if (sprite != null) return (long)sprite.SizeTiles.X * sprite.SizeTiles.Y;
        if (Shape == "line") return (1L + Math.Max(Math.Abs(EndX - X), Math.Abs(EndY - Y))) * Thickness * Thickness;
        return (long)(EndX - X + 1) * (EndY - Y + 1);
    }

    private string? ValidateSprite(World world, RectangleInt32? selection, out SpriteItem? sprite)
    {
        lock (WorldConfiguration.Sprites2Lock)
            sprite = WorldConfiguration.Sprites2.FirstOrDefault(sheet => sheet.Tile == Material)?
                .Styles.FirstOrDefault(item => item.Style == Style);
        if (sprite == null) return "unknown_sprite";
        int width = sprite.SizeTiles.X, height = sprite.SizeTiles.Y;
        if (width <= 0 || height <= 0 || (long)width * height > 256) return "drawing_limit";
        if (!world.ValidTileLocation(X + width - 1, Y + height - 1)) return "invalid_bounds";
        for (int y = Y; y < Y + height; y++)
        for (int x = X; x < X + width; x++)
        {
            if (SelectionOnly && !selection!.Value.Contains(x, y)) return "outside_selection";
            if (world.Tiles[x, y].IsActive) return "occupied";
        }
        return null;
    }

    private static bool IsFramed(World world, int id) => id >= WorldConfiguration.TileProperties.Count ||
        WorldConfiguration.TileProperties[id].IsFramed || (id < world.TileFrameImportant.Length && world.TileFrameImportant[id]);

    internal IEnumerable<Vector2Int32> Points()
    {
        if (Shape == "line")
        {
            var seen = new HashSet<Vector2Int32>();
            foreach (var center in TEdit.Geometry.Shape.DrawLineTool(new(X, Y), new(EndX, EndY)))
            for (int dy = -((Thickness - 1) / 2); dy <= Thickness / 2; dy++)
            for (int dx = -((Thickness - 1) / 2); dx <= Thickness / 2; dx++)
            {
                var point = new Vector2Int32(center.X + dx, center.Y + dy);
                if (seen.Add(point)) yield return point;
            }
            yield break;
        }
        for (int y = Y; y <= EndY; y++)
        for (int x = X; x <= EndX; x++)
        {
            if (Shape == "ellipse")
            {
                if (InsideEllipse(x, y, 0) && (Filled || !InsideEllipse(x, y, Thickness))) yield return new(x, y);
            }
            else if (Filled || x - X < Thickness || EndX - x < Thickness || y - Y < Thickness || EndY - y < Thickness)
                yield return new(x, y);
        }
    }

    private bool InsideEllipse(int x, int y, int inset)
    {
        long width = EndX - X + 1 - 2 * inset, height = EndY - Y + 1 - 2 * inset;
        if (width <= 0 || height <= 0) return false;
        long dx = (2L * x - X - EndX) * height, dy = (2L * y - Y - EndY) * width;
        long area = width * height;
        return dx * dx + dy * dy <= area * area;
    }

    internal bool Paint(World world, IUndoManager undo, RectangleInt32? selection, Vector2Int32 point)
    {
        if (SelectionOnly && !selection!.Value.Contains(point.X, point.Y)) return false;
        ref var tile = ref world.Tiles[point.X, point.Y];
        if (Layer == "tile" && tile.IsActive && IsFramed(world, tile.Type)) return false;
        if (MatchId != -1 && (Layer == "tile" ? !tile.IsActive || tile.Type != MatchId : tile.Wall != MatchId)) return false;
        var next = tile;
        WorldEditor.ApplyMaterial(ref next, Layer == "tile" ? Material : null, Layer == "wall" ? Material : null);
        if (next.Equals(tile)) return false;
        undo.SaveTile(world, point.X, point.Y);
        tile = next;
        return true;
    }
}
