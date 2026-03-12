using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

namespace TEdit.View.Popups;

public partial class SpriteSheetEditorWindow : FluentWindow
{
    private SpriteSheetEditorViewModel? _vm;

    // Drag-select state
    private bool _isDragging;
    private Point _dragStartPixel;
    private int _dragStartCol, _dragStartRow;
    private int _dragEndCol, _dragEndRow;

    // Named rects that should not be removed during overlay redraw
    private static readonly string[] CanvasFixedRects =
        ["CheckerboardRect", "ImageBorderRect", "SelectionRect"];

    public SpriteSheetEditorWindow(SpriteSheetEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _vm = viewModel;

        // Set the texture image and size canvas elements to match
        if (viewModel.TileTexture != null)
        {
            int texW = viewModel.TileTexture.PixelWidth;
            int texH = viewModel.TileTexture.PixelHeight;

            TextureImage.Source = viewModel.TileTexture;
            TextureCanvas.Width = texW;
            TextureCanvas.Height = texH;

            // Checkerboard background sized to texture
            CheckerboardRect.Width = texW;
            CheckerboardRect.Height = texH;

            // Image border sized to texture
            ImageBorderRect.Width = texW;
            ImageBorderRect.Height = texH;
            ImageBorderRect.StrokeThickness = 1.0 / viewModel.ZoomLevel;
        }

        // Subscribe to frame collection changes to redraw overlays
        viewModel.Frames.CollectionChanged += Frames_CollectionChanged;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Initial draw
        Loaded += (_, _) => RedrawFrameOverlays();
    }

