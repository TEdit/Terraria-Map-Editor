using System;
using System.Collections.Generic;
using ReactiveUI;
using TEdit.Geometry;
using TEdit.Terraria;

namespace TEdit.Editor;

public enum SelectionShape
{
    Rectangle,
    Brush,
    Lasso,
}

public partial class Selection : ReactiveObject, ISelection
{
    private RectangleInt32 _selectionArea = new RectangleInt32(0, 0, 0, 0);
    private bool _isActive;
    private SelectionShape _mode = SelectionShape.Rectangle;

    // Per-tile mask over SelectionArea (row-major). Null means the whole rectangle is selected.
    private bool[] _mask;

    public RectangleInt32 SelectionArea
    {
        get => _selectionArea;
        set
        {
            // Moving keeps a free-form mask; resizing turns the selection back into a rectangle.
            if (value.Width != _selectionArea.Width || value.Height != _selectionArea.Height)
                _mask = null;
            this.RaiseAndSetIfChanged(ref _selectionArea, value);
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (!value)
                _mask = null;
            this.RaiseAndSetIfChanged(ref _isActive, value);
        }
    }

    /// <summary>How the selection tool picks tiles.</summary>
    public SelectionShape Mode
    {
        get => _mode;
        set => this.RaiseAndSetIfChanged(ref _mode, value);
    }

    /// <summary>True when the selection is a free-form shape (brush / lasso) instead of a rectangle.</summary>
    public bool HasMask => _mask != null;

    public bool IsValid(Vector2Int32 p)
    {
       return IsValid(p.X, p.Y);
    }
    public bool IsValid(int x, int y)
    {
        if (!IsActive)
            return true;

        if (!SelectionArea.Contains(x, y))
            return false;

        return _mask == null || _mask[(y - _selectionArea.Y) * _selectionArea.Width + (x - _selectionArea.X)];
    }

    public void SetRectangle(Vector2Int32 p1, Vector2Int32 p2)
    {
        int x1 = p1.X < p2.X ? p1.X : p2.X;
        int y1 = p1.Y < p2.Y ? p1.Y : p2.Y;
        int width = Math.Abs(p2.X - p1.X) + 1;
        int height = Math.Abs(p2.Y - p1.Y) + 1;

        _mask = null;
        SelectionArea = new RectangleInt32(x1, y1, width, height);
        IsActive = true;
    }

    /// <summary>
    /// Adds tiles to (or removes them from) the selection, turning it into a free-form mask.
    /// Adding to an inactive selection starts a new one. Points outside <paramref name="clip"/> are ignored.
    /// </summary>
    public void SetTiles(IEnumerable<Vector2Int32> points, bool selected, RectangleInt32? clip = null)
    {
        bool hasOld = IsActive && _selectionArea.Width > 0 && _selectionArea.Height > 0;
        if (!hasOld && !selected)
            return;

        var pointList = new List<Vector2Int32>();
        int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
        foreach (var p in points)
        {
            if (clip.HasValue && !clip.Value.Contains(p.X, p.Y))
                continue;
            pointList.Add(p);
            minX = Math.Min(minX, p.X);
            minY = Math.Min(minY, p.Y);
            maxX = Math.Max(maxX, p.X);
            maxY = Math.Max(maxY, p.Y);
        }
        if (pointList.Count == 0)
            return;

        var area = _selectionArea;
        if (selected)
        {
            var bounds = new RectangleInt32(minX, minY, maxX - minX + 1, maxY - minY + 1);
            area = hasOld ? RectangleInt32.Union(_selectionArea, bounds) : bounds;
        }

        bool[] mask;
        if (hasOld && _mask != null && area == _selectionArea)
        {
            // Same bounds: edit the existing mask in place (the common case while dragging inside the selection).
            mask = _mask;
        }
        else
        {
            mask = new bool[area.Width * area.Height];
            if (hasOld)
            {
                int offsetX = _selectionArea.X - area.X;
                for (int y = 0; y < _selectionArea.Height; y++)
                {
                    int dst = (y + _selectionArea.Y - area.Y) * area.Width + offsetX;
                    if (_mask != null)
                        Array.Copy(_mask, y * _selectionArea.Width, mask, dst, _selectionArea.Width);
                    else
                        Array.Fill(mask, true, dst, _selectionArea.Width);
                }
            }
        }

        foreach (var p in pointList)
        {
            if (area.Contains(p.X, p.Y))
                mask[(p.Y - area.Y) * area.Width + (p.X - area.X)] = selected;
        }

        if (!selected && Array.IndexOf(mask, true) < 0)
        {
            IsActive = false;
            return;
        }

        // Set both fields before notifying: the property setter would drop the mask on resize.
        _selectionArea = area;
        _mask = mask;
        this.RaisePropertyChanged(nameof(SelectionArea));
        IsActive = true;
    }

    /// <summary>
    /// Makes a free-form selection cover multi-tile sprites (chests, tables, paintings...) whole:
    /// partially selected sprites are fully added (<paramref name="include"/>) or fully removed.
    /// Rectangle selections are left alone.
    /// </summary>
    public void SnapToSprites(World world, bool include)
    {
        if (world == null || !IsActive || _mask == null)
            return;

        var area = _selectionArea;
        var seen = new HashSet<Vector2Int32>();
        var changes = new List<Vector2Int32>();

        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                if (!IsValid(x, y))
                    continue;

                var bounds = SpritePlacer.GetSpriteBounds(world, x, y);
                if (bounds == null || !seen.Add(bounds.Value.Location))
                    continue;

                var sprite = bounds.Value;
                bool partial = false;
                for (int sy = sprite.Top; sy < sprite.Bottom && !partial; sy++)
                    for (int sx = sprite.Left; sx < sprite.Right && !partial; sx++)
                        partial = !IsValid(sx, sy);

                if (!partial)
                    continue;

                for (int sy = sprite.Top; sy < sprite.Bottom; sy++)
                    for (int sx = sprite.Left; sx < sprite.Right; sx++)
                        changes.Add(new Vector2Int32(sx, sy));
            }
        }

        if (changes.Count > 0)
            SetTiles(changes, include, new RectangleInt32(0, 0, world.TilesWide, world.TilesHigh));
    }

    /// <summary>
    /// Adds (or removes) the area enclosed by a closed lasso path, including the path itself.
    /// </summary>
    public void SetLasso(IList<Vector2Int32> path, bool selected, RectangleInt32? clip = null)
    {
        if (path == null || path.Count == 0)
            return;

        var points = new List<Vector2Int32>(Fill.FillPolygon(path));
        for (int i = 0; i < path.Count; i++)
            points.AddRange(Shape.DrawLineTool(path[i], path[(i + 1) % path.Count]));

        SetTiles(points, selected, clip);
    }
}
