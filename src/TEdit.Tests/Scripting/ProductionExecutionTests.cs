using System.Windows;
using System.Windows.Threading;
using System.Reactive.Threading.Tasks;
using System.IO;
using Shouldly;
using TEdit.Render;
using TEdit.Scripting.Engine;
using TEdit.Scripting.Execution;
using TEdit.ViewModel;
using Xunit;

namespace TEdit.Tests.Scripting;

[CollectionDefinition("Production editor", DisableParallelization = true)]
public class ProductionEditorCollection { }

[Collection("Production editor")]
public class ProductionExecutionTests
{
    internal static readonly Lazy<Dispatcher> Ui = new(() =>
    {
        var ready = new TaskCompletionSource<Dispatcher>();
        var thread = new Thread(() =>
        {
            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            App.Version = new Semver.SemVersion(0, 0, 0);
            ready.SetResult(app.Dispatcher);
            Dispatcher.Run();
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return ready.Task.WaitAsync(TimeSpan.FromSeconds(15)).GetAwaiter().GetResult();
    });

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task RealHostCommitsPartialEditsRendersAndSupportsUndoRedo(bool lua, bool fail)
    {
        await Ui.Value.InvokeAsync(async () =>
        {
            var world = ExecutionWorldFixture.Create();
            var vm = new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = world };
            try
            {
                vm.PixelMap = new PixelMapManager();
                vm.PixelMap.InitializeBuffers(world.TilesWide, world.TilesHigh);
                await vm.UpdateRenderWorldAsync();
                var original = world.Tiles[0, 0].Type;
                var originalColor = vm.PixelMap.GetPixelColor(0, 0);
                world.Tiles[0, 0].uvTileCache = 7;
                world.Tiles[0, 0].uvWallCache = 9;
                var revision = vm.WorldOperations.GetRevision(world)!.Value;
                TEdit.Terraria.WorldConfiguration.Sprites2.First(s => s.Tile == 21).Styles.First(s => s.Style == 0).SizeTiles.X.ShouldBeGreaterThan((short)1);
                var spriteCode = lua
                    ? "if not sprites.place(21,0,5,5) then error('sprite failed') end; if sprites.place(21,0,99,99) then error('clipped sprite allowed') end;"
                    : "if (!sprites.place(21,0,5,5)) throw new Error('sprite failed'); if (sprites.place(21,0,99,99)) throw new Error('clipped sprite allowed');";
                var code = "tile.setType(0, 0, 2);" + spriteCode + (fail ? (lua ? "error('partial')" : "throw new Error('partial')") : "");
                var run = await Task.Run(() => vm.ScriptExecution.ExecuteAsync(lua ? new LuaScriptEngine() : new JintScriptEngine(), code,
                    new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar }));
                run.Started.ShouldBeTrue();
                run.Script.Success.ShouldBe(!fail, run.Script.Error);
                world.Tiles[0, 0].Type.ShouldBe((ushort)2);
                world.Tiles[0, 0].uvTileCache.ShouldBe(ushort.MaxValue);
                world.Tiles[0, 0].uvWallCache.ShouldBe(ushort.MaxValue);
                vm.MinimapImage.ShouldNotBeNull();
                world.Chests.ShouldContain(c => c.X == 5 && c.Y == 5);
                world.Tiles[99, 99].Type.ShouldBe((ushort)1);
                vm.UndoManager.CurrentIndex.ShouldBe(1);
                using (var flushed = File.Open(Path.Combine(vm.UndoManager.CacheDirectory, "undo_temp_0"), FileMode.Open, FileAccess.Read, FileShare.None))
                    flushed.Length.ShouldBeGreaterThan(0);
                vm.PixelMap.GetPixelColor(0, 0).ShouldNotBe(originalColor);
                vm.WorldOperations.IsBusy.ShouldBeFalse();
                vm.WorldOperations.GetRevision(world)!.Value.Revision.ShouldBeGreaterThan(revision.Revision);
                await vm.UndoCommand.Execute().ToTask();
                world.Tiles[0, 0].Type.ShouldBe(original);
                world.Chests.ShouldNotContain(c => c.X == 5 && c.Y == 5);
                vm.PixelMap.GetPixelColor(0, 0).ShouldBe(originalColor);
                await vm.RedoCommand.Execute().ToTask();
                world.Tiles[0, 0].Type.ShouldBe((ushort)2);
                world.Chests.ShouldContain(c => c.X == 5 && c.Y == 5);
            }
            finally { vm.CurrentWorld = null!; }
        }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task BusyScriptRejectsManualUndoPropertiesAndReplacementThenCancellationFinalizes()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await Ui.Value.InvokeAsync(async () =>
        {
            var world = ExecutionWorldFixture.Create();
            var vm = new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = world };
            vm.PixelMap = new PixelMapManager();
            vm.PixelMap.InitializeBuffers(100, 100);
            var autosavePath = Path.Combine(WorldViewModel.AutoSavePath, $"mcp-test-{Guid.NewGuid():N}.autosave");
            vm.ActiveTool = new TestStrokeTool(vm, false);
            vm.CurrentFile = Path.ChangeExtension(autosavePath, ".wld");
            world.Title += " edited";
            vm.IsAutoSaveEnabled = true;
            var autosaveTick = typeof(WorldViewModel).GetMethod("SaveTimerTick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            try
            {
                var run = vm.ScriptExecution.ExecuteAsync(new PausedEngine(entered), "test",
                    new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar });
                try
                {
                    await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                    vm.IsWorldInteractionEnabled.ShouldBeFalse();
                    vm.BeginWorldOperation("Competing save").ShouldBeNull();
                    vm.BeginWorldRead("Competing read").ShouldBeNull();
                    var title = world.Title;
                    Should.Throw<InvalidOperationException>(() => world.Title = "uncoordinated");
                    world.Title.ShouldBe(title);
                    Should.Throw<InvalidOperationException>(() => vm.CurrentWorld = ExecutionWorldFixture.Create());
                    vm.CurrentWorld.ShouldBeSameAs(world);
                    vm.MouseDownTile(new TEdit.UI.TileMouseState { Location = new(5, 5), LeftButton = System.Windows.Input.MouseButtonState.Pressed });
                    vm.WorldOperations.ActiveName.ShouldBe("Script");
                    autosaveTick.Invoke(vm, new object?[] { null, null });
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    File.Exists(autosavePath).ShouldBeFalse();
                    await vm.UndoCommand.Execute().ToTask();
                    world.Tiles[0, 0].Type.ShouldBe((ushort)2);
                    await vm.SaveCommand.Execute().ToTask(); // Must return before opening a Save As dialog.
                    (await Task.Run(() => vm.BeginWorldOperation("Unowned worker"))).ShouldBeNull();
                    var revision = vm.WorldOperations.GetRevision(world);
                    // Inventory-linked entry points: arguments are deliberately empty
                    // because busy rejection must happen before dialogs or argument use.
                    foreach (var name in new[] { "EditDelete", "EditCut", "EditPaste", "CropWorldAsync", "ExpandWorld",
                        "NpcAdd", "NpcRemove", "ExecuteSaveTileEntity", "SaveXmasTree", "ExecuteSaveSign", "SaveChest",
                        "PasteAccept", "PasteCancel", "PasteRotateCW", "PasteRotateCCW", "PasteFlipH", "PasteFlipV",
                        "ClearTEditCache", "NewWorld", "ReloadWorldAsync", "LoadWorldAsync", "SaveWorldAsAsync" })
                    {
                        var method = typeof(WorldViewModel).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                            .Single(m => m.Name == name);
                        var args = method.GetParameters().Select(p => p.HasDefaultValue ? p.DefaultValue : p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null).ToArray();
                        var returned = method.Invoke(vm, args);
                        if (returned is Task task) await task;
                        vm.WorldOperations.GetRevision(world).ShouldBe(revision, name);
                        vm.CurrentWorld.ShouldBeSameAs(world, name);
                    }
                }
                finally { vm.WorldOperations.CancelActive(); }
                var result = await run;
                result.Script.Success.ShouldBeFalse();
                result.Script.Error.ShouldBe(Properties.Language.scripting_cancel);
                vm.UndoManager.CurrentIndex.ShouldBe(1);
                vm.WorldOperations.IsBusy.ShouldBeFalse();
                var afterScript = vm.WorldOperations.GetRevision(world);
                var untouched = world.Tiles[1, 1];
                var held = new TEdit.UI.TileMouseState { Location = new(1, 1), LeftButton = System.Windows.Input.MouseButtonState.Pressed };
                vm.MouseMoveTile(held);
                vm.MouseUpTile(new TEdit.UI.TileMouseState { Location = held.Location });
                world.Tiles[1, 1].ShouldBe(untouched);
                vm.WorldOperations.GetRevision(world).ShouldBe(afterScript);
                vm.UndoManager.CurrentIndex.ShouldBe(1);
                autosaveTick.Invoke(vm, new object?[] { null, null });
                for (int i = 0; i < 100 && (!File.Exists(autosavePath) || vm.WorldOperations.IsBusy); i++)
                    await Task.Delay(20);
                File.Exists(autosavePath).ShouldBeTrue();
                vm.WorldOperations.IsBusy.ShouldBeFalse();
                vm.HasUnsavedChanges.ShouldBeFalse();
                await vm.UndoCommand.Execute().ToTask();
                world.Tiles[0, 0].Type.ShouldBe((ushort)1);
            }
            finally { vm.IsAutoSaveEnabled = false; vm.CurrentWorld = null!; File.Delete(autosavePath); }
        }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));
    }

    private sealed class PausedEngine(TaskCompletionSource entered) : IScriptEngine
    {
        public string Name => "Paused test engine";
        public string FileExtension => ".test";
        public ScriptResult Execute(string code, TEdit.Scripting.Api.ScriptApi api, ScriptExecutionContext context)
        {
            api.Tile.SetType(0, 0, 2);
            entered.SetResult();
            context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
            context.CancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException("Test did not cancel the worker.");
        }
    }

    [Fact]
    public async Task ManualStrokeAllowsOwnedRenderingAndReleasesOnFocusLoss()
    {
        await Ui.Value.InvokeAsync(async () =>
        {
            var world = ExecutionWorldFixture.Create();
            var vm = new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = world };
            vm.PixelMap = new PixelMapManager();
            vm.PixelMap.InitializeBuffers(100, 100);
            try
            {
                vm.MouseDownTile(new TEdit.UI.TileMouseState { Location = new(1, 1), LeftButton = System.Windows.Input.MouseButtonState.Pressed });
                vm.WorldOperations.IsBusy.ShouldBeTrue();
                vm.WorldOperations.IsOwner.ShouldBeFalse();
                using (vm.BeginWorldRead("Viewport render"))
                {
                    vm.WorldOperations.IsOwner.ShouldBeTrue();
                    using var nested = vm.BeginWorldRead("Nested render");
                    nested.ShouldNotBeNull();
                }
                vm.WorldOperations.IsOwner.ShouldBeFalse();
                var run = await vm.ScriptExecution.ExecuteAsync(new JintScriptEngine(), "tile.clear(1, 1)",
                    new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar });
                run.Rejection.ShouldBe(OperationRejection.Busy);
                vm.CompleteManualStroke();
                vm.WorldOperations.IsBusy.ShouldBeFalse();
                vm.WorldOperations.IsOwner.ShouldBeFalse();
                vm.BeginWorldOperation("Next edit")!.Dispose();
            }
            finally { vm.CompleteManualStroke(); vm.CurrentWorld = null!; }
        }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task AsyncPluginRetainsOwnershipAndSavedWorldContainsCompletedEdits()
    {
        var directory = Path.Combine(Path.GetTempPath(), "tedit-production-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await Ui.Value.InvokeAsync(async () =>
            {
                var world = ExecutionWorldFixture.Create();
                var vm = new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = world, CurrentFile = Path.Combine(directory, "edited.wld") };
                vm.PixelMap = new PixelMapManager();
                vm.PixelMap.InitializeBuffers(100, 100);
                var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var plugin = new AwaitedPlugin(vm, release.Task);
                try
                {
                    var execution = plugin.ExecuteAsync();
                    vm.WorldOperations.IsBusy.ShouldBeTrue();
                    vm.WorldOperations.IsOwner.ShouldBeFalse();
                    await vm.SaveCommand.Execute().ToTask();
                    File.Exists(vm.CurrentFile).ShouldBeFalse();
                    release.SetResult();
                    await execution;
                    vm.WorldOperations.IsBusy.ShouldBeFalse();
                    world.Tiles[99, 99].Type.ShouldBe((ushort)2);
                    await vm.SaveCommand.Execute().ToTask();
                    var (loaded, error) = TEdit.Terraria.World.LoadWorld(vm.CurrentFile);
                    error.ShouldBeNull();
                    loaded!.Tiles[99, 99].Type.ShouldBe((ushort)2);
                    vm.HasUnsavedChanges.ShouldBeFalse();
                }
                finally { release.TrySetResult(); vm.CurrentWorld = null!; }
            }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private sealed class AwaitedPlugin(WorldViewModel vm, Task release) : TEdit.Editor.Plugins.BasePlugin(vm)
    {
        protected override async Task ExecuteCoreAsync()
        {
            await release;
            _wvm.WorldOperations.CanMutate.ShouldBeTrue();
            _wvm.UndoManager.SaveTile(99, 99);
            _wvm.CurrentWorld.Tiles[99, 99].Type = 2;
            await _wvm.UndoManager.SaveUndoAsync();
            await _wvm.UpdateRenderWorldAsync();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ManualToolCommitsOnFocusLossAndRecoversFromFailure(bool fail)
    {
        await Ui.Value.InvokeAsync(async () =>
        {
            var world = ExecutionWorldFixture.Create();
            var vm = new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = world };
            vm.PixelMap = new PixelMapManager();
            vm.PixelMap.InitializeBuffers(100, 100);
            vm.ActiveTool = new TestStrokeTool(vm, fail);
            try
            {
                var down = new TEdit.UI.TileMouseState { Location = new(0, 0), LeftButton = System.Windows.Input.MouseButtonState.Pressed };
                if (fail) Should.Throw<InvalidOperationException>(() => vm.MouseDownTile(down));
                else { vm.MouseDownTile(down); vm.CompleteManualStroke(); }
                world.Tiles[0, 0].Type.ShouldBe((ushort)2);
                vm.WorldOperations.IsBusy.ShouldBeFalse();
                vm.UndoManager.CurrentIndex.ShouldBe(1);
                await vm.UndoCommand.Execute().ToTask();
                world.Tiles[0, 0].Type.ShouldBe((ushort)1);
            }
            finally { vm.CompleteManualStroke(); vm.CurrentWorld = null!; }
        }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));
    }

    private sealed class TestStrokeTool(WorldViewModel vm, bool fail) : TEdit.Editor.Tools.BaseTool(vm)
    {
        public override void MouseMove(TEdit.UI.TileMouseState state) => MouseDown(state);
        public override void MouseDown(TEdit.UI.TileMouseState state)
        {
            _wvm.WorldOperations.CanMutate.ShouldBeTrue();
            _wvm.UndoManager.SaveTile(state.Location);
            _wvm.CurrentWorld.Tiles[state.Location.X, state.Location.Y].Type = 2;
            if (fail) throw new InvalidOperationException("Partial tool failure");
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LocalLoadRebindsSavedModuleAndMethodReferences(bool lua)
    {
        var directory = Path.Combine(Path.GetTempPath(), "tedit-load-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "replacement.wld");
        await TEdit.Terraria.World.SaveAsync(ExecutionWorldFixture.Create(200, 100), path, incrementRevision: false);
        try
        {
            await Ui.Value.InvokeAsync(async () =>
            {
                var original = ExecutionWorldFixture.Create();
                var vm = new WorldViewModel { IsAutoSaveEnabled = false, CurrentWorld = original };
                vm.PixelMap = new PixelMapManager();
                vm.PixelMap.InitializeBuffers(100, 100);
                try
                {
                    var literal = System.Text.Json.JsonSerializer.Serialize(path.Replace('\\', '/'));
                    var nestedCode = lua
                        ? $"batch.forEachTile(function(x,y) if x == 0 and y == 0 then if tools.load({literal}) then error('nested load allowed') end end end)"
                        : $"batch.forEachTile(function(x,y) {{ if (x === 0 && y === 0 && tools.load({literal})) throw new Error('nested load allowed'); }});";
                    var nested = await vm.ScriptExecution.ExecuteAsync(lua ? new LuaScriptEngine() : new JintScriptEngine(), nestedCode,
                        new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar });
                    nested.Script.Success.ShouldBeTrue(nested.Script.Error);
                    vm.CurrentWorld.ShouldBeSameAs(original);
                    var code = lua
                        ? $"local saved = tile; local set = tile.setType; if not tools.load({literal}) then error('load failed') end; set(199,99,2); saved.setWall(199,99,2)"
                        : $"var saved = tile; var set = tile.setType; if (!tools.load({literal})) throw new Error('load failed'); set(199,99,2); saved.setWall(199,99,2);";
                    var result = await vm.ScriptExecution.ExecuteAsync(lua ? new LuaScriptEngine() : new JintScriptEngine(), code,
                        new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar });
                    result.Script.Success.ShouldBeTrue(result.Script.Error);
                    vm.CurrentWorld.TilesWide.ShouldBe(200);
                    vm.CurrentWorld.Tiles[199, 99].Type.ShouldBe((ushort)2);
                    vm.PixelMap.TilesX.ShouldBeGreaterThan(0);
                    original.Tiles[99, 99].Type.ShouldBe((ushort)1);
                    vm.UndoManager.CurrentIndex.ShouldBe(1);
                    var badPath = System.Text.Json.JsonSerializer.Serialize(Path.Combine(directory, "bad|name.wld").Replace('\\', '/'));
                    var failedSave = await vm.ScriptExecution.ExecuteAsync(lua ? new LuaScriptEngine() : new JintScriptEngine(),
                        lua ? $"if tools.saveAs({badPath}) then error('invalid save succeeded') end" : $"if (tools.saveAs({badPath})) throw new Error('invalid save succeeded');",
                        new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar });
                    failedSave.Script.Success.ShouldBeTrue(failedSave.Script.Error);
                    Path.GetFullPath(vm.CurrentFile).ShouldBe(Path.GetFullPath(path));
                    var savedPath = Path.Combine(directory, "saved.wld");
                    var savedLiteral = System.Text.Json.JsonSerializer.Serialize(savedPath.Replace('\\', '/'));
                    var saved = await vm.ScriptExecution.ExecuteAsync(lua ? new LuaScriptEngine() : new JintScriptEngine(),
                        lua ? $"if not tools.saveAs({savedLiteral}) then error('save failed') end" : $"if (!tools.saveAs({savedLiteral})) throw new Error('save failed');",
                        new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar });
                    saved.Script.Success.ShouldBeTrue(saved.Script.Error);
                    vm.HasUnsavedChanges.ShouldBeFalse();
                    var (reloaded, error) = TEdit.Terraria.World.LoadWorld(savedPath);
                    error.ShouldBeNull();
                    reloaded.Tiles[199, 99].Type.ShouldBe((ushort)2);
                    await vm.UndoCommand.Execute().ToTask();
                    vm.CurrentWorld.Tiles[199, 99].Type.ShouldBe((ushort)1);
                }
                finally { vm.CurrentWorld = null!; }
            }).Task.Unwrap().WaitAsync(TimeSpan.FromSeconds(30));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
