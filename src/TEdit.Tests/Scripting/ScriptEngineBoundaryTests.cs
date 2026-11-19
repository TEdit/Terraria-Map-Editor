using System.Globalization;
using Shouldly;
using TEdit.Editor;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using TEdit.Scripting.Execution;
using Xunit;

namespace TEdit.Tests.Scripting;

public class ScriptEngineBoundaryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WorkerReportsDenialInSelectedUiLanguage(bool lua)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var context = new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.InspectOnly };
            using var api = new ScriptApi(TestWorldFactory.CreateSmallWorld(), new NoOpUndoManager(), new Selection(), context);
            IScriptEngine engine = lua ? new LuaScriptEngine() : new JintScriptEngine();
            var result = await Task.Run(() => engine.Execute("print('test')", api, context));
            result.Success.ShouldBeFalse();
            var french = Properties.Language.ResourceManager.GetResourceSet(
                CultureInfo.CurrentUICulture, true, false)!.GetString("scripting_inspect");
            french.ShouldNotBeNullOrWhiteSpace();
            result.Error.ShouldBe(french);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Theory]
    [InlineData(false, ScriptAccessProfile.InspectOnly)]
    [InlineData(true, ScriptAccessProfile.InspectOnly)]
    [InlineData(false, ScriptAccessProfile.McpEditing)]
    [InlineData(true, ScriptAccessProfile.McpEditing)]
    public void NonlocalProfilesRejectIndirectHostAccessBeforeEvaluation(bool lua, ScriptAccessProfile profile)
    {
        var touched = false;
        var context = new ScriptExecutionContext { AccessProfile = profile, OnLog = _ => touched = true };
        using var api = new ScriptApi(TestWorldFactory.CreateSmallWorld(), new NoOpUndoManager(), new Selection(), context);
        IScriptEngine engine = lua ? new LuaScriptEngine() : new JintScriptEngine();
        foreach (var code in new[] { "print('evaluated'); tools.getType().Assembly", "print('evaluated'); metadata.getType()", "print('evaluated'); world.title = 'changed'" })
        {
            engine.Execute(code, api, context).Success.ShouldBeFalse();
            touched.ShouldBeFalse();
        }
    }

    [Theory]
    [InlineData(false, ScriptAccessProfile.InspectOnly)]
    [InlineData(true, ScriptAccessProfile.InspectOnly)]
    [InlineData(false, ScriptAccessProfile.McpEditing)]
    [InlineData(true, ScriptAccessProfile.McpEditing)]
    public void DirectEngineCallsCannotBypassProfileDenial(bool lua, ScriptAccessProfile profile)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var context = new ScriptExecutionContext { AccessProfile = profile };
        using var api = new ScriptApi(world, new NoOpUndoManager(), new Selection(), context);
        IScriptEngine engine = lua ? new LuaScriptEngine() : new JintScriptEngine();
        var result = engine.Execute("tile.setType(2, 3, 1)", api, context);
        result.Success.ShouldBeFalse();
        result.Error!.ShouldBe(profile == ScriptAccessProfile.InspectOnly
            ? Properties.Language.scripting_inspect : Properties.Language.scripting_mcp);
        world.Tiles[2, 3].IsActive.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LogsProgressAndFinderCallbacksArePreserved(bool lua)
    {
        var events = new List<string>();
        var context = new ScriptExecutionContext
        {
            AccessProfile = ScriptAccessProfile.LocalSidebar,
            OnLog = s => events.Add(s),
            OnWarn = s => events.Add(s),
            OnError = s => events.Add(s),
            OnProgress = p => { p.ShouldBe(0.5); events.Add("progress"); },
            OnFindClear = () => events.Add("clear"),
            OnFindNavigate = n => events.Add($"navigate:{n}"),
            OnFindResult = (name, _, _, _, _) => events.Add(name)
        };
        using var api = new ScriptApi(TestWorldFactory.CreateSmallWorld(), new NoOpUndoManager(), new Selection(), context);
        IScriptEngine engine = lua ? new LuaScriptEngine() : new JintScriptEngine();
        var result = engine.Execute("print('print'); log.print('info'); log.warn('warn'); log.error('error'); " +
            "log.progress(0.5); finder.clear(); finder.addResult('found', 1, 2, 'tile', 'detail'); finder.navigateFirst()", api, context);
        result.Success.ShouldBeTrue(result.Error);
        events.ShouldBe(new[] { "print", "info", "warn", "error", "progress", "clear", "found", "navigate:0" });
    }

    [Theory]
    [InlineData("batch.forEachTile(function(x,y) while true do end end)")]
    [InlineData("batch.findTiles(function(x,y) while true do end return true end)")]
    public async Task LuaCallbacksObserveTimeout(string source)
    {
        var context = new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar, TimeoutMs = 100 };
        using var api = new ScriptApi(TestWorldFactory.CreateSmallWorld(), new NoOpUndoManager(), new Selection(), context);
        // Bounded test wait: a regression must fail rather than block the test runner.
        var result = await Task.Run(() => new LuaScriptEngine().Execute(source, api, context))
            .WaitAsync(TimeSpan.FromSeconds(5));
        result.Success.ShouldBeFalse();
        result.Error!.ShouldBe(string.Format(Properties.Language.scripting_timeout, context.TimeoutMs));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RuntimeFailureRetainsUndoRecordsForPartialChanges(bool lua)
    {
        var world = TestWorldFactory.CreateSmallWorld();
        var undo = new NoOpUndoManager();
        var context = new ScriptExecutionContext { AccessProfile = ScriptAccessProfile.LocalSidebar };
        using var api = new ScriptApi(world, undo, new Selection(), context);
        IScriptEngine engine = lua ? new LuaScriptEngine() : new JintScriptEngine();
        var source = "tile.setType(2,3,1); " + (lua ? "error('intentional failure')" : "throw new Error('intentional failure')");
        var result = engine.Execute(source, api, context);
        result.Success.ShouldBeFalse();
        world.Tiles[2, 3].Type.ShouldBe((ushort)1);
        undo.SavedTiles.ShouldContain((2, 3));
    }
}
