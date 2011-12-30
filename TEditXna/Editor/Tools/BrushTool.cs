using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class BrushTool : ObservableObject, ITool
    {
        private WorldViewModel _wvm;
        private WriteableBitmap _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
        private bool _isActive;
        private bool[] _setThisPass;

        public BrushTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paintbrush.png"));
            Name = "Brush";
            IsActive = false;
        }

        public string Name { get; private set; }

        public ToolType ToolType { get { return ToolType.Brush; } }

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
                _setThisPass = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
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

        public WriteableBitmap PreviewTool()
        {
            var bmp = new WriteableBitmap(_wvm.Brush.Width + 1, _wvm.Brush.Height + 1, 96, 96, PixelFormats.Bgra32, null);

            bmp.Clear();
            if (_wvm.Brush.Shape == BrushShape.Square)
                bmp.FillRectangle(0, 0, _wvm.Brush.Width, _wvm.Brush.Height, Color.FromArgb(127, 0, 90, 255));
            else
                bmp.FillEllipse(0, 0, _wvm.Brush.Width / 2, _wvm.Brush.Height / 2, Color.FromArgb(127, 0, 90, 255));

            _preview = bmp;
            return _preview;
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
            foreach (var point in BCCL.Geometry.Shape.DrawLineTool(_startPoint, to))
            {
                if (_wvm.Brush.Shape == BrushShape.Round)
                {
                    FillRound(point);
                }
                else if (_wvm.Brush.Shape == BrushShape.Square)
                {
                    FillRectangle(point);
                }
            }
        }

        private void FillRectangle(Vector2Int32 point)
        {
            if (_wvm.Brush.IsOutline)
            {
                var area = BCCL.Geometry.Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height));
                var interrior = BCCL.Geometry.Fill.FillRectangleCentered(point,
                    new Vector2Int32(
                        _wvm.Brush.Width - _wvm.Brush.Outline * 2,
                        _wvm.Brush.Height - _wvm.Brush.Outline * 2));
                FillHollow(area, interrior);
            }
            else
            {
                var area = BCCL.Geometry.Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height));
                FillSolid(area);
            }
        }

        private void FillRound(Vector2Int32 point)
        {
            if (_wvm.Brush.IsOutline)
            {
                var area = BCCL.Geometry.Fill.FillEllipseCentered(point, new Vector2Int32(_wvm.Brush.Width / 2, _wvm.Brush.Height / 2));
                var interrior = BCCL.Geometry.Fill.FillEllipseCentered(point,
                    new Vector2Int32(
                        _wvm.Brush.Width / 2 - _wvm.Brush.Outline * 2,
                        _wvm.Brush.Height / 2 - _wvm.Brush.Outline * 2));
                FillHollow(area, interrior);
            }
            else
            {
                var area = BCCL.Geometry.Fill.FillEllipseCentered(point, new Vector2Int32(_wvm.Brush.Width / 2, _wvm.Brush.Height / 2));
                FillSolid(area);
            }
        }

        private void FillSolid(IEnumerable<Vector2Int32> area)
        {
            foreach (var pixel in area)
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;
                if (!_setThisPass[index])
                {
                    _setThisPass[index] = true;
                    _wvm.UndoManager.SaveTile(pixel);
                    _wvm.SetPixel(pixel.X, pixel.Y);
                }
            }
        }

        private void FillHollow(IEnumerable<Vector2Int32> area, IEnumerable<Vector2Int32> interrior)
        {
            var border = area.Except(interrior);

            // Draw the border
            if (_wvm.TilePicker.PaintMode == PaintMode.Tile || _wvm.TilePicker.PaintMode == PaintMode.TileAndWall)
            {
                foreach (var pixel in border)
                {
                    if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                    int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;
                    if (!_setThisPass[index])
                    {
                        _setThisPass[index] = true;
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.Tile);
                    }
                }
            }

            // Draw the wall in the interrior, exclude the border so no overlaps
            foreach (var pixel in interrior)
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;
                _wvm.UndoManager.SaveTile(pixel);
                _wvm.SetPixel(pixel.X, pixel.Y, erase: true);

                if (_wvm.TilePicker.PaintMode == PaintMode.Wall || _wvm.TilePicker.PaintMode == PaintMode.TileAndWall)
                    _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.Wall);
            }
        }

        public bool PreviewIsTexture { get { return false; } }

    }
}