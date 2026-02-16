using System;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Scripting.Engine;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Scripting.Api;

public class ScriptApi : IDisposable
{
    private readonly WorldViewModel? _wvm;

    public TileApi Tile { get; }
    public GeometryApi Geometry { get; }
    public ChestApi Chests { get; }
    public SignApi Signs { get; }
    public NpcApi Npcs { get; }
    public WorldInfoApi World { get; }
    public SelectionApi Selection { get; }
    public MetadataApi Metadata { get; }
    public LogApi Log { get; }
    public BatchApi Batch { get; }
    public ToolsApi? Tools { get; }
    public FinderApi Finder { get; }

    public ScriptApi(WorldViewModel wvm, ScriptExecutionContext context)
    {
        _wvm = wvm;
        var undo = new UndoManagerWrapper(wvm.UndoManager);
        var world = wvm.CurrentWorld;

        Tile = new TileApi(world, undo);
        Geometry = new GeometryApi(world, undo);
        Chests = new ChestApi(world);
        Signs = new SignApi(world);
        Npcs = new NpcApi(world);
        World = new WorldInfoApi(world);
        Selection = new SelectionApi(wvm.Selection);
        Metadata = new MetadataApi();
        Log = new LogApi(context);
        Batch = new BatchApi(world, wvm.Selection, undo, context);
        Tools = new ToolsApi(wvm);
        Finder = new FinderApi(context);
    }

    /// <summary>
    /// Test constructor - creates ScriptApi without WorldViewModel dependency.
    /// </summary>
    public ScriptApi(World world, IUndoManager undo, ISelection selection, ScriptExecutionContext context)
    {
        _wvm = null;

        Tile = new TileApi(world, undo);
        Geometry = new GeometryApi(world, undo);
        Chests = new ChestApi(world);
        Signs = new SignApi(world);
        Npcs = new NpcApi(world);
        World = new WorldInfoApi(world);
        Selection = new SelectionApi(selection);
        Metadata = new MetadataApi();
        Log = new LogApi(context);
        Batch = new BatchApi(world, selection, undo, context);
        Tools = null;
        Finder = new FinderApi(context);
    }

    public void BeginExecution()
    {
        // Undo frame is managed implicitly - SaveTile records each tile before modification
    }

    public void EndExecution()
    {
        if (_wvm != null)
        {
            _wvm.UndoManager.SaveUndo();
            _wvm.UpdateRenderWorld();
        }
    }

    public void Dispose()
    {
    }
}
