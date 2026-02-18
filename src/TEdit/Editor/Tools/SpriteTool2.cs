using System;
using System.Windows.Media.Imaging;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Terraria.Editor;
using TEdit.Terraria.Objects;
using TEdit.UI;
using TEdit.ViewModel;
using Wpf.Ui.Controls;
namespace TEdit.Editor.Tools;

public sealed class SpriteTool2 : BaseTool
{
    private bool _isDrawing;
    private bool _isConstraining;
    private bool _isLineMode;
    private bool _constrainVertical;
    private bool _constrainDirectionLocked;
    private Vector2Int32 _anchorPoint;
    private Vector2Int32 _startPoint;
    private Vector2Int32 _endPoint;
    public SpriteTool2(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/sprite.png"));
        SymbolIcon = SymbolRegular.Couch20;
        Name = "Sprite2";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    public override bool PreviewIsTexture => true;

    public override void MouseDown(TileMouseState e)
    {
        if (_wvm.SelectedSpriteItem == null)
            return;

        var actions = GetActiveActions(e);

        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            _startPoint = e.Location;
            _anchorPoint = e.Location;
            _constrainDirectionLocked = false;
            int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
            if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
                _wvm.CheckTiles = new int[totalTiles];
            if (++_wvm.CheckTileGeneration <= 0)
            {
                _wvm.CheckTileGeneration = 1;
                Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
            }
        }

        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        if (_wvm.SelectedSpriteItem.SizeTiles.X == 1 && _wvm.SelectedSpriteItem.SizeTiles.Y == 1)
            ProcessDraw(e.Location);
        else if (_isDrawing || _isConstraining || _isLineMode)
            PlaceSelectedSprite(e.Location.X, e.Location.Y);
    }

    private void PlaceSelectedSprite(int x, int y)
    {
        ushort tileId = _wvm.SelectedSpriteItem.Tile;

        _wvm.SelectedSpriteItem.Place(x, y, _wvm);

        if (TileTypes.IsTileEntity(tileId))
        {
            // if the tile entity is not the same as it was, create a new TE.
            var existingTe = _wvm.CurrentWorld.GetTileEntityAtTile(x, y);
            if (existingTe == null || (ushort)existingTe.TileType != tileId)
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
        if (_wvm.SelectedSpriteItem == null) return;

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        if (_wvm.SelectedSpriteItem.SizeTiles.X == 1 && _wvm.SelectedSpriteItem.SizeTiles.Y == 1)
        {
            ProcessDraw(e.Location);
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        if (_wvm.SelectedSpriteItem == null) return;

        if (_wvm.SelectedSpriteItem.SizeTiles.X == 1 && _wvm.SelectedSpriteItem.SizeTiles.Y == 1)
        {
            ProcessDraw(e.Location);
        }

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");
        _constrainDirectionLocked = false;

        _wvm.UndoManager.SaveUndo();
    }

    private void ProcessDraw(Vector2Int32 tile)
    {
        Vector2Int32 p = tile;

        if (_isConstraining)
        {
            if (!_constrainDirectionLocked)
            {
                int dx = Math.Abs(tile.X - _anchorPoint.X);
                int dy = Math.Abs(tile.Y - _anchorPoint.Y);
                if (dx > 1 || dy > 1)
                {
                    _constrainVertical = dx < dy;
                    _constrainDirectionLocked = true;
                }
            }

            if (_constrainVertical)
                p.X = _anchorPoint.X;
            else
                p.Y = _anchorPoint.Y;

            DrawLine(p);
            _startPoint = p;
        }
        else if (_isLineMode)
        {
            DrawLineP2P(tile);
            _endPoint = tile;
        }
        else if (_isDrawing)
        {
            DrawLine(p);
            _startPoint = p;
            _endPoint = p;
        }
    }

    private void DrawLine(Vector2Int32 to)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        foreach (Vector2Int32 pixel in Shape.DrawLineTool(_startPoint, to))
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;
            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
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
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        foreach (Vector2Int32 pixel in Shape.DrawLineTool(_startPoint, _endPoint))
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;
            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
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
