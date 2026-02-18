using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Geometry;

namespace TEdit.Editor;

public partial class BrushSettings : ReactiveObject
{
    private int _maxOutline = 10;
    private int _maxHeight = 400;
    private int _maxWidth = 400;
    private int _minOutline = 1;
    private int _minHeight = 1;
    private int _minWidth = 1;
    private int _outline = 1;

    private int _width = 20;
    private int _height = 20;

    [Reactive]
    private int _offsetX = 10;
    [Reactive]
    private int _offsetY = 10;
    [Reactive]
    private bool _isLocked = true;
    [Reactive]
    private bool _isOutline = false;
    private double _rotation = 0;
    private bool _flipHorizontal = false;
    private bool _flipVertical = false;
    [Reactive]
    private bool _isSpray = false;
    private int _sprayDensity = 50;
    private int _sprayTickMs = 30;
    private BrushShape _shape = ToolDefaultData.BrushShape;

    public BrushSettings()
    {
        int width = ToolDefaultData.BrushWidth;
        int height = ToolDefaultData.BrushHeight;
        int outline = ToolDefaultData.BrushOutline;

        IsLocked = (width == height);

        Width = width;
        Height = height;
        Outline = outline;
        Rotation = ToolDefaultData.BrushRotation;
        FlipHorizontal = ToolDefaultData.BrushFlipHorizontal;
        FlipVertical = ToolDefaultData.BrushFlipVertical;
        IsSpray = ToolDefaultData.BrushIsSpray;
        SprayDensity = ToolDefaultData.BrushSprayDensity;
        SprayTickMs = ToolDefaultData.BrushSprayTickMs;
    }

    public event EventHandler BrushChanged;

    protected virtual void OnBrushChanged(object sender, EventArgs e)
    {
        if (BrushChanged != null) BrushChanged(sender, e);
    }

    private void BrushChange()
    {
        OnBrushChanged(this, new EventArgs());
    }

    public int MaxWidth { get { return _maxWidth; } }
    public int MaxHeight { get { return _maxHeight; } }
    public int MinWidth { get { return _minWidth; } }
    public int MinHeight { get { return _minHeight; } }

    public BrushShape Shape
    {
        get { return _shape; }
        set
        {
            this.RaiseAndSetIfChanged(ref _shape, value);
            BrushChange();
        }
    }

    public int MaxOutline
    {
        get { return _maxOutline; }
        private set { this.RaiseAndSetIfChanged(ref _maxOutline, value); }
    }

    public int Height
    {
        get { return _height; }
        set
        {
            if (value < _minHeight)
                value = _minHeight;
            if (value > _maxHeight)
                value = _maxHeight;
            this.RaiseAndSetIfChanged(ref _height, value);
            if (IsLocked)
            {
                _width = Height;
                this.RaisePropertyChanged(nameof(Width));
                OffsetX = _width / 2;
            }
            MaxOutline = Math.Min(Height, Width) / 2;
            OffsetY = _height / 2;
            BrushChange();
        }
    }

    public int Width
    {
        get { return _width; }
        set
        {
            if (value < _minWidth)
                value = _minWidth;
            if (value > _maxWidth)
                value = _maxWidth;
            this.RaiseAndSetIfChanged(ref _width, value);
            if (IsLocked)
            {
                _height = Width;
                this.RaisePropertyChanged(nameof(Height));
                OffsetY = _height / 2;
            }
            MaxOutline = Math.Min(Height, Width) / 2;
            OffsetX = _width / 2;
            BrushChange();
        }
    }

    public int Outline
    {
        get { return _outline; }
        set
        {
            if (value < _minOutline)
                value = _minOutline;
            if (value > _maxOutline)
                value = _maxOutline;
            this.RaiseAndSetIfChanged(ref _outline, value);
        }
    }

    public double Rotation
    {
        get { return _rotation; }
        set
        {
            this.RaiseAndSetIfChanged(ref _rotation, value);
            BrushChange();
        }
    }

    public bool FlipHorizontal
    {
        get { return _flipHorizontal; }
        set
        {
            this.RaiseAndSetIfChanged(ref _flipHorizontal, value);
            BrushChange();
        }
    }

    public bool FlipVertical
    {
        get { return _flipVertical; }
        set
        {
            this.RaiseAndSetIfChanged(ref _flipVertical, value);
            BrushChange();
        }
    }

