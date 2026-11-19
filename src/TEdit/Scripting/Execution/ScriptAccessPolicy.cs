namespace TEdit.Scripting.Execution;

public enum ScriptAccessProfile { Unspecified, LocalSidebar, InspectOnly, McpEditing }

public static class ScriptAccessPolicy
{
    // Only the local sidebar may bind the full ScriptApi. MCP evaluates source in its
    // isolated worker and exposes the audited scalar operations from ScriptWorker.Api.
    public static string? GetDenial(ScriptAccessProfile profile) => profile switch
    {
        ScriptAccessProfile.Unspecified => Properties.Language.scripting_profile,
        ScriptAccessProfile.LocalSidebar => null,
        ScriptAccessProfile.InspectOnly => Properties.Language.scripting_inspect,
        ScriptAccessProfile.McpEditing => Properties.Language.scripting_mcp,
        _ => Properties.Language.scripting_unknown
    };
}
