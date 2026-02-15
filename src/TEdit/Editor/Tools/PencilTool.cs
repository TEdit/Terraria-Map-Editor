using System;
using System.Windows.Input;
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
    private bool _constrainVertical; // true = vertical, false = horizontal
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

        // Start new stroke if not already drawing
        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            _startPoint = e.Location;
            _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
        }

        // Determine drawing mode from actions
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        // If starting constrain mode, determine direction based on first movement
        if (_isConstraining && !_isDrawing)
        {
            // Will determine direction on first move
        }

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

        _wvm.UndoManager.SaveUndo();
    }

    private void ProcessDraw(Vector2Int32 tile)
    {
        Vector2Int32 p = tile;

        if (_isConstraining)
        {
            // Constrained drawing - determine direction and lock to axis
            int dx = Math.Abs(tile.X - _startPoint.X);
            int dy = Math.Abs(tile.Y - _startPoint.Y);

            // Auto-detect direction based on movement
            _constrainVertical = dx < dy;

            if (_constrainVertical)
                p.X = _startPoint.X; // Lock X for vertical line
            else
                p.Y = _startPoint.Y; // Lock Y for horizontal line

            DrawLine(p);
            _startPoint = p;
        }
        else if (_isLineMode)
        {
            // Point-to-point line drawing (preview line from start to current)
            DrawLineP2P(tile);
            _endPoint = tile;
        }
        else if (_isDrawing)
        {
            // Freehand drawing
            DrawLine(p);
            _startPoint = p;
            _endPoint = p;
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
