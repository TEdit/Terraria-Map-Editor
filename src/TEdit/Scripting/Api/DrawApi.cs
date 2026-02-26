using System;
using System.Collections.Generic;
using System.Linq;
using TEdit.Common.Geometry;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Scripting.Api;

public class DrawApi
{
    private readonly World _world;
    private readonly IUndoManager _undo;
    private readonly ISelection _selection;
    private readonly WorldEditor _editor;
    private readonly TileMaskSettings _maskSettings;

    public DrawApi(World world, IUndoManager undo, ISelection selection)
    {
        _world = world;
        _undo = undo;
        _selection = selection;

        var picker = new TilePicker();
        _maskSettings = new TileMaskSettings();
        _editor = new WorldEditor(picker, _maskSettings, world, selection, undo, notifyTileChanged: null);
    }

    // ── Picker Configuration ─────────────────────────────────────────

    public void SetTile(int tileType)
    {
        _editor.TilePicker.Tile = tileType;
        _editor.TilePicker.TileStyleActive = true;
        _editor.TilePicker.PaintMode = PaintMode.TileAndWall;
    }

    public void SetWall(int wallType)
    {
        _editor.TilePicker.Wall = wallType;
        _editor.TilePicker.WallStyleActive = true;
        _editor.TilePicker.PaintMode = PaintMode.TileAndWall;
    }

    public void SetErase(bool erase)
    {
        _editor.TilePicker.IsEraser = erase;
    }

    public void SetPaintMode(string mode)
    {
        _editor.TilePicker.PaintMode = mode?.ToLowerInvariant() switch
        {
            "tile" or "tileandwall" => PaintMode.TileAndWall,
            "wire" => PaintMode.Wire,
            "liquid" => PaintMode.Liquid,
            _ => PaintMode.TileAndWall
        };
    }

    public void SetTileColor(int color)
    {
        _editor.TilePicker.TileColor = color;
        _editor.TilePicker.TilePaintActive = color != 0;
    }

    public void SetWallColor(int color)
    {
        _editor.TilePicker.WallColor = color;
        _editor.TilePicker.WallPaintActive = color != 0;
    }

    public void SetBrickStyle(string style)
    {
        _editor.TilePicker.BrickStyle = ParseBrickStyle(style);
        _editor.TilePicker.BrickStyleActive = true;
        _editor.TilePicker.ExtrasActive = true;
    }

    public void SetActuator(bool enabled, bool inactive = false)
    {
        _editor.TilePicker.Actuator = enabled;
        _editor.TilePicker.ActuatorInActive = inactive;
        _editor.TilePicker.ExtrasActive = true;
    }

    public void SetTileCoating(bool echo = false, bool illuminant = false)
    {
        _editor.TilePicker.TileCoatingEcho = echo;
        _editor.TilePicker.TileCoatingIlluminant = illuminant;
        _editor.TilePicker.EnableTileCoating = true;
    }

    public void SetWallCoating(bool echo = false, bool illuminant = false)
    {
        _editor.TilePicker.WallCoatingEcho = echo;
        _editor.TilePicker.WallCoatingIlluminant = illuminant;
        _editor.TilePicker.EnableWallCoating = true;
    }

    public void SetLiquid(string type, string amount = "full")
    {
        _editor.TilePicker.PaintMode = PaintMode.Liquid;
        _editor.TilePicker.LiquidType = type?.ToLowerInvariant() switch
        {
            "water" => LiquidType.Water,
            "lava" => LiquidType.Lava,
            "honey" => LiquidType.Honey,
            "shimmer" => LiquidType.Shimmer,
            "none" => LiquidType.None,
            _ => LiquidType.Water
        };
        _editor.TilePicker.LiquidAmountMode = amount?.ToLowerInvariant() switch
        {
            "full" or "100" => LiquidAmountMode.OneHundredPercent,
            "half" or "50" => LiquidAmountMode.FiftyPercent,
            "quarter" or "25" => LiquidAmountMode.TwentyPercent,
            _ => LiquidAmountMode.OneHundredPercent
        };
    }

