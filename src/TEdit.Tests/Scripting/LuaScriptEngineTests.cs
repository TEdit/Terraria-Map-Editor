using Shouldly;
using TEdit.Editor;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;
using Xunit;

namespace TEdit.Tests.Scripting;

public class LuaScriptEngineTests
{
    private readonly LuaScriptEngine _engine = new();

    private (ScriptApi api, ScriptExecutionContext context) CreateTestContext()
    {
        var context = new ScriptExecutionContext { CancellationToken = CancellationToken.None };
        var api = new ScriptApi(TestWorldFactory.CreateWorldWithTerrain(), new NoOpUndoManager(), new Selection(), context);
        return (api, context);
    }

    [Theory]
    [InlineData("require('os')")]
    [InlineData("load('return 1')()")]
    public void Execute_SandboxedGlobal_IsUnavailable(string code)
    {
        var (api, context) = CreateTestContext();

        _engine.Execute(code, api, context).Success.ShouldBeFalse();
    }

    [Fact]
    public void Execute_SimpleScript_Succeeds()
    {
        var (api, context) = CreateTestContext();

        var result = _engine.Execute("local x = math.floor(2.5)", api, context);
        result.Error.ShouldBeNull();
    }
}
