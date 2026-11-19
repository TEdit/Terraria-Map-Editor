using System.Threading;
using System;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Scripting.Execution;

namespace TEdit.ViewModel;

public partial class WorldViewModel
{
    private ScriptExecutionService? _scriptExecution;
    public WorldOperationCoordinator WorldOperations { get; } = new();
    private WorldOperationCoordinator.Lease? _manualOperation;
    private IDisposable? _worldChangeTracker;

    public bool IsWorldBusy => WorldOperations.IsBusy;
    public bool IsWorldInteractionEnabled => !WorldOperations.IsBusy || WorldOperations.ActiveName == "Manual stroke";
    public bool CanCancelWorldOperation => WorldOperations.ActiveName is "Script" or "MCP drawing" or "MCP script" or "ExportMapTiles" or "Export map tiles";
    private WorldRevision? _trackedRevision;

    private void InitializeWorldOperations()
    {
        WorldOperations.StateChanged += () =>
        {
            void Update()
            {
                this.RaisePropertyChanged(nameof(IsWorldBusy));
                this.RaisePropertyChanged(nameof(IsWorldInteractionEnabled));
                this.RaisePropertyChanged(nameof(CanCancelWorldOperation));
                var revision = WorldOperations.GetRevision(CurrentWorld);
                if (!WorldOperations.IsBusy && CurrentWorld != null && _trackedRevision != revision)
                {
                    _worldChangeTracker?.Dispose();
                    _worldChangeTracker = new TEdit.Scripting.Execution.WorldChangeTracker(CurrentWorld, WorldOperations.NotifyMutation,
                        () => WorldOperations.CanMutate || (!WorldOperations.IsBusy && Application.Current?.Dispatcher.CheckAccess() == true));
                    _trackedRevision = revision;
                }
            }
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess()) dispatcher.BeginInvoke(Update);
            else Update();
        };
    }

    [ReactiveCommand]
    private void CancelWorldOperation() => WorldOperations.CancelActive();

    public void CompleteManualStroke()
    {
        if (_manualOperation == null) return;
        using var owner = _manualOperation.Enter();
        try
        {
            var last = MouseOverTile.MouseState;
            if (last != null)
                ActiveTool?.MouseUp(new TEdit.UI.TileMouseState { Location = last.Location });
            UndoManager?.SaveUndo();
        }
        finally
        {
            _manualOperation.Dispose();
            _manualOperation = null;
        }
    }

    private async void ObserveWorldTask(Task task)
    {
        try { await task; }
        catch (Exception ex) { ErrorLogging.LogException(ex); }
    }

    internal void MarkScriptSaveCompleted(string filename)
    {
        CurrentFile = filename;
        _lastSavedUndoIndex = _lastUserSavedUndoIndex = UndoManager.CurrentIndex;
        _hasUnsavedPropertyChanges = _hasUnsavedUserPropertyChanges = false;
        this.RaisePropertyChanged(nameof(HasUnsavedChanges));
        this.RaisePropertyChanged(nameof(HasUnsavedUserChanges));
        UpdateTitle();
    }

    internal async Task FinalizeScriptExecutionAsync()
    {
        await UndoManager.SaveUndoAsync();
        var world = CurrentWorld;
        await Task.Run(() => TEdit.Render.BlendRules.ResetUVCache(world, TilePicker, 0, 0, world.TilesWide, world.TilesHigh));
        await UpdateRenderWorldUsingFilterAsync();
        await RebuildFilterOverlayAsync();
        RefreshPoints();
        MinimapImage = TEdit.Render.RenderMiniMap.Render(world, true,
            TEdit.Configuration.UserSettingsService.Current.MinimapBackground);
    }

    public IDisposable? BeginWorldRead(string name)
    {
        if (_manualOperation != null && Application.Current?.Dispatcher.CheckAccess() == true)
        {
            WorldOperationCoordinator.Lease? lease;
            OperationRejection result;
            using (_manualOperation.Enter())
                result = WorldOperations.TryAcquire(false, null, out lease, name, allowReentry: true);
            return result == OperationRejection.None ? new EditorOperationScope(lease!) : null;
        }
        return BeginWorldOperation(name, mutation: false);
    }

    public IDisposable? BeginWorldOperation(string name, bool mutation = true, bool allowNoWorld = false)
    {
        WorldOperationCoordinator.Lease? lease = null;
        OperationRejection Acquire() => WorldOperations.TryAcquire(mutation, null, out lease, name, allowNoWorld, allowReentry: true);
        var dispatcher = Application.Current?.Dispatcher;
        // Non-owner callers must enter through the asynchronous UI capture boundary.
        // Acquiring on a worker could race an unleased property binding on the UI;
        // blocking that worker on Dispatcher.Invoke could deadlock a UI caller.
        if (!WorldOperations.IsOwner && dispatcher != null && !dispatcher.CheckAccess()) return null;
        var result = Acquire();
        return result == OperationRejection.None ? new EditorOperationScope(lease!) : null;
    }

    private sealed class EditorOperationScope : IDisposable
    {
        private readonly WorldOperationCoordinator.Lease _lease;
        private readonly IDisposable _ownership;
        public EditorOperationScope(WorldOperationCoordinator.Lease lease)
        {
            _lease = lease;
            _ownership = lease.Enter();
        }
        public void Dispose()
        {
            _ownership.Dispose();
            _lease.Dispose();
        }
    }

    public ScriptExecutionService ScriptExecution => LazyInitializer.EnsureInitialized(
        ref _scriptExecution,
        () => new ScriptExecutionService(new WorldViewModelScriptHost(this, Application.Current.Dispatcher), WorldOperations));
}
