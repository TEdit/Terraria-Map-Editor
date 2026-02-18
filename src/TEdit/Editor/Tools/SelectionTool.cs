using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    private enum DragMode { NewSelection, MoveStartPoint, MoveEndPoint }
    private DragMode _dragMode;

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
    }

    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);

        if (actions.Contains("editor.secondary"))
        {
            _wvm.Selection.IsActive = false;
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
        _isDragging = false;
        _isConstraining = false;
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