    public void SetWire(bool red = false, bool blue = false, bool green = false, bool yellow = false)
    {
        _editor.TilePicker.PaintMode = PaintMode.Wire;
        _editor.TilePicker.RedWireActive = red;
        _editor.TilePicker.BlueWireActive = blue;
        _editor.TilePicker.GreenWireActive = green;
        _editor.TilePicker.YellowWireActive = yellow;
    }

    public void SetTileMask(string mode, int tileType = -1)
    {
        _maskSettings.TileMaskMode = ParseMaskMode(mode);
        _maskSettings.TileMaskValue = tileType;
    }

    public void SetWallMask(string mode, int wallType = 0)
    {
        _maskSettings.WallMaskMode = ParseMaskMode(mode);
        _maskSettings.WallMaskValue = wallType;
    }

    public void SetBrickStyleMask(string mode, string style = "full")
    {
        _maskSettings.BrickStyleMaskMode = ParseMaskMode(mode);
        _maskSettings.BrickStyleMaskValue = ParseBrickStyle(style);
    }

    public void SetActuatorMask(string mode)
    {
        _maskSettings.ActuatorMaskMode = ParseMaskMode(mode);
    }

    public void SetTilePaintMask(string mode, int color = 0)
    {
        _maskSettings.TilePaintMaskMode = ParseMaskMode(mode);
        _maskSettings.TilePaintMaskValue = color;
    }

    public void SetWallPaintMask(string mode, int color = 0)
    {
        _maskSettings.WallPaintMaskMode = ParseMaskMode(mode);
        _maskSettings.WallPaintMaskValue = color;
    }

    public void SetTileCoatingMask(string echoMode = "off", string illuminantMode = "off")
    {
        _maskSettings.TileEchoMaskMode = ParseMaskMode(echoMode);
        _maskSettings.TileIlluminantMaskMode = ParseMaskMode(illuminantMode);
    }

    public void SetWallCoatingMask(string echoMode = "off", string illuminantMode = "off")
    {
        _maskSettings.WallEchoMaskMode = ParseMaskMode(echoMode);
        _maskSettings.WallIlluminantMaskMode = ParseMaskMode(illuminantMode);
    }

    public void SetWireMask(string redMode = "off", string blueMode = "off",
                            string greenMode = "off", string yellowMode = "off")
    {
        _maskSettings.WireRedMaskMode = ParseMaskMode(redMode);
        _maskSettings.WireBlueMaskMode = ParseMaskMode(blueMode);
        _maskSettings.WireGreenMaskMode = ParseMaskMode(greenMode);
        _maskSettings.WireYellowMaskMode = ParseMaskMode(yellowMode);
    }

    public void SetLiquidTypeMask(string mode, string type = "water")
    {
        _maskSettings.LiquidTypeMaskMode = ParseMaskMode(mode);
        _maskSettings.LiquidTypeMaskValue = type?.ToLowerInvariant() switch
        {
            "water" => LiquidType.Water,
            "lava" => LiquidType.Lava,
            "honey" => LiquidType.Honey,
            "shimmer" => LiquidType.Shimmer,
            "none" => LiquidType.None,
            _ => LiquidType.Water
        };
    }

    public void SetLiquidLevelMask(string mode, byte level = 0)
    {
        _maskSettings.LiquidLevelMaskMode = mode?.ToLowerInvariant() switch
        {
            "ignore" => LiquidLevelMaskMode.Ignore,
            "greaterthan" or "gt" => LiquidLevelMaskMode.GreaterThan,
            "lessthan" or "lt" => LiquidLevelMaskMode.LessThan,
            "equal" or "eq" => LiquidLevelMaskMode.Equal,
            _ => LiquidLevelMaskMode.Ignore
        };
        _maskSettings.LiquidLevelMaskValue = level;
    }

