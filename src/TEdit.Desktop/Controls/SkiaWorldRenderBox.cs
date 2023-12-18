using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using TEdit.Desktop.Controls.WorldRenderEngine;
using TEdit.Desktop.Controls.WorldRenderEngine.Layers;
using TEdit.Geometry;
using TEdit.Terraria;
using static TEdit.Desktop.Controls.AdvancedImageBox;

namespace TEdit.Desktop.Controls;

[TemplatePart("PART_ContentPresenter", typeof(ScrollContentPresenter))]
[TemplatePart("PART_HorizontalScrollBar", typeof(ScrollBar))]
[TemplatePart("PART_VerticalScrollBar", typeof(ScrollBar))]
[TemplatePart("PART_ScrollBarsSeparator", typeof(Panel))]
public class SkiaWorldRenderBox : TemplatedControl
{
    private readonly GlyphRun _noSkia;

    private Point _startMousePosition;
    private Vector _startScrollPosition;
    private Point _pointerPosition;
    private bool _isPanning;
    private bool _isSelecting;

    static SkiaWorldRenderBox()
    {
        FocusableProperty.OverrideDefaultValue(typeof(AdvancedImageBox), true);
        AffectsRender<AdvancedImageBox>(
            GridCellSizeProperty,
            GridColorProperty,
            GridColorAlternateProperty,
            WorldProperty);
    }

    public SkiaWorldRenderBox()
    {
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.None);
        ClipToBounds = true;

