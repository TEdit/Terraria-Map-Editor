using System.Windows.Input;
using TEdit.Geometry;
using TEdit.UI.Xaml.XnaContentHost;

namespace TEdit.UI;

public class TileMouseState
{
    public MouseButtonState LeftButton { get; set; }
    public MouseButtonState RightButton { get; set; }
    public MouseButtonState MiddleButton { get; set; }

    public Vector2Int32 Location { get; set; }
    public int WheelDelta { get; set; }

    public static TileMouseState FromHwndMouseEventArgs(HwndMouseEventArgs e, Vector2Int32 tile)
    {
        return new TileMouseState
        {
            LeftButton = e.LeftButton,
            RightButton = e.RightButton,
            MiddleButton = e.MiddleButton,
            Location = tile
        };
    }
}
