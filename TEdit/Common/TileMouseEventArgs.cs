using System.Windows.Input;
using TEdit.Common.Structures;

namespace TEdit.Common
{
    public class TileMouseEventArgs
    {
        public MouseButtonState LeftButton { get; set; }
        public MouseButtonState RightButton { get; set; }
        public MouseButtonState MiddleButton { get; set; }

        public PointInt32 Tile { get; set; }
        public int WheelDelta { get; set; }
    }
}