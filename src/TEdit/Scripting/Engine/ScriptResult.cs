namespace TEdit.Scripting.Engine;

public record ScriptResult(bool Success, string? Error, long ElapsedMs);
