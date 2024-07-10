using Avalonia;
using Avalonia.Input;
using Avalonia.Rendering.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEdit.Desktop.Services;
using TEdit.Desktop.ViewModels;
using TEdit.Editor;
using static TEdit.Desktop.Controls.SkiaWorldRenderBox;

namespace TEdit.Desktop.Editor;

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

    public void Press(WorldEditor editor, PointerPoint buttons, Point worldCoordinate)
    {
        if (IsActive)
        {
            editor.SetPixel((int)worldCoordinate.X, (int)worldCoordinate.Y);
        }
    }
}
