using System;
using BCCL.MvvmLight;

namespace TEditXna.Editor
{
    public class BrushSettings : ObservableObject
    {
        private int _maxOutline = 10;
        private int _maxHeight = 200;
        private int _maxWidth = 200;
        private int _minHeight = 2;
        private int _minWidth = 2;

        private int _width = 20;
        private int _height = 20;
        private bool _isLocked = false;
        private bool _isOutline = false;
        private BrushShape _brushShape = BrushShape.Square;




        public event EventHandler BrushChanged;

        protected virtual void OnBrushChanged(object sender, EventArgs e)
        {
            if (BrushChanged != null) BrushChanged(sender, e);
        }

        public int MaxWidth { get { return _maxWidth; } }
        public int MaxHeight { get { return _maxHeight; } }
        public int MinWidth { get { return _minWidth; } }
        public int MinHeight { get { return _minHeight; } }

        public BrushShape BrushShape
        {
            get { return _brushShape; }
            set { Set("BrushShape", ref _brushShape, value); }
        }

        public int MaxOutline
        {
            get { return _maxOutline; }
            private set { Set("MaxOutline", ref _maxOutline, value); }
        }

        public bool IsOutline
        {
            get { return _isOutline; }
            set { Set("IsOutline", ref _isOutline, value); }
        }

        public bool IsLocked
        {
            get { return _isLocked; }
            set { Set("IsLocked", ref _isLocked, value); }
        }

        public int Height
        {
            get { return _height; }
            set { Set("Height", ref _height, value); }
        }

        public int Width
        {
            get { return _width; }
            set { Set("Width", ref _width, value); }
        }
    }
}