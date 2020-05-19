using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using TEdit.ViewModel;

namespace TEdit.Editor.Tools
{
    public class SelectionTool : ObservableObject, ITool
    {
        private readonly WriteableBitmap _preview;
        private readonly WorldViewModel _wvm;
        private bool _isActive;
        private Vector2Int32 _startSelection;
        private Vector2Int32 _modifySelection;
        private double _previewScale = 1;

        public SelectionTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/shape_square.png"));
            Name = "Selection";
            IsActive = false;
        }

        #region ITool Members

        public string Name { get; private set; }

        public ToolType ToolType
        {
            get { return ToolType.Pixel; }
        }

        public BitmapImage Icon { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }

        public void MouseDown(TileMouseState e)
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

        public void MouseMove(TileMouseState e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _wvm.Selection.SetRectangle(_startSelection, e.Location);
        }

        public void MouseUp(TileMouseState e)
        {
        }

        public void MouseWheel(TileMouseState e)
        {
        }

        public double PreviewScale
        {
            get { return _previewScale; }
            protected set { _previewScale = value; }
        }

        public WriteableBitmap PreviewTool()
        {
            return _preview;
        }

        public bool PreviewIsTexture
        {
            get { return false; }
        }

        #endregion
    }
}
