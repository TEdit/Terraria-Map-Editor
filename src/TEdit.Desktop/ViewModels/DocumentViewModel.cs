using Avalonia;
using Avalonia.Controls;
using TEdit.Desktop.Controls;
using TEdit.Terraria;

namespace TEdit.Desktop.ViewModels;

public partial class DocumentViewModel : ReactiveObject
{
    [Reactive] public World? World { get; set; }

    [Reactive] public int Zoom { get; set; } = 100;

    [Reactive] public int MinZoom { get; set; } = 7;

    [Reactive] public int MaxZoom { get; set; } = 6400;

    [Reactive] public Point CursorTileCoordinate { get; set; }

    [Reactive] public SkiaWorldRenderBox.SelectionModes SelectionMode { get; set; }

    public ToolSelectionViewModel ToolSelection { get; }

    public DocumentViewModel(ToolSelectionViewModel toolSelection)
    {
        ToolSelection = toolSelection;
    }
}
