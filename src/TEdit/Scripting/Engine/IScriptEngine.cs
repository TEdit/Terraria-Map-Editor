using TEdit.Scripting.Api;

namespace TEdit.Scripting.Engine;

public interface IScriptEngine
{
    string Name { get; }
    string FileExtension { get; }
    ScriptResult Execute(string code, ScriptApi api, ScriptExecutionContext context);
}
