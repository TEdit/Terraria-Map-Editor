using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TEdit5.Controls.WorldRenderEngine;
using TEdit5.Controls.WorldRenderEngine.Layers;
using TEdit5.Editor;
using TEdit.Editor;
using TEdit.Geometry;
using TEdit.Terraria;
using static SkiaSharp.SKImageFilter;

namespace TEdit5.Controls;

[TemplatePart("PART_ContentPresenter", typeof(ScrollContentPresenter))]
[TemplatePart("PART_HorizontalScrollBar", typeof(ScrollBar))]
[TemplatePart("PART_VerticalScrollBar", typeof(ScrollBar))]
[TemplatePart("PART_ScrollBarsSeparator", typeof(Panel))]
public class SkiaWorldRenderBox : TemplatedControl, IScrollable
{
    #region BindableBase
    /// <summary>
    ///     Multicast event for property change notifications.
    /// </summary>
    private PropertyChangedEventHandler? _propertyChanged;

    public new event PropertyChangedEventHandler? PropertyChanged
    {
        add { _propertyChanged -= value; _propertyChanged += value; }
        remove => _propertyChanged -= value;
    }


    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        RaisePropertyChanged(propertyName!);
        return true;
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
    }

    /// <summary>
    ///     Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    ///     Name of the property used to notify listeners.  This
    ///     value is optional and can be provided automatically when invoked from compilers
    ///     that support <see cref="CallerMemberNameAttribute" />.
    /// </param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var e = new PropertyChangedEventArgs(propertyName);
        OnPropertyChanged(e);
        _propertyChanged?.Invoke(this, e);
    }
    #endregion
    #region Sub Classes

    /// <summary>
    /// Represents available levels of zoom in an <see cref="SkiaWorldRenderBox"/> control
    /// </summary>
    public class ZoomLevelCollection : IList<int>
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomLevelCollection"/> class.
        /// </summary>
        public ZoomLevelCollection()
        {
            List = new SortedList<int, int>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoomLevelCollection"/> class.
        /// </summary>
        /// <param name="collection">The default values to populate the collection with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <c>collection</c> parameter is null</exception>
        public ZoomLevelCollection(IEnumerable<int> collection)
            : this()
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            AddRange(collection);
        }

        #endregion

        #region Public Class Properties

        /// <summary>
        /// Returns the default zoom levels
        /// </summary>
        public static ZoomLevelCollection Default =>
            new(new[] {
                7, 10, 15, 20, 25, 30, 50, 70, 100, 150, 200, 300, 400, 500, 600, 700, 800, 1200, 1600, 3200, 6400
            });

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ZoomLevelCollection" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ZoomLevelCollection" />.
        /// </returns>
        public int Count => List.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the zoom level at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public int this[int index]
        {
            get => List.Values[index];
            set
            {
                List.RemoveAt(index);
                Add(value);
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the backing list.
        /// </summary>
        protected SortedList<int, int> List { get; set; }

        #endregion

        #region Public Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(int item)
        {
            List.Add(item, item);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="ZoomLevelCollection"/>.
        /// </summary>
        /// <param name="collection">The items to add to the collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <c>collection</c> parameter is null.</exception>
        public void AddRange(IEnumerable<int> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (int value in collection)
            {
                Add(value);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            List.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(int item)
        {
            return List.ContainsKey(item);
        }

        /// <summary>
        /// Copies a range of elements this collection into a destination <see cref="Array"/>.
        /// </summary>
        /// <param name="array">The <see cref="Array"/> that receives the data.</param>
        /// <param name="arrayIndex">A 64-bit integer that represents the index in the <see cref="Array"/> at which storing begins.</param>
        public void CopyTo(int[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = List.Values[i];
            }
        }

        /// <summary>
        /// Finds the index of a zoom level matching or nearest to the specified value.
        /// </summary>
        /// <param name="zoomLevel">The zoom level.</param>
        public int FindNearest(int zoomLevel)
        {
            int nearestValue = List.Values[0];
            int nearestDifference = Math.Abs(nearestValue - zoomLevel);
            for (int i = 1; i < Count; i++)
            {
                int value = List.Values[i];
                int difference = Math.Abs(value - zoomLevel);
                if (difference < nearestDifference)
                {
                    nearestValue = value;
                    nearestDifference = difference;
                }
            }
            return nearestValue;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return List.Values.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public int IndexOf(int item)
        {
            return List.IndexOfKey(item);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.NotImplementedException">Not implemented</exception>
        public void Insert(int index, int item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the next increased zoom level for the given current zoom.
        /// </summary>
        /// <param name="zoomLevel">The current zoom level.</param>
        /// <param name="constrainZoomLevel">When positive, constrain maximum zoom to this value</param>
        /// <returns>The next matching increased zoom level for the given current zoom if applicable, otherwise the nearest zoom.</returns>
        public int NextZoom(int zoomLevel, int constrainZoomLevel = 0)
        {
            var index = IndexOf(FindNearest(zoomLevel));
            if (index < Count - 1) index++;

            return constrainZoomLevel > 0 && this[index] >= constrainZoomLevel ? constrainZoomLevel : this[index];
        }

        /// <summary>
        /// Returns the next decreased zoom level for the given current zoom.
        /// </summary>
        /// <param name="zoomLevel">The current zoom level.</param>
        /// <param name="constrainZoomLevel">When positive, constrain minimum zoom to this value</param>
        /// <returns>The next matching decreased zoom level for the given current zoom if applicable, otherwise the nearest zoom.</returns>
        public int PreviousZoom(int zoomLevel, int constrainZoomLevel = 0)
        {
            var index = IndexOf(FindNearest(zoomLevel));
            if (index > 0) index--;

            return constrainZoomLevel > 0 && this[index] <= constrainZoomLevel ? constrainZoomLevel : this[index];
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(int item)
        {
            return List.Remove(item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="ZoomLevelCollection"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ZoomLevelCollection"/> to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the <see cref="ZoomLevelCollection"/>.</returns>
        public int[] ToArray()
        {
            var results = new int[Count];
            CopyTo(results, 0);

            return results;
        }

        #endregion

        #region IList<int> Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="ZoomLevelCollection" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    #endregion

    #region Enums

    /// <summary>
    /// Determines the sizing mode of an image hosted in an <see cref="SkiaWorldRenderBox" /> control.
    /// </summary>
    public enum SizeModes : byte
    {
        /// <summary>
        /// The image is displayed according to current zoom and scroll properties.
        /// </summary>
        Normal,

        /// <summary>
        /// The image is stretched to fill the client area of the control.
        /// </summary>
        Stretch,

        /// <summary>
        /// The image is stretched to fill as much of the client area of the control as possible, whilst retaining the same aspect ratio for the width and height.
        /// </summary>
        Fit
    }

    [Flags]
    public enum MouseButtons : byte
    {
        None = 0,
        LeftButton = 1,
        MiddleButton = 2,
        RightButton = 4
    }

    /// <summary>
    /// Describes the zoom action occurring
    /// </summary>
    [Flags]
    public enum ZoomActions : byte
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,

        /// <summary>
        /// The control is increasing the zoom.
        /// </summary>
        ZoomIn = 1,

        /// <summary>
        /// The control is decreasing the zoom.
        /// </summary>
        ZoomOut = 2,

        /// <summary>
        /// The control zoom was reset.
        /// </summary>
        ActualSize = 4
    }

    public enum SelectionModes
    {
        /// <summary>
        ///   No selection.
        /// </summary>
        None,

        /// <summary>
        ///   Rectangle selection.
        /// </summary>
        Rectangle,

        /// <summary>
        ///   Zoom selection.
        /// </summary>
        Zoom
    }

    #endregion

    private readonly GlyphRun _noSkia;
    ZoomLevelCollection _zoomLevels = ZoomLevelCollection.Default;
    private Point _startMousePosition;
    private Vector _startScrollPosition;
    private Point _pointerPosition;
    private bool _isPanning;
    private bool _isSelecting;
    private IRasterTileCache _pixelTileCache = new RasterTileCache(0, 0);
    private int _oldZoom = 100;

    static SkiaWorldRenderBox()
    {
        FocusableProperty.OverrideDefaultValue(typeof(SkiaWorldRenderBox), true);
        AffectsRender<SkiaWorldRenderBox>(
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

    private int GetZoomLevel(ZoomActions action)
    {
        var result = action switch
        {
            ZoomActions.None => Zoom,
            ZoomActions.ZoomIn => _zoomLevels.NextZoom(Zoom),
            ZoomActions.ZoomOut => _zoomLevels.PreviousZoom(Zoom),
            ZoomActions.ActualSize => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
        return result;
    }

    private void PerformZoom(ZoomActions action, bool preservePosition, Point relativePoint)
    {
        Point currentPixel = PointToImage(relativePoint);
        int currentZoom = Zoom;
        int newZoom = GetZoomLevel(action);

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

    Brush redBrush = new Avalonia.Media.SolidColorBrush(Colors.Red, 0.5d);

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
            _pixelTileCache,
            Zoom / 100d));

        //context.DrawRectangle()
        var cursorTile = GetScaledRectangle(WorldCoordinate, new(1, 1));
        context.DrawRectangle(Brushes.Aqua, null, cursorTile);

        // draw out of bounds


        var selectionRect = GetScaledRectangle(SelectionRegion);
        context.DrawRectangle(SelectionColor, null, selectionRect);

        if (World != null)
        {
            int borderTop = 41;
            int borderLeft = 41;
            int borderRight = 42;
            int borderBottom = 42;
            int sidebarHeight = World.TilesHigh - borderTop - borderBottom;

            var topRect = GetScaledRectangle(0, 0, World.TilesWide, borderTop);
            var leftRect = GetScaledRectangle(0, borderTop, borderLeft, sidebarHeight);
            var rightRect = GetScaledRectangle(World.TilesWide - borderRight, borderTop, borderRight, sidebarHeight);
            var bottomRect = GetScaledRectangle(0, World.TilesHigh - borderBottom, World.TilesWide, borderBottom);

            context.DrawRectangle(redBrush, null, topRect);
            context.DrawRectangle(redBrush, null, leftRect);
            context.DrawRectangle(redBrush, null, rightRect);
            context.DrawRectangle(redBrush, null, bottomRect);
        }

        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }


    /// <summary>
    /// Defines the <see cref="IMouseTool"/> property.
    /// </summary>
    public static readonly DirectProperty<SkiaWorldRenderBox, IMouseTool?> ActiveToolProperty =
    AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, IMouseTool?>(
        nameof(ActiveTool),
        o => o.ActiveTool,
        (o, v) => o.ActiveTool = v);


    private IMouseTool? _activeTool = null;

    public IMouseTool? ActiveTool
    {
        get => _activeTool;
        set => SetProperty(ref _activeTool, value);
    }

    /// <summary>
    /// Defines the <see cref="World"/> property.
    /// </summary>
    public static readonly DirectProperty<SkiaWorldRenderBox, WorldEditor> WorldEditorProperty =
    AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, WorldEditor>(
        nameof(WorldEditor),
        o => o.WorldEditor,
        (o, v) => o.WorldEditor = v);


    private WorldEditor _worldEditor = null;

    public WorldEditor WorldEditor
    {
        get => _worldEditor;
        set => SetProperty(ref _worldEditor, value);
    }

    /// <summary>
    /// Defines the <see cref="World"/> property.
    /// </summary>
    public static readonly DirectProperty<SkiaWorldRenderBox, World?> WorldProperty =
    AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, World?>(
        nameof(World),
        o => o.World,
        (o, v) => o.World = v);


    private World? _world = null;

    /// <summary>
    /// The Terraria World
    /// </summary>
    public World? World
    {
        get => _world;
        set
        {
            if (_world == value) return;

            // clear the render tile cache when switching worlds
            _world = null;
            var oldCache = _pixelTileCache;

            _world = value;

            _fullRenderCancel.Cancel();
            oldCache?.Dispose();
            _fullRenderCancel.TryReset();

            if (_world == null) return;

            _pixelTileCache = new RasterTileCache(_world.TilesHigh, _world.TilesWide);
            StartFullRender(_fullRenderCancel.Token);

            UpdateViewPort();
            TriggerRender();
        }
    }

    private CancellationTokenSource _fullRenderCancel = new CancellationTokenSource();

    private void StartFullRender(CancellationToken cancel)
    {
        Task.Factory.StartNew(async () =>
        {
            // await _fullRenderLock.WaitAsync();

            try
            {
                int numX = _pixelTileCache.TilesX;
                int numY = _pixelTileCache.TilesY;

                for (int x = 0; x < numX; x++)
                {
                    await Parallel.ForAsync(
                        0,
                        numY,
                        new ParallelOptions { MaxDegreeOfParallelism = 4 },
                        (y, c) =>
                        {
                            if (!c.IsCancellationRequested)
                            {
                                RenderTile(y, x, c);
                            }

                            return ValueTask.CompletedTask;
                        });
                }
            }
            finally
            {
                _fullRenderCancel.TryReset();
            }
        });
    }

    private void RenderTile(int y, int x, CancellationToken cancel)
    {
        if (_world == null) { return; }
        var tile = new RasterTile
        {
            Bitmap = RasterTileRenderer.CreateBitmapTile(_world, x, y, _pixelTileCache.TileSize),
            TileX = x,
            TileY = y,
            PixelX = x * _pixelTileCache.TileSize,
            PixelY = y * _pixelTileCache.TileSize,
            IsDirty = false
        };

        if (!cancel.IsCancellationRequested)
        {
            _pixelTileCache.SetTile(tile, x, y);
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
    public double ScaledWidth => (double)(World?.TilesWide ?? 0d) * (Zoom / 100d);

    /// <summary>
    /// Gets the height of the scaled image.
    /// </summary>
    /// <value>The height of the scaled image.</value>
    public double ScaledHeight => (double)(World?.TilesHigh ?? 0d) * (Zoom / 100d);

    private bool UpdateViewPort()
    {
        if (HorizontalScrollBar == null || VerticalScrollBar == null) return false;

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


    public static readonly DirectProperty<SkiaWorldRenderBox, ZoomLevelCollection> ZoomLevelsProperty =
        AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, ZoomLevelCollection>(
            nameof(ZoomLevels),
            o => o.ZoomLevels,
            (o, v) => o.ZoomLevels = v);

    /// <summary>
    ///   Gets or sets the zoom levels.
    /// </summary>
    /// <value>The zoom levels.</value>
    public ZoomLevelCollection ZoomLevels
    {
        get => _zoomLevels;
        set => SetAndRaise(ZoomLevelsProperty, ref _zoomLevels, value);
    }

    public static readonly StyledProperty<int> MinZoomProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, int>(nameof(MinZoom), 10);

    /// <summary>
    /// Gets or sets the minimum possible zoom.
    /// </summary>
    /// <value>The zoom.</value>
    public int MinZoom
    {
        get => GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public static readonly StyledProperty<int> MaxZoomProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, int>(nameof(MaxZoom), 6400);

    /// <summary>
    /// Gets or sets the maximum possible zoom.
    /// </summary>
    /// <value>The zoom.</value>
    public int MaxZoom
    {
        get => GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    public static readonly StyledProperty<bool> ConstrainZoomOutToFitLevelProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, bool>(nameof(ConstrainZoomOutToFitLevel), true);

    /// <summary>
    /// Gets or sets if the zoom out should constrain to fit image as the lowest zoom level.
    /// </summary>
    public bool ConstrainZoomOutToFitLevel
    {
        get => GetValue(ConstrainZoomOutToFitLevelProperty);
        set => SetValue(ConstrainZoomOutToFitLevelProperty, value);
    }

    public static readonly DirectProperty<SkiaWorldRenderBox, int> OldZoomProperty =
        AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, int>(
            nameof(OldZoom),
            o => o.OldZoom);

    /// <summary>
    /// Gets the previous zoom value
    /// </summary>
    /// <value>The zoom.</value>
    public int OldZoom
    {
        get => _oldZoom;
        private set => SetAndRaise(OldZoomProperty, ref _oldZoom, value);
    }

    /// <summary>
    /// Defines the <see cref="Zoom"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ZoomProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, int>(nameof(ZoomProperty), 100);

    /// <summary>
    /// Zoom level
    /// </summary>
    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set
        {
            var minZoom = MinZoom;
            if (ConstrainZoomOutToFitLevel) minZoom = Math.Max(ZoomLevelToFit, minZoom);
            var newZoom = Math.Clamp(value, minZoom, MaxZoom);

            var previousZoom = Zoom;
            if (previousZoom == newZoom) return;

            SetValue(ZoomProperty, newZoom);
            UpdateViewPort();
            TriggerRender();
        }
    }

    public Size WorldSize => World is null ? default : new Size(World.TilesWide, World.TilesHigh);


    /// <summary>
    /// Gets the zoom to fit level which shows all the image
    /// </summary>
    public int ZoomLevelToFit
    {
        get
        {
            if (World == null) return 100;
            var size = WorldSize;

            double zoom;
            double aspectRatio;

            if (size.Width > size.Height)
            {
                aspectRatio = Viewport.Width / size.Width;
                zoom = aspectRatio * 100.0;

                if (Viewport.Height < size.Height * zoom / 100.0)
                {
                    aspectRatio = Viewport.Height / size.Height;
                    zoom = aspectRatio * 100.0;
                }
            }
            else
            {
                aspectRatio = Viewport.Height / size.Height;
                zoom = aspectRatio * 100.0;

                if (Viewport.Width < size.Width * zoom / 100.0)
                {
                    aspectRatio = Viewport.Width / size.Width;
                    zoom = aspectRatio * 100.0;
                }
            }

            return (int)zoom;
        }
    }

    public static readonly StyledProperty<byte> GridCellSizeProperty =
    AvaloniaProperty.Register<SkiaWorldRenderBox, byte>(nameof(GridCellSize), 15);

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
    public Size Extent => new(
        Math.Max(ViewPort.Bounds.Width, World?.TilesWide ?? 0 * (Zoom / 100d)),
        Math.Max(ViewPort.Bounds.Height, World?.TilesHigh ?? 0 * (Zoom / 100d)));


    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly DirectProperty<SkiaWorldRenderBox, Vector> OffsetProperty =
        AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, Vector>(nameof(Offset), o => o.Offset);


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
    public Size Viewport => this.Bounds.Size;
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
    AvaloniaProperty.Register<SkiaWorldRenderBox, bool>(nameof(AutoCenter), true);

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
        AvaloniaProperty.Register<SkiaWorldRenderBox, ISolidColorBrush>(nameof(GridColor), Brushes.Black);

    /// <summary>
    /// Gets or sets the color used to create the checkerboard style background
    /// </summary>
    public ISolidColorBrush GridColor
    {
        get => GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    public static readonly StyledProperty<ISolidColorBrush> GridColorAlternateProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, ISolidColorBrush>(nameof(GridColorAlternate), Brushes.DarkGray);

    /// <summary>
    /// Gets or sets the color used to create the checkerboard style background
    /// </summary>
    public ISolidColorBrush GridColorAlternate
    {
        get => GetValue(GridColorAlternateProperty);
        set => SetValue(GridColorAlternateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="WorldCoordinate"/> property.
    /// </summary>
    public static readonly DirectProperty<SkiaWorldRenderBox, Point> WorldCoordinateProperty =
        AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, Point>(nameof(WorldCoordinate), o => o.WorldCoordinate);

    private Point _worldCoordinate;

    /// <summary>
    /// Comment
    /// </summary>
    public Point WorldCoordinate
    {
        get => _worldCoordinate;
        private set => SetAndRaise(WorldCoordinateProperty, ref _worldCoordinate, value);
    }

    public static readonly DirectProperty<SkiaWorldRenderBox, bool> IsPanningProperty =
        AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, bool>(
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


    public static readonly DirectProperty<SkiaWorldRenderBox, bool> IsSelectingProperty =
        AvaloniaProperty.RegisterDirect<SkiaWorldRenderBox, bool>(
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
        AvaloniaProperty.Register<SkiaWorldRenderBox, bool>(nameof(AutoPan), true);

    /// <summary>
    /// Gets or sets if the control can pan with the mouse
    /// </summary>
    public bool AutoPan
    {
        get => GetValue(AutoPanProperty);
        set => SetValue(AutoPanProperty, value);
    }

    public static readonly StyledProperty<MouseButtons> PanWithMouseButtonsProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, MouseButtons>(nameof(PanWithMouseButtons), MouseButtons.MiddleButton);

    /// <summary>
    /// Gets or sets the mouse buttons to pan the image
    /// </summary>
    public MouseButtons PanWithMouseButtons
    {
        get => GetValue(PanWithMouseButtonsProperty);
        set => SetValue(PanWithMouseButtonsProperty, value);
    }

    public static readonly StyledProperty<bool> PanWithArrowsProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, bool>(nameof(PanWithArrows), true);

    /// <summary>
    /// Gets or sets if the control can pan with the keyboard arrows
    /// </summary>
    public bool PanWithArrows
    {
        get => GetValue(PanWithArrowsProperty);
        set => SetValue(PanWithArrowsProperty, value);
    }

    public static readonly StyledProperty<MouseButtons> SelectWithMouseButtonsProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, MouseButtons>(nameof(SelectWithMouseButtons), MouseButtons.RightButton);


    /// <summary>
    /// Gets or sets the mouse buttons to select a region on image
    /// </summary>
    public MouseButtons SelectWithMouseButtons
    {
        get => GetValue(SelectWithMouseButtonsProperty);
        set => SetValue(SelectWithMouseButtonsProperty, value);
    }


    public static readonly StyledProperty<SelectionModes> SelectionModeProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, SelectionModes>(nameof(SelectionMode), SelectionModes.None);

    public SelectionModes SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly StyledProperty<ISolidColorBrush> SelectionColorProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, ISolidColorBrush>(nameof(SelectionColor), new SolidColorBrush(new Color(127, 0, 128, 255)));

    public ISolidColorBrush SelectionColor
    {
        get => GetValue(SelectionColorProperty);
        set => SetValue(SelectionColorProperty, value);
    }

    public static readonly StyledProperty<Rect> SelectionRegionProperty =
        AvaloniaProperty.Register<SkiaWorldRenderBox, Rect>(nameof(SelectionRegion));

    public static readonly StyledProperty<bool> InvertMousePanProperty =
    AvaloniaProperty.Register<SkiaWorldRenderBox, bool>(nameof(InvertMousePan), false);

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

        var pointer = e.GetCurrentPoint(this);
        ActiveTool?.Release(WorldEditor, pointer, WorldCoordinate);

        IsPanning = false;
        IsSelecting = false;
    }

    private void ViewPortOnPointerExited(object? sender, PointerEventArgs e)
    {
        _pointerPosition = new Point(-1, -1);
        var pointer = e.GetCurrentPoint(this);
        ActiveTool?.LeaveWindow(WorldEditor, pointer, WorldCoordinate);


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

        if (ActiveTool != null)
        {
            ActiveTool?.Press(WorldEditor, pointer, WorldCoordinate);
            _pixelTileCache.SetPixelDirty((int)WorldCoordinate.X, (int)WorldCoordinate.Y);
        }

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
        _pointerPosition = pointer.Position;
        (double wcX, double wcY) = PointToImage(_pointerPosition, true);
        WorldCoordinate = new Point((int)wcX, (int)wcY); // cast to int

        if (ActiveTool != null)
        {
            ActiveTool?.Move(WorldEditor, pointer, WorldCoordinate);
            _pixelTileCache.SetPixelDirty((int)WorldCoordinate.X, (int)WorldCoordinate.Y);
        }

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
            x = (point.X + Offset.X - viewport.X) / (Zoom / 100d);
            y = (point.Y + Offset.Y - viewport.Y) / (Zoom / 100d);

            var size = World?.Size ?? Vector2Int32.One;
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
        var zoom = (Zoom / 100d);
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
        var viewPortSize = Viewport;

        double xOffset = HorizontalScrollBar.Value;
        double yOffset = VerticalScrollBar.Value;

        if (AutoCenter)
        {
            xOffset = (!IsHorizontalBarVisible ? (viewPortSize.Width - ScaledWidth) / 2 : 0);
            yOffset = (!IsVerticalBarVisible ? (viewPortSize.Height - ScaledHeight) / 2 : 0);
        }

        return new(source.X * (Zoom / 100d) - xOffset, source.Y * (Zoom / 100d) - yOffset);
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
        var viewPortSize = Viewport;
        double xOffset = HorizontalScrollBar.Value;
        double yOffset = VerticalScrollBar.Value;

        if (AutoCenter)
        {
            xOffset = (!IsHorizontalBarVisible ? -(viewPortSize.Width - ScaledWidth) / 2 : xOffset);
            yOffset = (!IsVerticalBarVisible ? -(viewPortSize.Height - ScaledHeight) / 2 : yOffset);
        }

        return new(
            source.Left * (Zoom / 100d) - xOffset,
            source.Top * (Zoom / 100d) - yOffset,
            source.Width * (Zoom / 100d),
            source.Height * (Zoom / 100d));
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
        return new(source.Width * (Zoom / 100d), source.Height * (Zoom / 100d));
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