    public int SprayDensity
    {
        get { return _sprayDensity; }
        set
        {
            if (value < 1) value = 1;
            if (value > 100) value = 100;
            this.RaiseAndSetIfChanged(ref _sprayDensity, value);
        }
    }

    public int SprayTickMs
    {
        get { return _sprayTickMs; }
        set
        {
            if (value < 10) value = 10;
            if (value > 100) value = 100;
            this.RaiseAndSetIfChanged(ref _sprayTickMs, value);
        }
    }

    public bool HasTransform => Math.Abs(Rotation) > 0.01 || FlipHorizontal || FlipVertical;

    public IList<Vector2Int32> GetShapePoints(Vector2Int32 center)
    {
        return GetShapePoints(center, Width, Height);
    }

    public IList<Vector2Int32> GetShapePoints(Vector2Int32 center, int width, int height)
    {
        // Line shapes: transform endpoints, then draw contiguous lines
        if (Shape is BrushShape.Right or BrushShape.Left or BrushShape.Cross)
            return GetLineShapePoints(center, width, height);

        // Polygon shapes: transform vertices, then re-fill for clean edges
        if (HasTransform && Shape is BrushShape.Square or BrushShape.Star or BrushShape.Triangle)
            return GetTransformedPolygonPoints(center, width, height);

        IEnumerable<Vector2Int32> points = Shape switch
        {
            BrushShape.Square => Fill.FillRectangleCentered(center, new Vector2Int32(width, height)),
            BrushShape.Round => Fill.FillEllipseCentered(center, new Vector2Int32(width / 2, height / 2)),
            BrushShape.Star => Fill.FillStarCentered(center, Math.Min(width, height) / 2, 0, 5),
            BrushShape.Triangle => Fill.FillTriangleCentered(center, width / 2, height / 2),
            BrushShape.Crescent => FillCrescent(center, width, height),
            BrushShape.Donut => Fill.FillDonutCentered(center,
                Math.Min(width, height) / 2,
                Math.Max(1, Math.Min(width, height) / 4)),
            _ => Fill.FillRectangleCentered(center, new Vector2Int32(width, height)),
        };

        // Ellipse-based shapes (Round, Crescent, Donut) still use pixel-level inverse mapping
        if (HasTransform)
            points = Fill.ApplyTransform(points, center, Rotation, FlipHorizontal, FlipVertical);

        return points.ToList();
    }

    private IList<Vector2Int32> GetTransformedPolygonPoints(Vector2Int32 center, int width, int height)
    {
        // Get untransformed vertices for the shape
        var vertices = GetShapeVertices(center, width, height);

        // Transform each vertex
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = TransformPoint(vertices[i], center);

        // Re-fill the polygon with transformed vertices
        return Fill.FillPolygon(vertices).ToList();
    }

    private Vector2Int32[] GetShapeVertices(Vector2Int32 center, int width, int height)
    {
        int hw = width / 2, hh = height / 2;

        return Shape switch
        {
            BrushShape.Square => [
                new(center.X - hw, center.Y - hh),
                new(center.X + hw, center.Y - hh),
                new(center.X + hw, center.Y + hh),
                new(center.X - hw, center.Y + hh),
            ],
            BrushShape.Star => GetStarVertices(center, Math.Min(width, height) / 2, 5),
            BrushShape.Triangle => GetTriangleVertices(center, hw),
            _ => [center],
        };
    }

    private static Vector2Int32[] GetStarVertices(Vector2Int32 center, int outerRadius, int numPoints)
    {
        if (outerRadius <= 0 || numPoints < 3) return [center];
        int innerRadius = (int)(outerRadius * Math.Cos(2 * Math.PI / numPoints) / Math.Cos(Math.PI / numPoints));
        var vertices = new Vector2Int32[numPoints * 2];
        double angleStep = Math.PI / numPoints;
        double startAngle = -Math.PI / 2;
        for (int i = 0; i < numPoints * 2; i++)
        {
            double angle = startAngle + i * angleStep;
            int r = (i % 2 == 0) ? outerRadius : innerRadius;
            vertices[i] = new Vector2Int32(
                center.X + (int)Math.Round(Math.Cos(angle) * r),
                center.Y + (int)Math.Round(Math.Sin(angle) * r));
        }
        return vertices;
    }

