using System;
using System.Windows.Input;
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

        if (actions.Contains("editor.draw") || actions.Contains("editor.draw.constrain"))
        {
            _startSelection = e.Location;
            _isDragging = true;
            _isConstraining = actions.Contains("editor.draw.constrain");
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        if (!_isDragging) return;

        var actions = GetActiveActions(e);
        _isConstraining = actions.Contains("editor.draw.constrain");

        var endPoint = e.Location;
        if (_isConstraining)
            endPoint = ConstrainToSquare(_startSelection, endPoint);

        _wvm.Selection.SetRectangle(_startSelection, endPoint);
    }

    public override void MouseUp(TileMouseState e)
    {
        _isDragging = false;
        _isConstraining = false;
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
