using Avalonia;
using Avalonia.Input;
using Avalonia.Rendering.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TEdit5.Services;
using TEdit5.ViewModels;
using TEdit.Editor;
using TEdit.Geometry;
using static TEdit5.Controls.SkiaWorldRenderBox;

namespace TEdit5.Editor;

public interface IMouseTool
{
    ICustomDrawOperation? DrawTool { get; }
    string Name { get; }
    string Tooltip { get; }
    bool IsActive { get; set; }
    string IconName { get; }
    void Move(WorldEditor editor, PointerPoint buttons, Point worldCoordinate) { }
    void Press(WorldEditor editor, PointerPoint buttons, Point worldCoordinate) { }
    void Release(WorldEditor editor, PointerPoint buttons, Point worldCoordinate) { }
    void LeaveWindow(WorldEditor editor, PointerPoint buttons, Point worldCoordinate) { }
    void Scroll(int delta, Point worldCoordinate) { }
}

public class BrushTool : ReactiveObject, IMouseTool
{
    public ICustomDrawOperation? DrawTool { get; }
    public string Name { get; } = "Brush";
    public string Tooltip { get; } = "Brush Tool";
    [Reactive] public bool IsActive { get; set; }
    public string IconName { get; } = "mdi-brush";
}

public class ArrowTool : ReactiveObject, IMouseTool
{
    public ICustomDrawOperation? DrawTool { get; }
    public string Name { get; } = "Arrow";
    public string Tooltip { get; } = "Arrow Tool";
    [Reactive] public bool IsActive { get; set; }
    public string IconName { get; } = "mdi-cursor-default";
}

public class SelectTool : ReactiveObject, IMouseTool
{
    public ICustomDrawOperation? DrawTool { get; }
    public string Name { get; } = "Select";
    public string Tooltip { get; } = "Select Tool";
    [Reactive] public bool IsActive { get; set; }
    public string IconName { get; } = "mdi-select";
}

public class PencilTool : ReactiveObject, IMouseTool
{
    private readonly IDocumentService _documentService;

    public PencilTool(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public ICustomDrawOperation? DrawTool { get; }
    public string Name { get; } = "Pencil";
    public string Tooltip { get; } = "Pencil Tool";
    [Reactive] public bool IsActive { get; set; }
    public string IconName { get; } = "mdi-pencil";

    private bool _isLeftDown;
    private bool _isRightDown;
    private Vector2Int32 _startPoint;
    private Vector2Int32 _endPoint;
    private WorldEditor _editor;

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
            System.Diagnostics.Debug.WriteLine($"Update _startpoint {_startPoint} CheckDirectionandDraw _isRightDown");
        }
        else if (_isLeftDown)
        {
            DrawLine(p);
            _startPoint = p;
            System.Diagnostics.Debug.WriteLine($"Update _startpoint {_startPoint} CheckDirectionandDraw _isLeftDown");
            _endPoint = p;
        }
    }

    private void DrawLine(Vector2Int32 to)
    {
        foreach (Vector2Int32 pixel in Shape.DrawLineTool(_startPoint, to))
        {
            _editor.SetPixel(pixel.X, pixel.Y);
            //BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
        }
    }

    private void DrawLineP2P(Vector2Int32 endPoint)
    {
        foreach (Vector2Int32 pixel in Shape.DrawLineTool(_startPoint, _endPoint))
        {
            _editor.SetPixel(pixel.X, pixel.Y);
        }
    }

    public void Press(WorldEditor editor, PointerPoint buttons, Point worldCoordinate)
    {
        _editor = editor;
        _editor.BeginOperationAsync().Wait();

        if (!_isRightDown && !_isLeftDown)
        {
            _startPoint = new Vector2Int32((int)worldCoordinate.X, (int)worldCoordinate.Y);
        }

        _isLeftDown = buttons.Properties.IsLeftButtonPressed;
        _isRightDown = buttons.Properties.IsRightButtonPressed;

        CheckDirectionandDraw(new Vector2Int32((int)worldCoordinate.X, (int)worldCoordinate.Y));
    }

    public void Move(WorldEditor editor, PointerPoint buttons, Point worldCoordinate)
    {
        _isLeftDown = buttons.Properties.IsLeftButtonPressed;
        _isRightDown = buttons.Properties.IsRightButtonPressed;

        CheckDirectionandDraw(new Vector2Int32((int)worldCoordinate.X, (int)worldCoordinate.Y));
    }
    public void Release(WorldEditor editor, PointerPoint buttons, Point worldCoordinate)
    {
        CheckDirectionandDraw(new Vector2Int32((int)worldCoordinate.X, (int)worldCoordinate.Y));

        _isLeftDown = buttons.Properties.IsLeftButtonPressed;
        _isRightDown = buttons.Properties.IsRightButtonPressed;

        if (!_isLeftDown && !_isRightDown)
        {
            _editor.EndOperationAsync().Wait();
        }
    }

    public void LeaveWindow(WorldEditor editor, PointerPoint buttons, Point worldCoordinate)
    {
    }
}