    public void SetMaskPreset(string preset)
    {
        _maskSettings.MaskPreset = preset?.ToLowerInvariant() switch
        {
            "off" => MaskPreset.Off,
            "exact" or "exactmatch" or "matchall" => MaskPreset.ExactMatch,
            "custom" or "matchcustom" => MaskPreset.Custom,
            _ => MaskPreset.Off
        };
    }

    public void ClearMasks()
    {
        _maskSettings.ClearAll();
    }

    public void SetExactMask(int x, int y)
    {
        if (!_world.ValidTileLocation(x, y)) return;
        _maskSettings.SetFromTile(_world.Tiles[x, y]);
    }

    public void SetBrush(int width, int height, string shape = "square")
    {
        _editor.Brush.Width = width;
        _editor.Brush.Height = height;
        _editor.Brush.Shape = shape?.ToLowerInvariant() switch
        {
            "round" => BrushShape.Round,
            "right" => BrushShape.Right,
            "left" => BrushShape.Left,
            "star" => BrushShape.Star,
            "triangle" => BrushShape.Triangle,
            "crescent" => BrushShape.Crescent,
            "donut" => BrushShape.Donut,
            "cross" or "x" => BrushShape.Cross,
            _ => BrushShape.Square
        };
    }

    public void SetRotation(double degrees)
    {
        _editor.Brush.Rotation = degrees;
    }

    public void SetFlip(bool horizontal = false, bool vertical = false)
    {
        _editor.Brush.FlipHorizontal = horizontal;
        _editor.Brush.FlipVertical = vertical;
    }

    public void SetSpray(bool enabled, int density = 50, int tickMs = 100)
    {
        _editor.Brush.IsSpray = enabled;
        _editor.Brush.SprayDensity = density;
        _editor.Brush.SprayTickMs = tickMs;
    }

    public void SetBrushOutline(int outline, bool enabled)
    {
        _editor.Brush.Outline = outline;
        _editor.Brush.IsOutline = enabled;
    }

    public void Reset()
    {
        _editor.TilePicker.PaintMode = PaintMode.TileAndWall;
        _editor.TilePicker.TileStyleActive = false;
        _editor.TilePicker.WallStyleActive = false;
        _editor.TilePicker.BrickStyleActive = false;
        _editor.TilePicker.ExtrasActive = false;
        _editor.TilePicker.TilePaintActive = false;
        _editor.TilePicker.WallPaintActive = false;
        _editor.TilePicker.EnableTileCoating = false;
        _editor.TilePicker.EnableWallCoating = false;
        _editor.TilePicker.IsEraser = false;
        _editor.TilePicker.RedWireActive = false;
        _editor.TilePicker.BlueWireActive = false;
        _editor.TilePicker.GreenWireActive = false;
        _editor.TilePicker.YellowWireActive = false;
        _maskSettings.ClearAll();
        _editor.Brush = new BrushSettings();
    }

    // ── Drawing Operations ───────────────────────────────────────────

    /// <summary>
    /// Draw a 1px line from (x1,y1) to (x2,y2) using the current tile picker settings.
    /// </summary>
    public void Pencil(int x1, int y1, int x2, int y2)
    {
        ResetCheckTiles();
        var points = Shape.DrawLineTool(
            new Vector2Int32(x1, y1),
            new Vector2Int32(x2, y2)).ToList();
        _editor.FillSolid(points);
    }

    /// <summary>
    /// Draw a brush-width line from (x1,y1) to (x2,y2).
    /// </summary>
    public void Brush(int x1, int y1, int x2, int y2)
    {
        ResetCheckTiles();
        _editor.DrawLine(
            new Vector2Int32(x1, y1),
            new Vector2Int32(x2, y2));
    }

