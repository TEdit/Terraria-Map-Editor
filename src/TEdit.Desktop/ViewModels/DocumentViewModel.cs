using Avalonia;
using TEdit.Desktop.Editor;
using TEdit.Terraria;

namespace TEdit.Desktop.ViewModels;

public partial class DocumentViewModel : ReactiveObject
{
    [Reactive] public World? World { get; set; }

    [Reactive] public int Zoom { get; set; } = 100;

    [Reactive] public int MinZoom { get; set; } = 7;

    [Reactive] public int MaxZoom { get; set; } = 6400;

    [Reactive] public Point CursorTileCoordinate { get; set; }

    [Reactive] public IMouseTool? ActiveTool { get; set; }

    public DocumentViewModel()
    {
    }
}
