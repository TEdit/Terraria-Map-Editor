using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.Geometry.Primitives;
using TEditXna.ViewModel;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Tools
{
    public sealed class PencilTool : BaseTool
    {
        private bool _isLeftDown;
        private bool _isRightDown;
        private Vector2Int32 _startPoint;
        private Vector2Int32 _endPoint;

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
                _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
            }

            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
            CheckDirectionandDraw(e.Location);
        }

        public override void MouseMove(TileMouseState e)
        {
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);
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
            Vector2Int32 p2 = tile;
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
                if ((Keyboard.IsKeyUp(Key.LeftShift)) && (Keyboard.IsKeyUp(Key.RightShift)))
                {
                    DrawLine(p);
                    _startPoint = p;
                    _endPoint = p;
                }
                else if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
                {
                    DrawLineP2P(p2);
                    _endPoint = p2;
                }
            }
        }

        private void DrawLine(Vector2Int32 to)
        {
            foreach (Vector2Int32 pixel in Shape.DrawLineTool(_startPoint, to))
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;
                if (!_wvm.CheckTiles[index])
                {
                    _wvm.CheckTiles[index] = true;
                    if (_wvm.Selection.IsValid(pixel))
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.SetPixel(pixel.X, pixel.Y);

                        /* Heathtech */
                        BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                    }
                }
            }
        }
        private void DrawLineP2P(Vector2Int32 endPoint)
        {
            foreach (Vector2Int32 pixel in Shape.DrawLineTool(_startPoint, _endPoint))
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;
                if (!_wvm.CheckTiles[index])
                {
                    _wvm.CheckTiles[index] = true;
                    if (_wvm.Selection.IsValid(pixel))
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.SetPixel(pixel.X, pixel.Y);

                        /* Heathtech */
                        BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                    }
                }
            }
        }
    }
}
