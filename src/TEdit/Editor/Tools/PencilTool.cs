using System;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using TEdit.Render;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class PencilTool : BaseTool
{
    private bool _isDrawing;
    private bool _isConstraining;
    private bool _isLineMode;
    private int _constrainDirection; // 0=horizontal, 1=vertical, 2=diagonal
    private bool _constrainDirectionLocked;
    private Vector2Int32 _anchorPoint;
    private Vector2Int32 _startPoint;
    private Vector2Int32 _endPoint;

    public PencilTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/pencil.png"));
        SymbolIcon = SymbolRegular.Edit24;
        Name = "Pencil";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    public override void MouseDown(TileMouseState e)
    {
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

        ProcessDraw(e.Location);
    }

    public override void MouseMove(TileMouseState e)
    {
        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        ProcessDraw(e.Location);
    }

    public override void MouseUp(TileMouseState e)
    {
        ProcessDraw(e.Location);

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
                    _constrainDirection = ConstrainHelper.DetectDirection(dx, dy);
                    _constrainDirectionLocked = true;
                }
            }

            p = ConstrainHelper.Snap(tile, _anchorPoint, _constrainDirection);

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
        // Use thin line for Track mode to avoid 2px-thick staircases on diagonals
        var linePoints = _wvm.TilePicker.PaintMode == PaintMode.Track
            ? Shape.DrawLineThin(_startPoint, to)
            : Shape.DrawLineTool(_startPoint, to);
        foreach (Vector2Int32 pixel in linePoints)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
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
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        var linePoints = _wvm.TilePicker.PaintMode == PaintMode.Track
            ? Shape.DrawLineThin(_startPoint, _endPoint)
            : Shape.DrawLineTool(_startPoint, _endPoint);
        foreach (Vector2Int32 pixel in linePoints)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
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
