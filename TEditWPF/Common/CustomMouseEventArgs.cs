using System.Windows.Input;

namespace TEditWPF.Common
{
    public class CustomMouseEventArgs
    {
        public MouseButtonState LeftButton { get; set; }
        public MouseButtonState RightButton { get; set; }
        public MouseButtonState MiddleButton { get; set; }

        public System.Windows.Point Location { get; set; }
        public int WheelDelta { get; set; }
    }
}
