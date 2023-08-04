using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Editor;
using TEdit.Terraria.Objects;
using TEdit.UI;
using TEdit.ViewModel;
namespace TEdit.Editor.Tools;

public sealed class SpriteTool2 : BaseTool
{
    private bool _isLeftDown;
    private bool _isRightDown;
    private Vector2Int32 _startPoint;
    private Vector2Int32 _endPoint;
    public SpriteTool2(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/sprite.png"));
        Name = "Sprite2";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    public override bool PreviewIsTexture => true;

    public override void MouseDown(TileMouseState e)
    {
        if (_wvm.SelectedSpriteItem == null)
            return;

        var x = e.Location.X;
        var y = e.Location.Y;

        if (!_isRightDown && !_isLeftDown)
        {
            _startPoint = e.Location;
            _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
        }

        _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
        _isRightDown = (e.RightButton == MouseButtonState.Pressed);

        if (_wvm.SelectedSpriteItem.SizeTiles.X == 1 && _wvm.SelectedSpriteItem.SizeTiles.Y == 1)
            CheckDirectionandDraw(e.Location);
        else
            PlaceSelectedSprite(x, y);
    }

    private void PlaceSelectedSprite(int x, int y)
    {
        ushort tileId = _wvm.SelectedSpriteItem.Tile;

        _wvm.SelectedSpriteItem.Place(x, y, _wvm);

        if (TileTypes.IsTileEntity(tileId))
        {
            // if the tile entity is not the same as it was, create a new TE.
            var existingTe = _wvm.CurrentWorld.GetTileEntityAtTile(x, y);
            if (existingTe == null || (ushort)existingTe.TileType != _wvm.SelectedSpriteSheet.Tile)
            {
                var te = TileEntity.CreateForTile(_wvm.CurrentWorld.Tiles[x, y], x, y, 0);
                TileEntity.PlaceEntity(te, _wvm.CurrentWorld); // this will also remove the existing if there is one
            }
        }
        else if (TileTypes.IsChest(tileId))
        {
            var existingChest = _wvm.CurrentWorld.GetChestAtTile(x, y);
            if (existingChest == null)
            {
                _wvm.CurrentWorld.Chests.Add(new Chest(x, y));
            }
        }
        else if (TileTypes.IsSign(tileId))
        {
            var existingSign = _wvm.CurrentWorld.GetSignAtTile(x, y);
            if (existingSign == null)
            {
                _wvm.CurrentWorld.Signs.Add(new Sign(x, y, string.Empty));
            }
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
        _isRightDown = (e.RightButton == MouseButtonState.Pressed);

        if (_wvm.SelectedSpriteItem == null) { return; }
        if (_wvm.SelectedSpriteItem.SizeTiles.X == 1 && _wvm.SelectedSpriteItem.SizeTiles.Y == 1)
        {
            CheckDirectionandDraw(e.Location);
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        if (_wvm.SelectedSpriteItem == null) { return; }
        if (_wvm.SelectedSpriteItem.SizeTiles.X == 1 && _wvm.SelectedSpriteItem.SizeTiles.Y == 1)
        {
            CheckDirectionandDraw(e.Location);
        }

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
                    if (_wvm.SelectedSpriteSheet == null)
                        return;

                    PlaceSelectedSprite(pixel.X, pixel.Y);
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
                    if (_wvm.SelectedSpriteSheet == null)
                        return;


                    PlaceSelectedSprite(pixel.X, pixel.Y);
                }
            }
        }
    }

    public override WriteableBitmap PreviewTool()
    {
        if (_wvm.SelectedSpriteItem != null)
            return ((SpriteItemPreview)_wvm.SelectedSpriteItem).Preview;
        return base.PreviewTool();
    }
}
