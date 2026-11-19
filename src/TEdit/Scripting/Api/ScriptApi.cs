using System;
using System.Threading.Tasks;
using TEdit.Editor;
using TEdit.Editor.Undo;
using TEdit.Scripting.Engine;
using TEdit.Terraria;
using TEdit.ViewModel;

namespace TEdit.Scripting.Api;

public class ScriptApi : IDisposable
{
    private readonly WorldViewModel? _wvm;
    private readonly IUndoManager _undo;
    private readonly Func<Task>? _render;
    private int _apiCallDepth;

    public TileApi Tile { get; private set; }
    public GeometryApi Geometry { get; private set; }
    public ChestApi Chests { get; private set; }
    public SignApi Signs { get; private set; }
    public NpcApi Npcs { get; private set; }
    public WorldInfoApi World { get; private set; }
    public SelectionApi Selection { get; private set; }
    public MetadataApi Metadata { get; private set; }
    public LogApi Log { get; private set; }
    public BatchApi Batch { get; private set; }
    public ToolsApi? Tools { get; private set; }
    public FinderApi Finder { get; private set; }
    public SpriteApi Sprites { get; private set; }
    public DrawApi Draw { get; private set; }
    public TileEntityApi TileEntities { get; private set; }
    public GenerateApi Generate { get; private set; }

    public ScriptApi(WorldViewModel wvm, ScriptExecutionContext context)
    {
        _wvm = wvm;
        _undo = wvm.UndoManager;
        BindWorld(wvm, context);
        Tools = new ToolsApi(wvm, () => _apiCallDepth <= 1, () => BindWorld(wvm, context), context.CancellationToken);
    }

    private void BindWorld(WorldViewModel wvm, ScriptExecutionContext context)
    {
        var world = wvm.CurrentWorld;

        Tile = new TileApi(world, wvm.UndoManager);
        Geometry = new GeometryApi(world, wvm.UndoManager);
        Chests = new ChestApi(world);
        Signs = new SignApi(world);
        TileEntities = new TileEntityApi(world);
        Npcs = new NpcApi(world);
        World = new WorldInfoApi(world);
        Selection ??= new SelectionApi(wvm.Selection);
        Metadata ??= new MetadataApi();
        Log ??= new LogApi(context);
        Batch = new BatchApi(world, wvm.Selection, wvm.UndoManager, context);

        Finder ??= new FinderApi(context);
        Sprites = new SpriteApi(world, wvm.UndoManager);
        Draw = new DrawApi(world, wvm.UndoManager, wvm.Selection, context.CancellationToken);
        Generate = new GenerateApi(world, wvm.UndoManager, wvm.Selection);
    }

    /// <summary>
    /// Test constructor - creates ScriptApi without WorldViewModel dependency.
    /// </summary>
    public ScriptApi(World world, IUndoManager undo, ISelection selection, ScriptExecutionContext context, Func<Task>? render = null)
    {
        _wvm = null;
        _undo = undo;
        _render = render;

        Tile = new TileApi(world, undo);
        Geometry = new GeometryApi(world, undo);
        Chests = new ChestApi(world);
        Signs = new SignApi(world);
        TileEntities = new TileEntityApi(world);
        Npcs = new NpcApi(world);
        World = new WorldInfoApi(world);
        Selection = new SelectionApi(selection);
        Metadata = new MetadataApi();
        Log = new LogApi(context);
        Batch = new BatchApi(world, selection, undo, context);
        Tools = null;
        Finder = new FinderApi(context);
        Sprites = new SpriteApi(world, undo);
        Draw = new DrawApi(world, undo, selection, context.CancellationToken);
        Generate = new GenerateApi(world, undo, selection);
    }

    internal void EnterApiCall() => _apiCallDepth++;
    internal void ExitApiCall() => _apiCallDepth--;

    public void BeginExecution()
    {
        // Undo frame is managed implicitly - SaveTile records each tile before modification
    }

    public async Task EndExecutionAsync()
    {
        if (_wvm != null)
        {
            await _wvm.FinalizeScriptExecutionAsync();
        }
        else
        {
            await _undo.SaveUndoAsync();
            if (_render != null) await _render();
        }
    }

    public void Dispose()
    {
    }
}