    private void Frames_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RedrawFrameOverlays();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SpriteSheetEditorViewModel.SelectedFrame)
            or nameof(SpriteSheetEditorViewModel.ZoomLevel))
        {
            RedrawFrameOverlays();

            // Update border/selection stroke thickness on zoom change
            if (e.PropertyName == nameof(SpriteSheetEditorViewModel.ZoomLevel) && _vm != null)
            {
                ImageBorderRect.StrokeThickness = 1.0 / _vm.ZoomLevel;
                SelectionRect.StrokeThickness = 1.0 / _vm.ZoomLevel;
            }
        }
    }

    private void RedrawFrameOverlays()
    {
        if (_vm == null) return;

        // Remove existing dynamic elements (keep named/fixed elements and the Image)
        for (int i = TextureCanvas.Children.Count - 1; i >= 0; i--)
        {
            var child = TextureCanvas.Children[i];
            if (child is Line)
            {
                TextureCanvas.Children.RemoveAt(i);
            }
            else if (child is Rectangle rect)
            {
                string? name = rect.Name;
                if (string.IsNullOrEmpty(name) || !IsFixedRect(name))
                    TextureCanvas.Children.RemoveAt(i);
            }
        }

        // Draw tile grid lines (above texture, below frame overlays)
        // Tile edge = light grey, gap/padding edge = darker/dimmer line
        if (_vm.TileTexture != null)
        {
            int texW = _vm.TileTexture.PixelWidth;
            int texH = _vm.TileTexture.PixelHeight;
            int gridX = _vm.TextureGridX;
            int gridY = _vm.TextureGridY;
            int gapX = _vm.FrameGapX;
            int gapY = _vm.FrameGapY;
            int intervalX = gridX + gapX;
            int intervalY = gridY + gapY;
            double thickness = 1.0 / _vm.ZoomLevel;

            var tileBrush = new SolidColorBrush(Color.FromArgb(90, 200, 200, 200));
            tileBrush.Freeze();
            var gapBrush = new SolidColorBrush(Color.FromArgb(60, 255, 140, 50));
            gapBrush.Freeze();

            // Vertical lines
            if (intervalX > 0)
            {
                for (int x = 0; x <= texW; x += intervalX)
                {
                    // Tile leading edge
                    AddGridLine(x, 0, x, texH, tileBrush, thickness);

                    // Gap leading edge (end of tile cell)
                    if (gapX > 0 && x + gridX <= texW)
                        AddGridLine(x + gridX, 0, x + gridX, texH, gapBrush, thickness);
                }
            }

            // Horizontal lines
            if (intervalY > 0)
            {
                for (int y = 0; y <= texH; y += intervalY)
                {
                    // Tile leading edge
                    AddGridLine(0, y, texW, y, tileBrush, thickness);

                    // Gap leading edge (end of tile cell)
                    if (gapY > 0 && y + gridY <= texH)
                        AddGridLine(0, y + gridY, texW, y + gridY, gapBrush, thickness);
                }
            }
        }

        // Draw frame rectangles
        foreach (var frame in _vm.Frames)
        {
            bool isSelected = frame == _vm.SelectedFrame;

            var rect = new Rectangle
            {
                Width = frame.PixelWidth,
                Height = frame.PixelHeight,
                Stroke = isSelected ? Brushes.Yellow : Brushes.Red,
                StrokeThickness = isSelected ? 2.0 / _vm.ZoomLevel : 1.0 / _vm.ZoomLevel,
                Fill = isSelected
                    ? new SolidColorBrush(Color.FromArgb(40, 255, 255, 0))
                    : new SolidColorBrush(Color.FromArgb(20, 255, 0, 0)),
                IsHitTestVisible = false,
            };

            Canvas.SetLeft(rect, frame.PixelX);
            Canvas.SetTop(rect, frame.PixelY);
            TextureCanvas.Children.Add(rect);
        }

        // Update hint visibility
        NoFrameHint.Visibility = _vm.SelectedFrame == null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void AddGridLine(double x1, double y1, double x2, double y2, Brush stroke, double thickness)
    {
        var line = new Line
        {
            X1 = x1, Y1 = y1,
            X2 = x2, Y2 = y2,
            Stroke = stroke,
            StrokeThickness = thickness,
            IsHitTestVisible = false,
        };
        TextureCanvas.Children.Add(line);
    }

    private static bool IsFixedRect(string name)
    {
        foreach (var n in CanvasFixedRects)
        {
            if (n == name) return true;
        }
        return false;
    }

    #region Mouse event handlers

    private void TextureCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_vm == null) return;

        var pos = e.GetPosition(TextureCanvas);
        int px = (int)pos.X;
        int py = (int)pos.Y;

        // Calculate tile col/row
        int intervalX = _vm.TextureGridX + _vm.FrameGapX;
        int intervalY = _vm.TextureGridY + _vm.FrameGapY;
        int col = intervalX > 0 ? px / intervalX : 0;
        int row = intervalY > 0 ? py / intervalY : 0;

        MouseCoordsText.Text = $"Pixel: ({px}, {py})  Tile: [{col}, {row}]";

        // Update drag selection
        if (_isDragging)
        {
            _dragEndCol = col;
            _dragEndRow = row;
            UpdateSelectionRect();
        }
    }

    private void TextureCanvas_MouseLeave(object sender, MouseEventArgs e)
    {
        MouseCoordsText.Text = string.Empty;
    }

    private void TextureCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm == null) return;

        var pos = e.GetPosition(TextureCanvas);
        int px = (int)pos.X;
        int py = (int)pos.Y;

        int intervalX = _vm.TextureGridX + _vm.FrameGapX;
        int intervalY = _vm.TextureGridY + _vm.FrameGapY;

        _dragStartCol = intervalX > 0 ? px / intervalX : 0;
        _dragStartRow = intervalY > 0 ? py / intervalY : 0;
        _dragEndCol = _dragStartCol;
        _dragEndRow = _dragStartRow;
        _dragStartPixel = pos;
        _isDragging = true;

        // Hide selection and button until drag completes
        SelectionRect.Visibility = Visibility.Collapsed;
        AddSelectionButton.Visibility = Visibility.Collapsed;

        TextureCanvas.CaptureMouse();
    }

    private void TextureCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_vm == null) return;

        TextureCanvas.ReleaseMouseCapture();

        if (!_isDragging) return;
        _isDragging = false;

        var pos = e.GetPosition(TextureCanvas);
        double dx = Math.Abs(pos.X - _dragStartPixel.X);
        double dy = Math.Abs(pos.Y - _dragStartPixel.Y);

        // If drag distance < 3px, treat as click (select frame)
        if (dx < 3 && dy < 3)
        {
            SelectionRect.Visibility = Visibility.Collapsed;
            AddSelectionButton.Visibility = Visibility.Collapsed;
            var frame = _vm.GetFrameAtPixel(pos.X, pos.Y);
            _vm.SelectedFrame = frame;
            return;
        }

        // Finalize drag selection
        UpdateSelectionRect();
        SelectionRect.Visibility = Visibility.Visible;
        AddSelectionButton.Visibility = Visibility.Visible;
    }

    private void UpdateSelectionRect()
    {
        if (_vm == null) return;

        int minCol = Math.Min(_dragStartCol, _dragEndCol);
        int maxCol = Math.Max(_dragStartCol, _dragEndCol);
        int minRow = Math.Min(_dragStartRow, _dragEndRow);
        int maxRow = Math.Max(_dragStartRow, _dragEndRow);

        int intervalX = _vm.TextureGridX + _vm.FrameGapX;
        int intervalY = _vm.TextureGridY + _vm.FrameGapY;

        int numCols = maxCol - minCol + 1;
        int numRows = maxRow - minRow + 1;

        int x = minCol * intervalX;
        int y = minRow * intervalY;
        int w = numCols * _vm.TextureGridX + (numCols - 1) * _vm.FrameGapX;
        int h = numRows * _vm.TextureGridY + (numRows - 1) * _vm.FrameGapY;

        Canvas.SetLeft(SelectionRect, x);
        Canvas.SetTop(SelectionRect, y);
        SelectionRect.Width = Math.Max(w, 1);
        SelectionRect.Height = Math.Max(h, 1);
        SelectionRect.StrokeThickness = 1.0 / _vm.ZoomLevel;
        SelectionRect.Visibility = Visibility.Visible;
    }

    #endregion

    private void AddSelection_Click(object sender, RoutedEventArgs e)
    {
        if (_vm == null) return;

        int minCol = Math.Min(_dragStartCol, _dragEndCol);
        int maxCol = Math.Max(_dragStartCol, _dragEndCol);
        int minRow = Math.Min(_dragStartRow, _dragEndRow);
        int maxRow = Math.Max(_dragStartRow, _dragEndRow);

        int intervalX = _vm.TextureGridX + _vm.FrameGapX;
        int intervalY = _vm.TextureGridY + _vm.FrameGapY;

        int numCols = maxCol - minCol + 1;
        int numRows = maxRow - minRow + 1;

        short uvX = (short)(minCol * intervalX);
        short uvY = (short)(minRow * intervalY);
        short sizeX = (short)numCols;
        short sizeY = (short)numRows;
        int pixelW = numCols * _vm.TextureGridX + (numCols - 1) * _vm.FrameGapX;
        int pixelH = numRows * _vm.TextureGridY + (numRows - 1) * _vm.FrameGapY;

        _vm.AddFrameFromSelection(uvX, uvY, sizeX, sizeY, pixelW, pixelH);

        // Hide selection after adding
        SelectionRect.Visibility = Visibility.Collapsed;
        AddSelectionButton.Visibility = Visibility.Collapsed;
    }

    private void TextureScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_vm == null) return;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (e.Delta > 0)
                _vm.ZoomLevel = Math.Min(_vm.ZoomLevel * 1.5, 16.0);
            else
                _vm.ZoomLevel = Math.Max(_vm.ZoomLevel / 1.5, 0.25);

            e.Handled = true;
        }
    }

    private void RegenerateGrid_Click(object sender, RoutedEventArgs e)
    {
        _vm?.RegenerateFramesFromGrid();
    }

    private void SingleFrame_Click(object sender, RoutedEventArgs e)
    {
        _vm?.SetSingleFrame();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void RemoveFrame_Click(object sender, RoutedEventArgs e)
    {
        if (_vm == null) return;
        if (sender is FrameworkElement fe && fe.Tag is EditableFrame frame)
        {
            if (frame == _vm.SelectedFrame)
                _vm.SelectedFrame = null;
            _vm.Frames.Remove(frame);
            _vm.StatusText = $"Removed frame ({_vm.Frames.Count} remaining)";
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_vm != null)
        {
            _vm.Frames.CollectionChanged -= Frames_CollectionChanged;
            _vm.PropertyChanged -= ViewModel_PropertyChanged;
        }
        base.OnClosed(e);
    }
}
