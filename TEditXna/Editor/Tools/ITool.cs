using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BCCL.MvvmLight;

namespace TEditXna.Editor.Tools
{
    public interface ITool
    {
        BitmapImage Icon { get; }
        bool IsActive { get; set; }
        string Name { get; }
        void MouseDown(TileMouseState e);
        void MouseMove(TileMouseState e);
        void MouseUp(TileMouseState e);
        void MouseWheel(TileMouseState e);

        WriteableBitmap PreviewTool();
    }

    public class ArrowTool : ObservableObject, ITool
    {
        private bool _isActive;

        public ArrowTool()
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/cursor.png"));
            Name = "Arrow";
            IsActive = false;
        }

        public string Name { get; private set; }
        public BitmapImage Icon { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }

        public void MouseDown(TileMouseState e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // Check chests and signs and show popup
            }
        }

        public void MouseMove(TileMouseState e)
        {
        }

        public void MouseUp(TileMouseState e)
        {
            
        }

        public void MouseWheel(TileMouseState e)
        {
        }

        public WriteableBitmap PreviewTool()
        {
            return null;
        }
    }
}