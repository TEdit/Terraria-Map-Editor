using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BCCL.Geometry;
using BCCL.Geometry.Primitives;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public sealed class PencilTool : BaseTool
    {
        private bool _isLeftDown;
        private bool _isRightDown;
        private Vector2Int32 _startPoint;

        public PencilTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/pencil.png"));
            Name = "Pencil";
            IsActive = false;
            ToolType = ToolType.Pixel;
        }

        public override void MouseDown(TileMouseState e)
        {
            if (!_isRightDown && !_isLeftDown)
            {
                _startPoint = e.Location;
            }

            CheckDirectionandDraw(e.Location);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
        }

        public override void MouseMove(TileMouseState e)
        {
            CheckDirectionandDraw(e.Location);
        }

        public override void MouseUp(TileMouseState e)
        {
            CheckDirectionandDraw(e.Location);
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            _wvm.UndoManager.SaveUndo();
        }

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
            foreach (Vector2Int32 p in Shape.DrawLineTool(_startPoint, to))
            {
                if (_wvm.Selection.IsValid(p))
                {
                    _wvm.UndoManager.SaveTile(p);
                    _wvm.SetPixel(p.X, p.Y);
                }
            }
        }
    }
}