    /// <summary>
    /// Flood fill from (x, y) using the current tile picker settings.
    /// Scanline flood fill algorithm matching FillTool behavior.
    /// </summary>
    public void Fill(int x, int y)
    {
        if (!_world.ValidTileLocation(x, y)) return;

        int bitmapWidth = _world.TilesWide;
        int bitmapHeight = _world.TilesHigh;

        var checkTiles = new bool[bitmapWidth * bitmapHeight];
        var ranges = new FloodFillRangeQueue();
        var originTile = _world.Tiles[x, y];

        // Seed the fill
        LinearFloodFill(ref x, ref y, ref originTile, checkTiles, ranges, bitmapWidth, bitmapHeight);

        while (ranges.Count > 0)
        {
            var range = ranges.Dequeue();
            int upY = range.Y - 1;
            int downY = range.Y + 1;

            for (int i = range.StartX; i <= range.EndX; i++)
            {
                // Fill upward
                if (upY >= 0)
                {
                    int upIdx = i + upY * bitmapWidth;
                    if (!checkTiles[upIdx] &&
                        CheckTileMatch(ref originTile, ref _world.Tiles[i, upY]) &&
                        _selection.IsValid(i, upY))
                    {
                        int xi = i;
                        LinearFloodFill(ref xi, ref upY, ref originTile, checkTiles, ranges, bitmapWidth, bitmapHeight);
                    }
                }

                // Fill downward
                if (downY < bitmapHeight)
                {
                    int downIdx = i + downY * bitmapWidth;
                    if (!checkTiles[downIdx] &&
                        CheckTileMatch(ref originTile, ref _world.Tiles[i, downY]) &&
                        _selection.IsValid(i, downY))
                    {
                        int xi = i;
                        LinearFloodFill(ref xi, ref downY, ref originTile, checkTiles, ranges, bitmapWidth, bitmapHeight);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Auto-slope tiles along a brush-width line from (x1,y1) to (x2,y2).
    /// </summary>
    public void Hammer(int x1, int y1, int x2, int y2)
    {
        ResetCheckTiles();
        var line = Shape.DrawLineTool(
            new Vector2Int32(x1, y1),
            new Vector2Int32(x2, y2)).ToList();

        foreach (var point in line)
        {
            // Generate brush area at each point using unified shape dispatch
            IList<Vector2Int32> area = _editor.GetShapePoints(point);

            foreach (var pixel in area)
            {
                if (!_world.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _world.TilesWide;
                if (_editor._checkTiles[index] != _editor._checkTileGeneration)
                {
                    _editor._checkTiles[index] = _editor._checkTileGeneration;

                    if (_selection.IsValid(pixel))
                    {
                        var style = GetAutoSlopeStyle(pixel);
                        if (style != null)
                        {
                            _undo.SaveTile(_world, pixel);
                            _world.Tiles[pixel.X, pixel.Y].BrickStyle = style.Value;
                        }
                    }
                }
            }
        }
    }

    // ── Wire Routing ───────────────────────────────────────────────

    /// <summary>
    /// Route a single wire path from (x1,y1) to (x2,y2).
    /// Uses the current draw.setWire() or draw.setTile() configuration.
    /// </summary>
    /// <param name="x1">Start X</param>
    /// <param name="y1">Start Y</param>
    /// <param name="x2">End X</param>
    /// <param name="y2">End Y</param>
    /// <param name="mode">Routing mode: '90' or '45' (default '90')</param>
    /// <param name="direction">Direction: 'auto', 'h', or 'v' (default 'auto')</param>
    /// <returns>Number of tiles placed</returns>
    public int RouteWire(int x1, int y1, int x2, int y2, string mode = "90", string direction = "auto")
    {
        var start = new Vector2Int32(x1, y1);
        var end = new Vector2Int32(x2, y2);
        bool verticalFirst = ParseDirection(direction, start, end);
        bool miter = ParseRoutingMode(mode);

        var path = miter
            ? WireRouter.RouteMiter(start, end, verticalFirst)
            : WireRouter.Route90(start, end, verticalFirst);

        return ApplyRoutedPath(path);
    }

    /// <summary>
    /// Route a bus of parallel wires from (x1,y1) to (x2,y2).
    /// Uses the current draw.setWire() or draw.setTile() configuration.
    /// </summary>
    /// <param name="wireCount">Number of parallel wires (auto-converts to brush size based on spacing)</param>
    /// <param name="x1">Start X</param>
    /// <param name="y1">Start Y</param>
    /// <param name="x2">End X</param>
    /// <param name="y2">End Y</param>
    /// <param name="mode">Routing mode: '90' or '45' (default '90')</param>
    /// <param name="direction">Direction: 'auto', 'h', or 'v' (default 'auto')</param>
    /// <returns>Number of tiles placed</returns>
    public int RouteBus(int wireCount, int x1, int y1, int x2, int y2, string mode = "90", string direction = "auto")
    {
        wireCount = Math.Max(1, wireCount);
        var anchor = new Vector2Int32(x1, y1);
        var cursor = new Vector2Int32(x2, y2);
        bool verticalFirst = ParseDirection(direction, anchor, cursor);
        bool miter = ParseRoutingMode(mode);

        // Convert wire count to brush size: size = (wireCount - 1) * spacing + 1
        int spacing = miter ? 3 : 2;
        int brushSize = (wireCount - 1) * spacing + 1;

        var path = miter
            ? WireRouter.RouteBusMiter(anchor, cursor, brushSize, brushSize, verticalFirst)
            : WireRouter.RouteBus90(anchor, cursor, brushSize, brushSize, verticalFirst);

        return ApplyRoutedPath(path);
    }

    /// <summary>
    /// Get the routed path coordinates without applying them.
    /// Useful for previewing or custom processing.
    /// </summary>
    /// <param name="x1">Start X</param>
    /// <param name="y1">Start Y</param>
    /// <param name="x2">End X</param>
    /// <param name="y2">End Y</param>
    /// <param name="mode">Routing mode: '90' or '45' (default '90')</param>
    /// <param name="direction">Direction: 'auto', 'h', or 'v' (default 'auto')</param>
    /// <returns>List of {x, y} coordinate dictionaries</returns>
    public List<Dictionary<string, int>> RouteWirePath(int x1, int y1, int x2, int y2, string mode = "90", string direction = "auto")
    {
        var start = new Vector2Int32(x1, y1);
        var end = new Vector2Int32(x2, y2);
        bool verticalFirst = ParseDirection(direction, start, end);
        bool miter = ParseRoutingMode(mode);

        var path = miter
            ? WireRouter.RouteMiter(start, end, verticalFirst)
            : WireRouter.Route90(start, end, verticalFirst);

        return path.Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } }).ToList();
    }

    /// <summary>
    /// Get the bus routed path coordinates without applying them.
    /// </summary>
    public List<Dictionary<string, int>> RouteBusPath(int wireCount, int x1, int y1, int x2, int y2, string mode = "90", string direction = "auto")
    {
        wireCount = Math.Max(1, wireCount);
        var anchor = new Vector2Int32(x1, y1);
        var cursor = new Vector2Int32(x2, y2);
        bool verticalFirst = ParseDirection(direction, anchor, cursor);
        bool miter = ParseRoutingMode(mode);

        int spacing = miter ? 3 : 2;
        int brushSize = (wireCount - 1) * spacing + 1;

        var path = miter
            ? WireRouter.RouteBusMiter(anchor, cursor, brushSize, brushSize, verticalFirst)
            : WireRouter.RouteBus90(anchor, cursor, brushSize, brushSize, verticalFirst);

        return path.Select(p => new Dictionary<string, int> { { "x", p.X }, { "y", p.Y } }).ToList();
    }

    private int ApplyRoutedPath(List<Vector2Int32> path)
    {
        ResetCheckTiles();
        _editor.FillSolid(path);
        return path.Count(p => _world.ValidTileLocation(p) && _selection.IsValid(p));
    }

    private static bool ParseDirection(string direction, Vector2Int32 start, Vector2Int32 end)
    {
        return direction?.ToLowerInvariant() switch
        {
            "h" or "horizontal" => false,
            "v" or "vertical" => true,
            _ => WireRouter.DetectVerticalFirst(start, end)
        };
    }

    private static bool ParseRoutingMode(string mode)
    {
        return mode?.ToLowerInvariant() switch
        {
            "45" or "miter" => true,
            _ => false
        };
    }

    // ── Private Helpers ──────────────────────────────────────────────

    private void ResetCheckTiles()
    {
        _editor._checkTiles = new int[_world.TilesWide * _world.TilesHigh];
        if (++_editor._checkTileGeneration <= 0) _editor._checkTileGeneration = 1;
    }

    private BrickStyle? GetAutoSlopeStyle(Vector2Int32 v)
    {
        var t = _world.Tiles[v.X, v.Y];
        var tp = WorldConfiguration.GetTileProperties(t.Type);
        if (!t.IsActive || t.LiquidType != LiquidType.None || tp.IsFramed) return null;

        bool up = _world.SlopeCheck(v, new Vector2Int32(v.X, v.Y - 1));
        bool down = _world.SlopeCheck(v, new Vector2Int32(v.X, v.Y + 1));
        bool upLeft = _world.SlopeCheck(v, new Vector2Int32(v.X - 1, v.Y - 1));
        bool left = _world.SlopeCheck(v, new Vector2Int32(v.X - 1, v.Y));
        bool upRight = _world.SlopeCheck(v, new Vector2Int32(v.X + 1, v.Y - 1));
        bool right = _world.SlopeCheck(v, new Vector2Int32(v.X + 1, v.Y));
        bool downLeft = _world.SlopeCheck(v, new Vector2Int32(v.X - 1, v.Y + 1));
        bool downRight = _world.SlopeCheck(v, new Vector2Int32(v.X + 1, v.Y + 1));

        var mask = new BitsByte(up, upRight, right, downRight, down, downLeft, left, upLeft);
        var maskValue = mask.Value;

        if (maskValue == byte.MinValue || maskValue == byte.MaxValue) return null;
        if (!up && !down) return null;

        if (!up && left && !right && (downRight || !upRight)) return BrickStyle.SlopeTopRight;
        if (!up && right && !left && (downLeft || !upLeft)) return BrickStyle.SlopeTopLeft;
        if (!down && left && !right && (!downRight || upRight)) return BrickStyle.SlopeBottomRight;
        if (!down && right && !left && (!downLeft || upLeft)) return BrickStyle.SlopeBottomLeft;

        return null;
    }

    private bool CheckTileMatch(ref Tile originTile, ref Tile nextTile)
    {
        switch (_editor.TilePicker.PaintMode)
        {
            case PaintMode.TileAndWall:
                if (_editor.TilePicker.TileStyleActive && (originTile.Type != nextTile.Type || originTile.IsActive != nextTile.IsActive))
                    return false;
                if (_editor.TilePicker.WallStyleActive && (originTile.Wall != nextTile.Wall ||
                    (originTile.IsActive && (originTile.Type != nextTile.Type || !nextTile.IsActive)) ||
                    (originTile.Type != nextTile.Type && nextTile.IsActive &&
                     (nextTile.StopsWallsFloodFill() || !WorldConfiguration.GetTileProperties(nextTile.Type).IsFramed))))
                    return false;
                if (_editor.TilePicker.BrickStyleActive && (originTile.BrickStyle != nextTile.BrickStyle))
                    return false;
                if (_editor.TilePicker.TilePaintActive && (originTile.Type != nextTile.Type || originTile.IsActive != nextTile.IsActive))
                    return false;
                if (_editor.TilePicker.WallPaintActive && (originTile.Wall != nextTile.Wall ||
                    (originTile.IsActive && (originTile.Type != nextTile.Type || !nextTile.IsActive)) ||
                    (originTile.Type != nextTile.Type && nextTile.IsActive &&
                     (nextTile.StopsWallsFloodFill() || !WorldConfiguration.GetTileProperties(nextTile.Type).IsFramed))))
                    return false;
                if (_editor.TilePicker.ExtrasActive && (originTile.Actuator != nextTile.Actuator || originTile.InActive != nextTile.InActive || originTile.IsActive != nextTile.IsActive))
                    return false;
                break;
            case PaintMode.Wire:
                if (originTile.Type != nextTile.Type || originTile.IsActive != nextTile.IsActive)
                    return false;
                break;
            case PaintMode.Liquid:
                if ((originTile.LiquidAmount > 0 != nextTile.LiquidAmount > 0) ||
                    originTile.LiquidType != nextTile.LiquidType ||
                    (originTile.IsActive && WorldConfiguration.TileProperties[originTile.Type].IsSolid) ||
                    (nextTile.IsActive && WorldConfiguration.TileProperties[nextTile.Type].IsSolid))
                    return false;
                break;
        }
        return true;
    }

    private void LinearFloodFill(ref int x, ref int y, ref Tile originTile,
        bool[] checkTiles, FloodFillRangeQueue ranges, int bitmapWidth, int bitmapHeight)
    {
        // Walk left
        int lFillLoc = x;
        int tileIndex = bitmapWidth * y + x;
        while (true)
        {
            if (!checkTiles[tileIndex])
            {
                _undo.SaveTile(_world, lFillLoc, y);
                _editor.SetPixel(lFillLoc, y);
                checkTiles[tileIndex] = true;
            }
            lFillLoc--;
            tileIndex--;
            if (lFillLoc <= 0 || checkTiles[tileIndex] ||
                !CheckTileMatch(ref originTile, ref _world.Tiles[lFillLoc, y]) ||
                !_selection.IsValid(lFillLoc, y))
                break;
        }
        lFillLoc++;

        // Walk right
        int rFillLoc = x;
        tileIndex = bitmapWidth * y + x;
        while (true)
        {
            if (!checkTiles[tileIndex])
            {
                _undo.SaveTile(_world, rFillLoc, y);
                _editor.SetPixel(rFillLoc, y);
                checkTiles[tileIndex] = true;
            }
            rFillLoc++;
            tileIndex++;
            if (rFillLoc >= bitmapWidth || checkTiles[tileIndex] ||
                !CheckTileMatch(ref originTile, ref _world.Tiles[rFillLoc, y]) ||
                !_selection.IsValid(rFillLoc, y))
                break;
        }
        rFillLoc--;

        var r = new FloodFillRange(lFillLoc, rFillLoc, y);
        ranges.Enqueue(ref r);
    }

    private static BrickStyle ParseBrickStyle(string style)
    {
        return style?.ToLowerInvariant() switch
        {
            "full" => BrickStyle.Full,
            "halfbrick" or "half" => BrickStyle.HalfBrick,
            "slopetopright" or "topright" => BrickStyle.SlopeTopRight,
            "slopetopleft" or "topleft" => BrickStyle.SlopeTopLeft,
            "slopebottomright" or "bottomright" => BrickStyle.SlopeBottomRight,
            "slopebottomleft" or "bottomleft" => BrickStyle.SlopeBottomLeft,
            _ => BrickStyle.Full
        };
    }

    private static MaskMode ParseMaskMode(string mode)
    {
        return mode?.ToLowerInvariant() switch
        {
            "off" => MaskMode.Off,
            "match" => MaskMode.Match,
            "empty" => MaskMode.Empty,
            "notmatching" => MaskMode.NotMatching,
            _ => MaskMode.Off
        };
    }
}
