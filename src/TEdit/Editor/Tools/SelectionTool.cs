using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.UI;
using TEdit.ViewModel;

namespace TEdit.Editor.Tools;

public class SelectionTool : BaseTool
{
    private Vector2Int32 _startSelection;
    private Vector2Int32 _modifySelection;

    public SelectionTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        _wvm = worldViewModel;
        _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        _preview.Clear();
        _preview.SetPixel(0, 0, 127, 0, 90, 255);

        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/shape_square.png"));
        Name = "Selection";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    public override void MouseDown(TileMouseState e)
    {
        if ((e.LeftButton == MouseButtonState.Pressed) && (Keyboard.IsKeyUp(Key.LeftShift) && Keyboard.IsKeyUp(Key.RightShift)))
            _startSelection = e.Location;
        if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            _modifySelection= e.Location;
        if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Released)
            _wvm.Selection.IsActive = false;
        if (e.RightButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            _wvm.Selection.IsActive = true;
    }

    public override void MouseMove(TileMouseState e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            _wvm.Selection.SetRectangle(_startSelection, e.Location);
    }
}
