using Avalonia;
using Avalonia.Controls;
using TEdit5.Controls;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Terraria;

namespace TEdit5.ViewModels;

public partial class DocumentViewModel : ReactiveObject
{
    [Reactive] private int _zoom = 100;
    [Reactive] private int _minZoom = 7;
    [Reactive] private int _maxZoom = 6400;
    [Reactive] private Point _cursorTileCoordinate;
    [Reactive] private SkiaWorldRenderBox.SelectionModes _selectionMode;

    public ToolSelectionViewModel ToolSelection { get; }
    public TilePicker TilePicker { get; }
    public ISelection Selection { get; }
    [Reactive] private World _world;
    [Reactive] private WorldEditor _worldEditor;

    public DocumentViewModel(World world, ToolSelectionViewModel toolSelection, TilePicker tilePicker)
    {
        _world = world;
        ToolSelection = toolSelection;
        TilePicker = tilePicker;
        Selection = new Selection();
        IUndoManager undoManager = null;

        _worldEditor = new WorldEditor(tilePicker, World, Selection, undoManager, (x, y, height, width) => { });
    }
}