        // "No Skia" text for unsupported platforms
        var text = "Current rendering API is not Skia";
        var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
        _noSkia = new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ViewPort is not null)
        {
            ViewPort.PointerPressed -= ViewPortOnPointerPressed;
            ViewPort.PointerExited -= ViewPortOnPointerExited;
            ViewPort.PointerMoved -= ViewPortOnPointerMoved;
            ViewPort.PointerWheelChanged -= ViewPortOnPointerWheelChanged;
            HorizontalScrollBar.Scroll -= ScrollBarOnScroll;
            VerticalScrollBar.Scroll -= ScrollBarOnScroll;
        }

        ViewPort = e.NameScope.Find<ScrollContentPresenter>("PART_ContentPresenter")!;
        HorizontalScrollBar = e.NameScope.Find<ScrollBar>("PART_HorizontalScrollBar")!;
        VerticalScrollBar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollBar")!;

        ViewPort.PointerPressed += ViewPortOnPointerPressed;
        ViewPort.PointerExited += ViewPortOnPointerExited;
        ViewPort.PointerMoved += ViewPortOnPointerMoved;
        ViewPort.PointerWheelChanged += ViewPortOnPointerWheelChanged;
        HorizontalScrollBar.Scroll += ScrollBarOnScroll;
        VerticalScrollBar.Scroll += ScrollBarOnScroll;
    }

    private void ViewPortOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        //throw new NotImplementedException();
        ProcessMouseZoom(e.Delta.Y > 0, e.GetPosition(ViewPort));
    }

    private void ProcessMouseZoom(bool isZoomIn, Point cursorPosition)
    => PerformZoom(isZoomIn ? ZoomActions.ZoomIn : ZoomActions.ZoomOut, true, cursorPosition);

    private void PerformZoom(ZoomActions action, bool preservePosition, Point relativePoint)
    {
        Point currentPixel = PointToImage(relativePoint);
        float currentZoom = Zoom;
        float newZoom = action == ZoomActions.ZoomIn ? currentZoom * 1.1f : currentZoom / 1.1f;

        /*if (preservePosition && Zoom != currentZoom)
            CanRender = false;*/

        Zoom = newZoom;

        if (preservePosition && Zoom != currentZoom)
        {
            ScrollTo(currentPixel, relativePoint);
        }
    }

    public void TriggerRender(bool renderOnlyCursorTracker = false)
    {
        if (renderOnlyCursorTracker) return;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var imageViewPort = GetImageViewPort();


        var viewport = Bounds;
        context.Custom(new NoSkiaCustomDrawOp(viewport, _noSkia));
        context.Custom(new BackgroundGridCustomDrawOp(viewport));

        context.Custom(new WorldPixelsCustomDrawOp(
            imageViewPort,
            Offset,
            World,
            PixelTileCache,
            Zoom));

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }


    /// <summary>
    /// Defines the <see cref="World"/> property.
    /// </summary>
    public static readonly StyledProperty<World?> WorldProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, World?>(nameof(World));

    /// <summary>
    /// The Terraria World
    /// </summary>
    public World? World
    {
        get { return GetValue(WorldProperty); }
        set
        {
            SetValue(WorldProperty, value);
            PixelTileCache?.Clear();
            UpdateViewPort();
            TriggerRender();
        }
    }

    /// <summary>
    /// Gets the size of the scaled image.
    /// </summary>
    /// <value>The size of the scaled image.</value>
    public Size ScaledImageSize => new(ScaledWidth, ScaledHeight);

    /// <summary>
    /// Gets the width of the scaled image.
    /// </summary>
    /// <value>The width of the scaled image.</value>
    public double ScaledWidth => (double)(World?.TilesWide ?? 0d) * Zoom;

    /// <summary>
    /// Gets the height of the scaled image.
    /// </summary>
    /// <value>The height of the scaled image.</value>
    public double ScaledHeight => (double)(World?.TilesHigh ?? 0d) * Zoom;

    private bool UpdateViewPort()
    {
        if (World == null)
        {
            HorizontalScrollBar.Maximum = 0;
            VerticalScrollBar.Maximum = 0;
            return true;
        }

        var scaledImageWidth = ScaledWidth;
        var scaledImageHeight = ScaledHeight;
        var width = scaledImageWidth - HorizontalScrollBar.ViewportSize;
        var height = scaledImageHeight - VerticalScrollBar.ViewportSize;

        bool changed = false;
        if (Math.Abs(HorizontalScrollBar.Maximum - width) > 0.01)
        {
            HorizontalScrollBar.Maximum = width;
            changed = true;
        }

        if (Math.Abs(VerticalScrollBar.Maximum - scaledImageHeight) > 0.01)
        {
            VerticalScrollBar.Maximum = height;
            changed = true;
        }

        return changed;
    }



    /// <summary>
    /// Defines the <see cref="Zoom"/> property.
    /// </summary>
    public static readonly StyledProperty<float> ZoomProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, float>(nameof(Zoom), 1.0f);

    /// <summary>
    /// Zoom level
    /// </summary>
    public float Zoom
    {
        get { return GetValue(ZoomProperty); }
        set
        {
            SetValue(ZoomProperty, value);
            UpdateViewPort();
            TriggerRender();
        }
    }

    /// <summary>
    /// Defines the <see cref="PixelTileCache"/> property.
    /// </summary>
    public static readonly StyledProperty<IRasterTileCache> PixelTileCacheProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, IRasterTileCache>(nameof(PixelTileCache), null);

    /// <summary>
    /// Comment
    /// </summary>
    public IRasterTileCache PixelTileCache
    {
        get { return GetValue(PixelTileCacheProperty); }
        set { SetValue(PixelTileCacheProperty, value); }
    }

    public static readonly StyledProperty<byte> GridCellSizeProperty =
    AvaloniaProperty.Register<AdvancedImageBox, byte>(nameof(GridCellSize), 15);

    /// <summary>
    /// Gets the image view port.
    /// </summary>
    /// <returns></returns>
    public Rect GetImageViewPort()
    {
        var viewPortSize = Viewport;
        if (World == null || viewPortSize is { Width: 0, Height: 0 }) return default;

        double xOffset = 0;
        double yOffset = 0;
        double width = Math.Min(ScaledWidth - Math.Abs(Offset.X), viewPortSize.Width);
        double height = Math.Min(ScaledHeight - Math.Abs(Offset.Y), viewPortSize.Height);

        if (AutoCenter)
        {
            xOffset = (!IsHorizontalBarVisible ? (viewPortSize.Width - ScaledWidth) / 2 : 0);
            yOffset = (!IsVerticalBarVisible ? (viewPortSize.Height - ScaledHeight) / 2 : 0);
        }

        return new(xOffset, yOffset, width, height);
    }

    #region UI Controls
    protected internal ScrollContentPresenter ViewPort = null!;
    protected internal ScrollBar HorizontalScrollBar = null!;
    protected internal ScrollBar VerticalScrollBar = null!;

    /// <inheritdoc />
    public Size Extent => new(Math.Max(ViewPort.Bounds.Width, World?.TilesWide ?? 0 * Zoom), Math.Max(ViewPort.Bounds.Height, World?.TilesHigh ?? 0 * Zoom));

    /// <inheritdoc />
    public Vector Offset
    {
        get => new(HorizontalScrollBar.Value, VerticalScrollBar.Value);
        set
        {
            HorizontalScrollBar.Value = value.X;
            VerticalScrollBar.Value = value.Y;
            TriggerRender();
        }
    }

    /// <inheritdoc />
    public Size Viewport => ViewPort.Bounds.Size;
    #endregion

    #region Properties

    public bool IsHorizontalBarVisible
    {
        get
        {
            if (World is null) return false;
            return ScaledWidth > Viewport.Width;
        }
    }

    public bool IsVerticalBarVisible
    {
        get
        {
            if (World is null) return false;
            return ScaledHeight > Viewport.Height;
        }
    }

    public static readonly StyledProperty<bool> AutoCenterProperty =
    AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(AutoCenter), true);

    /// <summary>
    /// Gets or sets if image is auto centered
    /// </summary>
    public bool AutoCenter
    {
        get => GetValue(AutoCenterProperty);
        set => SetValue(AutoCenterProperty, value);
    }

    /// <summary>
    /// Gets or sets the grid cell size
    /// </summary>
    public byte GridCellSize
    {
        get => GetValue(GridCellSizeProperty);
        set => SetValue(GridCellSizeProperty, value);
    }

    public static readonly StyledProperty<ISolidColorBrush> GridColorProperty =
        AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(GridColor), Brushes.Black);

    /// <summary>
    /// Gets or sets the color used to create the checkerboard style background
    /// </summary>
    public ISolidColorBrush GridColor
    {
        get => GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    public static readonly StyledProperty<ISolidColorBrush> GridColorAlternateProperty =
        AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(GridColorAlternate), Brushes.DarkGray);

    /// <summary>
    /// Gets or sets the color used to create the checkerboard style background
    /// </summary>
    public ISolidColorBrush GridColorAlternate
    {
        get => GetValue(GridColorAlternateProperty);
        set => SetValue(GridColorAlternateProperty, value);
    }

    public static readonly DirectProperty<AdvancedImageBox, Point> PointerPositionProperty =
       AvaloniaProperty.RegisterDirect<AdvancedImageBox, Point>(
           nameof(PointerPosition),
           o => o.PointerPosition);




    /// <summary>
    /// Gets the current pointer position
    /// </summary>
    public Point PointerPosition
    {
        get => _pointerPosition;
        private set => SetAndRaise(PointerPositionProperty, ref _pointerPosition, value);
    }

    public static readonly DirectProperty<AdvancedImageBox, bool> IsPanningProperty =
        AvaloniaProperty.RegisterDirect<AdvancedImageBox, bool>(
            nameof(IsPanning),
            o => o.IsPanning);

    /// <summary>
    /// Gets if control is currently panning
    /// </summary>
    public bool IsPanning
    {
        get => _isPanning;
        protected set
        {
            if (!SetAndRaise(IsPanningProperty, ref _isPanning, value)) return;
            _startScrollPosition = Offset;

            if (value)
            {
                Cursor = new Cursor(StandardCursorType.SizeAll);
                //this.OnPanStart(EventArgs.Empty);
            }
            else
            {
                Cursor = Cursor.Default;
                //this.OnPanEnd(EventArgs.Empty);
            }
        }
    }


    public static readonly DirectProperty<AdvancedImageBox, bool> IsSelectingProperty =
        AvaloniaProperty.RegisterDirect<AdvancedImageBox, bool>(
            nameof(IsSelecting),
            o => o.IsSelecting);

    /// <summary>
    /// Gets if control is currently selecting a ROI
    /// </summary>
    public bool IsSelecting
    {
        get => _isSelecting;
        protected set => SetAndRaise(IsSelectingProperty, ref _isSelecting, value);
    }

    /// <summary>
    /// Gets the center point of the viewport
    /// </summary>
    public Point CenterPoint
    {
        get
        {
            var viewport = GetImageViewPort();
            return new(viewport.Width / 2, viewport.Height / 2);
        }
    }

    public static readonly StyledProperty<bool> AutoPanProperty =
        AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(AutoPan), true);

    /// <summary>
    /// Gets or sets if the control can pan with the mouse
    /// </summary>
    public bool AutoPan
    {
        get => GetValue(AutoPanProperty);
        set => SetValue(AutoPanProperty, value);
    }

    public static readonly StyledProperty<MouseButtons> PanWithMouseButtonsProperty =
        AvaloniaProperty.Register<AdvancedImageBox, MouseButtons>(nameof(PanWithMouseButtons), MouseButtons.LeftButton | MouseButtons.MiddleButton | MouseButtons.RightButton);

    /// <summary>
    /// Gets or sets the mouse buttons to pan the image
    /// </summary>
    public MouseButtons PanWithMouseButtons
    {
        get => GetValue(PanWithMouseButtonsProperty);
        set => SetValue(PanWithMouseButtonsProperty, value);
    }

    public static readonly StyledProperty<bool> PanWithArrowsProperty =
        AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(PanWithArrows), true);

    /// <summary>
    /// Gets or sets if the control can pan with the keyboard arrows
    /// </summary>
    public bool PanWithArrows
    {
        get => GetValue(PanWithArrowsProperty);
        set => SetValue(PanWithArrowsProperty, value);
    }

    public static readonly StyledProperty<MouseButtons> SelectWithMouseButtonsProperty =
        AvaloniaProperty.Register<AdvancedImageBox, MouseButtons>(nameof(SelectWithMouseButtons), MouseButtons.LeftButton | MouseButtons.RightButton);


    /// <summary>
    /// Gets or sets the mouse buttons to select a region on image
    /// </summary>
    public MouseButtons SelectWithMouseButtons
    {
        get => GetValue(SelectWithMouseButtonsProperty);
        set => SetValue(SelectWithMouseButtonsProperty, value);
    }


    public static readonly StyledProperty<SelectionModes> SelectionModeProperty =
        AvaloniaProperty.Register<AdvancedImageBox, SelectionModes>(nameof(SelectionMode), SelectionModes.None);

    public SelectionModes SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly StyledProperty<ISolidColorBrush> SelectionColorProperty =
        AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(SelectionColor), new SolidColorBrush(new Color(127, 0, 128, 255)));

    public ISolidColorBrush SelectionColor
    {
        get => GetValue(SelectionColorProperty);
        set => SetValue(SelectionColorProperty, value);
    }

    public static readonly StyledProperty<Rect> SelectionRegionProperty =
        AvaloniaProperty.Register<AdvancedImageBox, Rect>(nameof(SelectionRegion));

    public static readonly StyledProperty<bool> InvertMousePanProperty =
    AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(InvertMousePan), false);

    /// <summary>
    /// Gets or sets if mouse pan is inverted
    /// </summary>
    public bool InvertMousePan
    {
        get => GetValue(InvertMousePanProperty);
        set => SetValue(InvertMousePanProperty, value);
    }

    public Rect SelectionRegion
    {
        get => GetValue(SelectionRegionProperty);
        set
        {
            SetValue(SelectionRegionProperty, value);
            //if (!RaiseAndSetIfChanged(ref _selectionRegion, value)) return;
            TriggerRender();
            //RaisePropertyChanged(nameof(HaveSelection));
            //RaisePropertyChanged(nameof(SelectionRegionNet));
            //RaisePropertyChanged(nameof(SelectionPixelSize));
        }
    }

    public Rect SelectionRegionNet
    {
        get
        {
            var rect = SelectionRegion;
            return new Rect((int)Math.Ceiling(rect.X), (int)Math.Ceiling(rect.Y),
                (int)rect.Width, (int)rect.Height);
        }
    }

    public PixelSize SelectionPixelSize
    {
        get
        {
            var rect = SelectionRegion;
            return new PixelSize((int)rect.Width, (int)rect.Height);
        }
    }

    public bool HaveSelection => SelectionRegion != default;
    #endregion
    #region Event Handlers

    private void ScrollBarOnScroll(object? sender, ScrollEventArgs e)
    {
        TriggerRender();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Handled) return;

        IsPanning = false;
        IsSelecting = false;
    }

    private void ViewPortOnPointerExited(object? sender, PointerEventArgs e)
    {
        PointerPosition = new Point(-1, -1);
        TriggerRender(true);
        e.Handled = true;
    }

    private void ViewPortOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled
            || _isPanning
            || _isSelecting
            || World is null) { return; }

        var pointer = e.GetCurrentPoint(this);

        if (SelectionMode != SelectionModes.None)
        {
            if (!(
                    pointer.Properties.IsLeftButtonPressed && (SelectWithMouseButtons & MouseButtons.LeftButton) != 0 ||
                    pointer.Properties.IsMiddleButtonPressed && (SelectWithMouseButtons & MouseButtons.MiddleButton) != 0 ||
                    pointer.Properties.IsRightButtonPressed && (SelectWithMouseButtons & MouseButtons.RightButton) != 0
                )
               ) { return; }
            IsSelecting = true;
        }
        else
        {
            if (!(
                    pointer.Properties.IsLeftButtonPressed && (PanWithMouseButtons & MouseButtons.LeftButton) != 0 ||
                    pointer.Properties.IsMiddleButtonPressed && (PanWithMouseButtons & MouseButtons.MiddleButton) != 0 ||
                    pointer.Properties.IsRightButtonPressed && (PanWithMouseButtons & MouseButtons.RightButton) != 0
                ) || !AutoPan
               )
            {
                return;
            }

            IsPanning = true;
        }

        var location = pointer.Position;

        if (location.X > Viewport.Width) return;
        if (location.Y > Viewport.Height) return;
        _startMousePosition = location;
    }

    private void ViewPortOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Handled) return;

        var pointer = e.GetCurrentPoint(ViewPort);
        PointerPosition = pointer.Position;

        if (!_isPanning && !_isSelecting)
        {
            TriggerRender(true);
            return;
        }

        if (_isPanning)
        {
            double x;
            double y;

            if (!InvertMousePan)
            {
                x = _startScrollPosition.X + (_startMousePosition.X - _pointerPosition.X);
                y = _startScrollPosition.Y + (_startMousePosition.Y - _pointerPosition.Y);
            }
            else
            {
                x = (_startScrollPosition.X - (_startMousePosition.X - _pointerPosition.X));
                y = (_startScrollPosition.Y - (_startMousePosition.Y - _pointerPosition.Y));
            }

            Offset = new Vector(x, y);
        }
        else if (_isSelecting)
        {
            var viewPortPoint = new Point(
                Math.Min(_pointerPosition.X, ViewPort.Bounds.Right),
                Math.Min(_pointerPosition.Y, ViewPort.Bounds.Bottom));

            double x;
            double y;
            double w;
            double h;

            var imageOffset = GetImageViewPort().Position;

            if (viewPortPoint.X < _startMousePosition.X)
            {
                x = viewPortPoint.X;
                w = _startMousePosition.X - viewPortPoint.X;
            }
            else
            {
                x = _startMousePosition.X;
                w = viewPortPoint.X - _startMousePosition.X;
            }

            if (viewPortPoint.Y < _startMousePosition.Y)
            {
                y = viewPortPoint.Y;
                h = _startMousePosition.Y - viewPortPoint.Y;
            }
            else
            {
                y = _startMousePosition.Y;
                h = viewPortPoint.Y - _startMousePosition.Y;
            }

            x -= imageOffset.X - Offset.X;
            y -= imageOffset.Y - Offset.Y;

            var zoom = Zoom;
            x /= zoom;
            y /= zoom;
            w /= zoom;
            h /= zoom;

            if (w > 0 && h > 0)
            {
                SelectionRegion = FitRectangle(new Rect(x, y, w, h));
            }
        }

        e.Handled = true;
    }



    #endregion

    #region Utility methods
    /// <summary>
    ///   Determines whether the specified point is located within the image view port
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>
    ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
    /// </returns>
    public bool IsPointInImage(Point point)
        => GetImageViewPort().Contains(point);

    /// <summary>
    ///   Determines whether the specified point is located within the image view port
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to check.</param>
    /// <param name="y">The Y co-ordinate of the point to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
    /// </returns>
    public bool IsPointInImage(int x, int y)
        => IsPointInImage(new Point(x, y));

    /// <summary>
    ///   Determines whether the specified point is located within the image view port
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to check.</param>
    /// <param name="y">The Y co-ordinate of the point to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
    /// </returns>
    public bool IsPointInImage(double x, double y)
        => IsPointInImage(new Point(x, y));

    /// <summary>
    ///   Converts the given client size point to represent a coordinate on the source image.
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to convert.</param>
    /// <param name="y">The Y co-ordinate of the point to convert.</param>
    /// <param name="fitToBounds">
    ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
    /// </param>
    /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
    public Point PointToImage(double x, double y, bool fitToBounds = true)
        => PointToImage(new Point(x, y), fitToBounds);

    /// <summary>
    ///   Converts the given client size point to represent a coordinate on the source image.
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to convert.</param>
    /// <param name="y">The Y co-ordinate of the point to convert.</param>
    /// <param name="fitToBounds">
    ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
    /// </param>
    /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
    public Point PointToImage(int x, int y, bool fitToBounds = true)
    {
        return PointToImage(new Point(x, y), fitToBounds);
    }

    /// <summary>
    ///   Converts the given client size point to represent a coordinate on the source image.
    /// </summary>
    /// <param name="point">The source point.</param>
    /// <param name="fitToBounds">
    ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
    /// </param>
    /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
    public Point PointToImage(Point point, bool fitToBounds = true)
    {
        double x;
        double y;

        var viewport = GetImageViewPort();

        if (!fitToBounds || viewport.Contains(point))
        {
            x = (point.X + Offset.X - viewport.X) / Zoom;
            y = (point.Y + Offset.Y - viewport.Y) / Zoom;

            var size = World?.Size ?? Vector2Int32.Zero;
            if (fitToBounds)
            {
                x = Math.Clamp(x, 0, size.X - 1);
                y = Math.Clamp(y, 0, size.Y - 1);
            }
        }
        else
        {
            x = 0; // Return Point.Empty if we couldn't match
            y = 0;
        }

        return new(x, y);
    }

    /// <summary>
    ///   Returns the source <see cref="T:System.Drawing.Point" /> repositioned to include the current image offset and scaled by the current zoom level
    /// </summary>
    /// <param name="source">The source <see cref="Point"/> to offset.</param>
    /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
    public Point GetOffsetPoint(System.Drawing.Point source)
    {
        var offset = GetOffsetPoint(new Point(source.X, source.Y));

        return new((int)offset.X, (int)offset.Y);
    }

    /// <summary>
    ///   Returns the source co-ordinates repositioned to include the current image offset and scaled by the current zoom level
    /// </summary>
    /// <param name="x">The source X co-ordinate.</param>
    /// <param name="y">The source Y co-ordinate.</param>
    /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
    public Point GetOffsetPoint(int x, int y)
    {
        return GetOffsetPoint(new System.Drawing.Point(x, y));
    }

    /// <summary>
    ///   Returns the source co-ordinates repositioned to include the current image offset and scaled by the current zoom level
    /// </summary>
    /// <param name="x">The source X co-ordinate.</param>
    /// <param name="y">The source Y co-ordinate.</param>
    /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
    public Point GetOffsetPoint(double x, double y)
    {
        return GetOffsetPoint(new Point(x, y));
    }

    /// <summary>
    ///   Returns the source <see cref="T:System.Drawing.PointF" /> repositioned to include the current image offset and scaled by the current zoom level
    /// </summary>
    /// <param name="source">The source <see cref="PointF"/> to offset.</param>
    /// <returns>A <see cref="PointF"/> which has been repositioned to match the current zoom level and image offset</returns>
    public Point GetOffsetPoint(Point source)
    {
        Rect viewport = GetImageViewPort();
        var scaled = GetScaledPoint(source);
        var offsetX = viewport.Left + Offset.X;
        var offsetY = viewport.Top + Offset.Y;

        return new(scaled.X + offsetX, scaled.Y + offsetY);
    }


    /// <summary>
    ///   Fits a given <see cref="T:System.Drawing.RectangleF" /> to match image boundaries
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    ///   A <see cref="T:System.Drawing.RectangleF" /> structure remapped to fit the image boundaries
    /// </returns>
    public Rect FitRectangle(Rect rectangle)
    {
        var size = World.Size;

        var x = rectangle.X;
        var y = rectangle.Y;
        var w = rectangle.Width;
        var h = rectangle.Height;

        if (x < 0)
        {
            w -= -x;
            x = 0;
        }

        if (y < 0)
        {
            h -= -y;
            y = 0;
        }

        if (x + w > size.X)
        {
            w = size.X - x;
        }

        if (y + h > size.Y)
        {
            h = size.Y - y;
        }

        return new(x, y, w, h);
    }
    #endregion

    #region Navigate / Scroll methods
    /// <summary>
    ///   Scrolls the control to the given point in the image, offset at the specified display point
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to scroll to.</param>
    /// <param name="y">The Y co-ordinate of the point to scroll to.</param>
    /// <param name="relativeX">The X co-ordinate relative to the <c>x</c> parameter.</param>
    /// <param name="relativeY">The Y co-ordinate relative to the <c>y</c> parameter.</param>
    public void ScrollTo(double x, double y, double relativeX, double relativeY)
        => ScrollTo(new Point(x, y), new Point(relativeX, relativeY));

    /// <summary>
    ///   Scrolls the control to the given point in the image, offset at the specified display point
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to scroll to.</param>
    /// <param name="y">The Y co-ordinate of the point to scroll to.</param>
    /// <param name="relativeX">The X co-ordinate relative to the <c>x</c> parameter.</param>
    /// <param name="relativeY">The Y co-ordinate relative to the <c>y</c> parameter.</param>
    public void ScrollTo(int x, int y, int relativeX, int relativeY)
        => ScrollTo(new Point(x, y), new Point(relativeX, relativeY));

    /// <summary>
    ///   Scrolls the control to the given point in the image, offset at the specified display point
    /// </summary>
    /// <param name="imageLocation">The point of the image to attempt to scroll to.</param>
    /// <param name="relativeDisplayPoint">The relative display point to offset scrolling by.</param>
    public void ScrollTo(Point imageLocation, Point relativeDisplayPoint)
    {
        //CanRender = false;
        var zoom = Zoom;
        var x = imageLocation.X * zoom - relativeDisplayPoint.X;
        var y = imageLocation.Y * zoom - relativeDisplayPoint.Y;

        Offset = new Vector(x, y);
    }

    /// <summary>
    ///   Centers the given point in the image in the center of the control
    /// </summary>
    /// <param name="imageLocation">The point of the image to attempt to center.</param>
    public void CenterAt(System.Drawing.Point imageLocation)
        => ScrollTo(new Point(imageLocation.X, imageLocation.Y), new Point(Viewport.Width / 2, Viewport.Height / 2));

    /// <summary>
    ///   Centers the given point in the image in the center of the control
    /// </summary>
    /// <param name="imageLocation">The point of the image to attempt to center.</param>
    public void CenterAt(Point imageLocation)
        => ScrollTo(imageLocation, new Point(Viewport.Width / 2, Viewport.Height / 2));

    /// <summary>
    ///   Centers the given point in the image in the center of the control
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to center.</param>
    /// <param name="y">The Y co-ordinate of the point to center.</param>
    public void CenterAt(int x, int y)
        => CenterAt(new Point(x, y));

    /// <summary>
    ///   Centers the given point in the image in the center of the control
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to center.</param>
    /// <param name="y">The Y co-ordinate of the point to center.</param>
    public void CenterAt(double x, double y)
        => CenterAt(new Point(x, y));

    /// <summary>
    /// Resets the viewport to show the center of the image.
    /// </summary>
    public void CenterToImage()
    {
        Offset = new Vector(HorizontalScrollBar.Maximum / 2, VerticalScrollBar.Maximum / 2);
    }
    #endregion

    #region Selection / ROI methods

    /// <summary>
    ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
    /// </summary>
    /// <param name="x">The X co-ordinate of the point to scale.</param>
    /// <param name="y">The Y co-ordinate of the point to scale.</param>
    /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
    public Point GetScaledPoint(int x, int y)
    {
        return GetScaledPoint(new Point(x, y));
    }

    /// <summary>
    ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
    /// </summary>
    /// <param name="source">The source <see cref="Point"/> to scale.</param>
    /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
    public Point GetScaledPoint(Point source)
    {
        return new(source.X * Zoom, source.Y * Zoom);
    }

    /// <summary>
    ///   Returns the source rectangle scaled according to the current zoom level
    /// </summary>
    /// <param name="x">The X co-ordinate of the source rectangle.</param>
    /// <param name="y">The Y co-ordinate of the source rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
    public Rect GetScaledRectangle(int x, int y, int width, int height)
    {
        return GetScaledRectangle(new Rect(x, y, width, height));
    }


    /// <summary>
    ///   Returns the source rectangle scaled according to the current zoom level
    /// </summary>
    /// <param name="location">The location of the source rectangle.</param>
    /// <param name="size">The size of the source rectangle.</param>
    /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
    public Rect GetScaledRectangle(Point location, Size size)
    {
        return GetScaledRectangle(new Rect(location, size));
    }


    /// <summary>
    ///   Returns the source <see cref="T:System.Drawing.Rectangle" /> scaled according to the current zoom level
    /// </summary>
    /// <param name="source">The source <see cref="Rectangle"/> to scale.</param>
    /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
    public Rect GetScaledRectangle(Rect source)
    {
        return new(source.Left * Zoom, source.Top * Zoom, source.Width * Zoom, source.Height * Zoom);
    }

    /// <summary>
    ///   Returns the source size scaled according to the current zoom level
    /// </summary>
    /// <param name="width">The width of the size to scale.</param>
    /// <param name="height">The height of the size to scale.</param>
    /// <returns>A <see cref="Size"/> which has been resized to match the current zoom level</returns>
    public Size GetScaledSize(int width, int height)
    {
        return GetScaledSize(new Size(width, height));
    }

    /// <summary>
    ///   Returns the source <see cref="T:System.Drawing.Size" /> scaled according to the current zoom level
    /// </summary>
    /// <param name="source">The source <see cref="Size"/> to scale.</param>
    /// <returns>A <see cref="Size"/> which has been resized to match the current zoom level</returns>
    public Size GetScaledSize(Size source)
    {
        return new(source.Width * Zoom, source.Height * Zoom);
    }

    /// <summary>
    ///   Creates a selection region which encompasses the entire image
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown if no image is currently set</exception>
    public void SelectAll()
    {
        var size = World.Size;
        SelectionRegion = new Rect(0, 0, size.X, size.Y);
    }

    /// <summary>
    /// Clears any existing selection region
    /// </summary>
    public void SelectNone()
    {
        SelectionRegion = default;
    }

    #endregion
}
