using Avalonia;
using TEdit5.Controls;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Terraria;
using System.Threading.Tasks;
namespace TEdit5.ViewModels;

public partial class DocumentViewModel : ReactiveObject
{
    [Reactive] private int _zoom = 100;
    [Reactive] private int _minZoom = 7;
    [Reactive] private int _maxZoom = 6400;
    [Reactive] private Point _cursorTileCoordinate;
    [Reactive] private SkiaWorldRenderBox.SelectionModes _selectionMode;

    [Reactive] private World _world;
    [Reactive] private WorldEditor _worldEditor;

    public string FileName { get; }

    public ToolSelectionViewModel ToolSelection { get; }
    public TilePicker TilePicker { get; }
    public ISelection Selection { get; }

    public DocumentViewModel(
        World world,
        string fileName,
        ToolSelectionViewModel toolSelection,
        TilePicker tilePicker)
    {
        _world = world;
        FileName = fileName;

        ToolSelection = toolSelection;
        TilePicker = tilePicker;

        Selection = new Selection();

        IUndoManager undoManager = null;

        _worldEditor = new WorldEditor(
            tilePicker,
            new TEdit.Editor.TileMaskSettings(),
            World,
            Selection,
            undoManager,
            (x, y, height, width) => { });
    }

    public Task SaveAsync()
    {
        // Run synchronously on the UI thread. Saving mutates World state that is
        // bound to the UI (the PropertyGrid over World, plus the Chests/Signs/
        // TileEntities ObservableCollections that Validate() prunes, and
        // FileRevision). World.SaveAsync does this work on a background thread,
        // so those change notifications reach Avalonia bindings off-thread and
        // throw "Call from invalid thread". World.Save is the synchronous variant.
        World.Save(World, FileName);
        return Task.CompletedTask;
    }
}
