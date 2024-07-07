using Avalonia.Rendering.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEdit.Desktop.Editor;

public interface IMouseTool
{
    public ICustomDrawOperation? DrawTool { get; }
    public string Name { get; }
    public string Tooltip { get; }
    public bool IsActive { get; set; }
    public string IconName { get; }
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
    public ICustomDrawOperation? DrawTool { get; }
    public string Name { get; } = "Pencil";
    public string Tooltip { get; } = "Pencil Tool";
    [Reactive] public bool IsActive { get; set; }
    public string IconName { get; } = "mdi-pencil";
}