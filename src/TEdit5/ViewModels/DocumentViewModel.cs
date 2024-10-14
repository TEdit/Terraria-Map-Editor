using Avalonia;
using Avalonia.Controls;
using TEdit5.Controls;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Terraria;

namespace TEdit5.ViewModels;

public partial class DocumentViewModel : ReactiveObject
{
    [Reactive] public int Zoom { get; set; } = 100;

    [Reactive] public int MinZoom { get; set; } = 7;

    [Reactive] public int MaxZoom { get; set; } = 6400;

    [Reactive] public Point CursorTileCoordinate { get; set; }

    [Reactive] public SkiaWorldRenderBox.SelectionModes SelectionMode { get; set; }

    public ToolSelectionViewModel ToolSelection { get; }
    public TilePicker TilePicker { get; }
    public ISelection Selection { get; }
    [Reactive] public World World { get; private set; }
    [Reactive] public WorldEditor WorldEditor { get; private set; }

    public DocumentViewModel(World world, ToolSelectionViewModel toolSelection, TilePicker tilePicker)
    {
        World = world;
        ToolSelection = toolSelection;
        TilePicker = tilePicker;
        Selection = new Selection();
        IUndoManager undoManager = null;

        WorldEditor = new WorldEditor(tilePicker, World, Selection, undoManager, (x, y, height, width) => { });
    }
}
