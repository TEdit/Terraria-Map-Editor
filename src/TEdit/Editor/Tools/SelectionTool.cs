using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Editor;
using TEdit.Geometry;
using TEdit.UI;
using TEdit.ViewModel;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public class SelectionTool : BaseTool
{
    private Vector2Int32 _startSelection;
    private bool _isDragging;
    private bool _isConstraining;

    private enum DragMode { NewSelection, MoveStartPoint, MoveEndPoint, Brush, Lasso }
    private DragMode _dragMode;

    // Brush / lasso: Ctrl+drag removes tiles instead of adding them.
    private bool _isSubtracting;
    private Vector2Int32 _lastBrushPoint;
    private readonly List<Vector2Int32> _lassoPath = new();
    private readonly List<Vector2Int32> _lassoOutline = new();

    public SelectionTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        _wvm = worldViewModel;
        _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        _preview.Clear();
        _preview.SetPixel(0, 0, 127, 0, 90, 255);

        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/shape_square.png"));
        SymbolIcon = SymbolRegular.SelectObject24;
        Name = "Selection";
        IsActive = false;
        ToolType = ToolType.Pixel;

        _wvm.Selection.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Selection.Mode) && IsActive)
                _wvm.PreviewChange();
        };
    }

    public override IReadOnlyList<Vector2Int32> CadPreviewPath => _lassoOutline;
    public override bool HasCadPreview => _isDragging && _dragMode == DragMode.Lasso && _lassoOutline.Count > 0;

    public override WriteableBitmap PreviewTool()
    {
        if (_wvm.Selection.Mode != SelectionShape.Brush)
        {
            PreviewOffsetX = -1;
            PreviewOffsetY = -1;
            return base.PreviewTool();
        }

        // Show the brush footprint, reusing the brush tool's preview.
        var brushTool = _wvm.Tools.OfType<BrushTool>().FirstOrDefault();
        if (brushTool == null)
            return base.PreviewTool();

        var preview = brushTool.PreviewTool();
        PreviewOffsetX = brushTool.PreviewOffsetX;
        PreviewOffsetY = brushTool.PreviewOffsetY;
        return preview;
    }

    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);

        if (actions.Contains("editor.secondary"))
        {
            _wvm.Selection.IsActive = false;
            return;
        }

        if (_wvm.Selection.Mode != SelectionShape.Rectangle)
        {
            if (!actions.Contains("editor.draw") && !actions.Contains("selection.adjust.startpoint"))
                return;

            // Ctrl+drag shares the binding used to move the rectangle's start point.
            _isSubtracting = actions.Contains("selection.adjust.startpoint");
            _isDragging = true;

            if (_wvm.Selection.Mode == SelectionShape.Brush)
            {
                _dragMode = DragMode.Brush;
                _lastBrushPoint = e.Location;
                ApplyBrush(_wvm.Brush.GetShapePoints(e.Location));
            }
            else
            {
                _dragMode = DragMode.Lasso;
                _lassoPath.Clear();
                _lassoOutline.Clear();
                AddLassoPoint(e.Location);
            }
            return;
        }

        // Ctrl+click: adjust start point (top-left corner)
        if (actions.Contains("selection.adjust.startpoint") && _wvm.Selection.IsActive)
        {
            _isDragging = true;
            _dragMode = DragMode.MoveStartPoint;
            AdjustStartPoint(e.Location);
            return;
        }

        // Shift+click: adjust end point (bottom-right corner)
        if (actions.Contains("selection.adjust.endpoint") && _wvm.Selection.IsActive)
        {
            _isDragging = true;
            _dragMode = DragMode.MoveEndPoint;
            AdjustEndPoint(e.Location);
            return;
        }

        if (actions.Contains("editor.draw") || actions.Contains("editor.draw.constrain"))
        {
            _startSelection = e.Location;
            _isDragging = true;
            _dragMode = DragMode.NewSelection;
            _isConstraining = actions.Contains("editor.draw.constrain");
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        if (!_isDragging) return;

        switch (_dragMode)
        {
            case DragMode.MoveStartPoint:
                AdjustStartPoint(e.Location);
                break;
            case DragMode.MoveEndPoint:
                AdjustEndPoint(e.Location);
                break;
            case DragMode.Brush:
                // Stamp along the segment so fast mouse moves leave no gaps.
                var stroke = new HashSet<Vector2Int32>();
                foreach (var p in Shape.DrawLineTool(_lastBrushPoint, e.Location))
                    stroke.UnionWith(_wvm.Brush.GetShapePoints(p));
                _lastBrushPoint = e.Location;
                ApplyBrush(stroke);
                break;
            case DragMode.Lasso:
                AddLassoPoint(e.Location);
                break;
            case DragMode.NewSelection:
                var actions = GetActiveActions(e);
                _isConstraining = actions.Contains("editor.draw.constrain");
                var endPoint = e.Location;
                if (_isConstraining)
                    endPoint = ConstrainToSquare(_startSelection, endPoint);
                _wvm.Selection.SetRectangle(_startSelection, endPoint);
                break;
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        if (_isDragging && _dragMode == DragMode.Lasso)
        {
            AddLassoPoint(e.Location);
            if (_lassoPath.Count >= 3)
                _wvm.Selection.SetLasso(_lassoPath, !_isSubtracting, WorldBounds());
            _lassoPath.Clear();
            _lassoOutline.Clear();
        }

        // Never leave half a chest or table selected: copying it would paste a broken sprite.
        if (_isDragging && _dragMode is DragMode.Brush or DragMode.Lasso)
            _wvm.Selection.SnapToSprites(_wvm.CurrentWorld, !_isSubtracting);

        _isDragging = false;
        _isConstraining = false;
    }

    private void ApplyBrush(IEnumerable<Vector2Int32> points)
    {
        _wvm.Selection.SetTiles(points, !_isSubtracting, WorldBounds());
    }

    private void AddLassoPoint(Vector2Int32 location)
    {
        if (_lassoPath.Count > 0)
        {
            var last = _lassoPath[_lassoPath.Count - 1];
            if (last == location)
                return;
            _lassoOutline.AddRange(Shape.DrawLineTool(last, location));
        }
        else
        {
            _lassoOutline.Add(location);
        }
        _lassoPath.Add(location);
    }

    private RectangleInt32? WorldBounds()
    {
        var world = _wvm.CurrentWorld;
        return world == null ? null : new RectangleInt32(0, 0, world.TilesWide, world.TilesHigh);
    }

    private void AdjustStartPoint(Vector2Int32 location)
    {
        var area = _wvm.Selection.SelectionArea;
        int endX = area.X + area.Width - 1;
        int endY = area.Y + area.Height - 1;

        // Move start point, keep end point fixed
        int newX = Math.Min(location.X, endX);
        int newY = Math.Min(location.Y, endY);
        int newW = endX - newX + 1;
        int newH = endY - newY + 1;

        _wvm.Selection.SelectionArea = new RectangleInt32(newX, newY, Math.Max(1, newW), Math.Max(1, newH));
    }

    private void AdjustEndPoint(Vector2Int32 location)
    {
        var area = _wvm.Selection.SelectionArea;

        // Move end point, keep start point fixed
        int newW = location.X - area.X + 1;
        int newH = location.Y - area.Y + 1;

        _wvm.Selection.SelectionArea = new RectangleInt32(
            area.X, area.Y,
            Math.Max(1, newW),
            Math.Max(1, newH));
    }

    private static Vector2Int32 ConstrainToSquare(Vector2Int32 start, Vector2Int32 end)
    {
        int dx = end.X - start.X;
        int dy = end.Y - start.Y;
        int side = Math.Max(Math.Abs(dx), Math.Abs(dy));
        return new Vector2Int32(
            start.X + side * Math.Sign(dx),
            start.Y + side * Math.Sign(dy));
    }
}
