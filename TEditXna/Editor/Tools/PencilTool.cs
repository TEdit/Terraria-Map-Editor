using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using TEditXNA.Terraria;
using TEditXna.Editor.Undo;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class PencilTool : ObservableObject, ITool
    {
        private WorldViewModel _wvm;
        private WriteableBitmap _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        private bool[] _checked;
        private bool _isActive;

        public PencilTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/pencil.png"));
            Name = "Pencil";
            IsActive = false;
        }

        public string Name { get; private set; }

        public ToolType ToolType { get { return ToolType.Pixel; } }

        public BitmapImage Icon { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }


        private bool _isLeftDown;
        private bool _isRightDown;
        private Vector2Int32 _startPoint;
        public void MouseDown(TileMouseState e)
        {
            if (!_isRightDown && !_isLeftDown)
            {
                _startPoint = e.Location;
                _checked = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
                
            }

            CheckDirectionandDraw(e.Location);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
        }

        public void MouseMove(TileMouseState e)
        {
            CheckDirectionandDraw(e.Location);
        }

        public void MouseUp(TileMouseState e)
        {
            CheckDirectionandDraw(e.Location);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            _wvm.UndoManager.SaveUndo();
        }

        public void MouseWheel(TileMouseState e)
        {
        }

        public WriteableBitmap PreviewTool() { return _preview; }

        private void CheckDirectionandDraw(Vector2Int32 tile)
        {
            Vector2Int32 p = tile;
            if (_isRightDown)
            {
                if (_isLeftDown)
                    p.X = _startPoint.X;
                else
                    p.Y = _startPoint.Y;

                DrawLine(p);
                _startPoint = p;
            }
            else if (_isLeftDown)
            {
                DrawLine(p);
                _startPoint = p;
            }
        }

        private void DrawLine(Vector2Int32 to)
        {
            foreach (var p in BCCL.Geometry.Shape.DrawLineTool(_startPoint, to))
            {
                // center
                int x0 = p.X - _wvm.Brush.OffsetX;
                int y0 = p.Y - _wvm.Brush.OffsetY;

                _wvm.UndoManager.SaveTile(p);
                _wvm.SetPixel(p.X, p.Y);
            }
        }

        public bool PreviewIsTexture { get { return false; } }
    }

    
}