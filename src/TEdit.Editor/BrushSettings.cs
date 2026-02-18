using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

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
    [Reactive]
    private double _rotation = 0;
    [Reactive]
    private bool _flipHorizontal = false;
    [Reactive]
    private bool _flipVertical = false;
    [Reactive]
    private bool _isSpray = false;
    private int _sprayDensity = 50;
    private int _sprayTickMs = 100;
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
            if (value < 50) value = 50;
            if (value > 500) value = 500;
            this.RaiseAndSetIfChanged(ref _sprayTickMs, value);
        }
    }

    public bool HasTransform => Math.Abs(Rotation) > 0.01 || FlipHorizontal || FlipVertical;
}
