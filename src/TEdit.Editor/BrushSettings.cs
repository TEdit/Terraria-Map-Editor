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
        IEnumerable<Vector2Int32> points = Shape switch
        {
            BrushShape.Square => Fill.FillRectangleCentered(center, new Vector2Int32(width, height)),
            BrushShape.Round => Fill.FillEllipseCentered(center, new Vector2Int32(width / 2, height / 2)),
            BrushShape.Right => TEdit.Geometry.Shape.DrawLine(
                new Vector2Int32(center.X - width / 2, center.Y + height / 2),
                new Vector2Int32(center.X + width / 2, center.Y - height / 2)),
            BrushShape.Left => TEdit.Geometry.Shape.DrawLine(
                new Vector2Int32(center.X - width / 2, center.Y - height / 2),
                new Vector2Int32(center.X + width / 2, center.Y + height / 2)),
            BrushShape.Star => Fill.FillStarCentered(center, Math.Min(width, height) / 2, 0, 5),
            BrushShape.Triangle => Fill.FillTriangleCentered(center, width / 2, height / 2),
            BrushShape.Crescent => FillCrescent(center, width, height),
            BrushShape.Donut => Fill.FillDonutCentered(center,
                Math.Min(width, height) / 2,
                Math.Max(1, Math.Min(width, height) / 4)),
            _ => Fill.FillRectangleCentered(center, new Vector2Int32(width, height)),
        };

        if (HasTransform)
            points = Fill.ApplyTransform(points, center, Rotation, FlipHorizontal, FlipVertical);

        return points.ToList();
    }

    /// <summary>
    /// Height = size (outer radius), Width = fullness.
    /// width==height → default crescent, width==height/2 → thin ring, width==height*2 → full circle.
    /// </summary>
    private static IEnumerable<Vector2Int32> FillCrescent(Vector2Int32 center, int width, int height)
    {
        int outerRadius = height / 2;
        if (outerRadius <= 0) return [center];

        // ratio: 0.5 (thinnest) → 1.0 (default) → 2.0 (full circle)
        double ratio = Math.Clamp((double)width / Math.Max(1, height), 0.5, 2.0);

        // f: 1.0 (thinnest) → 0.0 (full circle). At ratio=1: f = 2/3 (default).
        double f = (2.0 - ratio) / 1.5;

        if (f < 0.001)
        {
            // Full circle
            return Fill.FillEllipseCentered(center, new Vector2Int32(outerRadius, outerRadius));
        }

        // Scale so f=2/3 reproduces the original 0.75R / 0.5R values
        int innerRadius = Math.Clamp((int)(outerRadius * f * 1.125), 1, outerRadius - 1);
        int innerOffsetX = Math.Max(1, (int)(outerRadius * f * 0.75));

        return Fill.FillCrescentCentered(center, outerRadius, innerRadius, innerOffsetX);
    }
}
