using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using TEdit5.Editor;

namespace TEdit5.ViewModels;

public partial class ToolSelectionViewModel : ReactiveObject
{
    [Reactive] public IMouseTool? ActiveTool { get; set; }

    [Reactive]
    public List<IMouseTool> Tools { get; set; } = [];

    public ReactiveCommand<IMouseTool, Unit> SetToolCommand { get; }

    public ToolSelectionViewModel(IEnumerable<IMouseTool> tools)
    {
        Tools = tools.ToList();
        SetToolCommand = ReactiveCommand.Create<IMouseTool>(SetTool);

        SetTool(Tools.FirstOrDefault(t => t.Name == "Arrow"));
    }

    private void SetTool(IMouseTool? tool)
    {
        // deactivate previous tool
        if (ActiveTool != null) { ActiveTool.IsActive = false; }

        // activate new tool
        if (tool != null)
        {
            tool.IsActive = true;
            ActiveTool = tool;
        }
    }
}
