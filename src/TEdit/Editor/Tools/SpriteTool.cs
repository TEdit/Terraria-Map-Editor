using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Terraria.Objects;
namespace TEdit.Editor.Tools
{
    public sealed class SpriteTool : BaseTool
    {
        private bool _isLeftDown;
        private bool _isRightDown;
        private Vector2Int32 _startPoint;
        private Vector2Int32 _endPoint;

        public SpriteTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/sprite.png"));
            Name = "Sprite";
            IsActive = false;
            ToolType = ToolType.Pixel;
        }
        public override bool PreviewIsTexture
        {
            get
            {
                if (_wvm.SelectedSprite != null)
                    return _wvm.SelectedSprite.IsPreviewTexture;
                return false;
            }
        }
        public override void MouseDown(TileMouseState e)
        {
            if (_wvm.SelectedSprite == null)
                return;

            Sprite.PlaceSprite(e.Location.X, e.Location.Y, _wvm.SelectedSprite, _wvm);
            if (Tile.IsTileEntity(_wvm.SelectedSprite.Tile))
            {
                var te = TileEntity.CreateForTile(_wvm.CurrentWorld.Tiles[e.Location.X, e.Location.Y], e.Location.X, e.Location.Y, 0);
                TileEntity.PlaceEntity(te, _wvm);
            }

            if (!_isRightDown && !_isLeftDown)
            {
                _startPoint = e.Location;
                _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
            }

            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);

            if (_wvm.SelectedSprite.Size.X == 1 && _wvm.SelectedSprite.Size.Y == 1)
                CheckDirectionandDraw(e.Location);
        }
        public override void MouseMove(TileMouseState e)
        {
            _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
            _isRightDown = (e.RightButton == MouseButtonState.Pressed);

            if (_wvm.SelectedSprite == null)
                return;
            if (_wvm.SelectedSprite.Size.X == 1 && _wvm.SelectedSprite.Size.Y == 1)
                CheckDirectionandDraw(e.Location);
        }
        public override void MouseUp(TileMouseState e)
        {
            if (_wvm.SelectedSprite == null)
                return;
            if (_wvm.SelectedSprite.Size.X == 1 && _wvm.SelectedSprite.Size.Y == 1)
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
                        if (_wvm.SelectedSprite == null)
                            return;

                        Sprite.PlaceSprite(pixel.X, pixel.Y, _wvm.SelectedSprite, _wvm);
                        if (Tile.IsTileEntity(_wvm.SelectedSprite.Tile))
                        {
                            var te = TileEntity.CreateForTile(_wvm.CurrentWorld.Tiles[pixel.X, pixel.Y], pixel.X, pixel.Y, 0);
                            TileEntity.PlaceEntity(te, _wvm);
                        }
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
                        if (_wvm.SelectedSprite == null)
                            return;

                        Sprite.PlaceSprite(pixel.X, pixel.Y, _wvm.SelectedSprite, _wvm);
                        if (Tile.IsTileEntity(_wvm.SelectedSprite.Tile))
                        {
                            var te = TileEntity.CreateForTile(_wvm.CurrentWorld.Tiles[pixel.X, pixel.Y], pixel.X, pixel.Y, 0);
                            TileEntity.PlaceEntity(te, _wvm);
                        }

                    }
                }
            }
        }

        public override WriteableBitmap PreviewTool()
        {
            if (_wvm.SelectedSprite != null)
                return _wvm.SelectedSprite.Preview;
            return base.PreviewTool();
        }
    }
}