    private static Vector2Int32[] GetTriangleVertices(Vector2Int32 center, int halfWidth)
    {
        if (halfWidth <= 0) return [center];
        int eqHeight = (int)Math.Round(halfWidth * Math.Sqrt(3));
        int apexY = center.Y - eqHeight * 2 / 3;
        int baseY = center.Y + eqHeight / 3;
        return [
            new(center.X, apexY),
            new(center.X - halfWidth, baseY),
            new(center.X + halfWidth, baseY),
        ];
    }

    private IList<Vector2Int32> GetLineShapePoints(Vector2Int32 center, int width, int height)
    {
        int hw = width / 2, hh = height / 2;

        // Define endpoints for each line shape
        var endpoints = Shape switch
        {
            BrushShape.Right => new[]
            {
                (new Vector2Int32(center.X - hw, center.Y + hh), new Vector2Int32(center.X + hw, center.Y - hh))
            },
            BrushShape.Left => new[]
            {
                (new Vector2Int32(center.X - hw, center.Y - hh), new Vector2Int32(center.X + hw, center.Y + hh))
            },
            BrushShape.Cross => new[]
            {
                (new Vector2Int32(center.X - hw, center.Y - hh), new Vector2Int32(center.X + hw, center.Y + hh)),
                (new Vector2Int32(center.X - hw, center.Y + hh), new Vector2Int32(center.X + hw, center.Y - hh))
            },
            _ => []
        };

        // Transform endpoints, then draw contiguous lines between them
        IEnumerable<Vector2Int32> points = Enumerable.Empty<Vector2Int32>();
        foreach (var (start, end) in endpoints)
        {
            var s = HasTransform ? TransformPoint(start, center) : start;
            var e = HasTransform ? TransformPoint(end, center) : end;
            points = points.Concat(TEdit.Geometry.Shape.DrawLine(s, e));
        }

        return points.ToList();
    }

    private Vector2Int32 TransformPoint(Vector2Int32 point, Vector2Int32 center)
    {
        double dx = point.X - center.X;
        double dy = point.Y - center.Y;

        if (FlipHorizontal) dx = -dx;
        if (FlipVertical) dy = -dy;

        if (Math.Abs(Rotation) > 0.01)
        {
            double rad = Rotation * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            double rx = dx * cos - dy * sin;
            double ry = dx * sin + dy * cos;
            dx = rx;
            dy = ry;
        }

        return new Vector2Int32(
            center.X + (int)Math.Round(dx),
            center.Y + (int)Math.Round(dy));
    }

    /// <summary>
    /// Height = size (outer radius), Width = fullness.
    /// width==height → default crescent, width==height/2 → thin ring, width==height*2 → full circle.
    /// </summary>
    private static IEnumerable<Vector2Int32> FillCrescent(Vector2Int32 center, int width, int height)
    {
        int outerRadius = height / 2;
        if (outerRadius <= 0) return [center];

        double ratio = Math.Clamp((double)width / Math.Max(1, height), 0.5, 2.0);

        int innerRadius, innerOffsetX;

        if (ratio >= 2.0)
        {
            // Full circle
            return Fill.FillEllipseCentered(center, new Vector2Int32(outerRadius, outerRadius));
        }
        else if (ratio >= 1.0)
        {
            // ratio 1.0→2.0: default crescent → full circle
            // t: 0 at ratio=1, 1 at ratio=2
            double t = ratio - 1.0;
            // Default: innerR=0.75R, offset=0.5R → lerp to 0
            innerRadius = Math.Max(1, (int)(outerRadius * 0.75 * (1.0 - t)));
            innerOffsetX = Math.Max(1, (int)(outerRadius * 0.5 * (1.0 - t)));
        }
        else
        {
            // ratio 0.5→1.0: thin ring → default crescent
            // t: 0 at ratio=1, 1 at ratio=0.5
            double t = (1.0 - ratio) / 0.5;
            // Default: innerR=0.75R, offset=0.5R → thin: innerR≈R-1, offset≈1
            innerRadius = Math.Clamp(
                (int)(outerRadius * (0.75 + t * 0.25)),
                1, outerRadius - 1);
            innerOffsetX = Math.Max(1, (int)(outerRadius * 0.5 * (1.0 - t)) + (t > 0.99 ? 0 : 1));
        }

        return Fill.FillCrescentCentered(center, outerRadius, innerRadius, innerOffsetX);
    }
}
