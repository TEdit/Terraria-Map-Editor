using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TEdit.Scripting;
using TEdit.Scripting.Execution;

namespace TEdit.Mcp.Scripting;

[McpServerToolType]
internal sealed class ScriptTools(McpServerService server, ScriptHost host, IHttpContextAccessor http)
{
    [McpServerTool(Name = "execute_script", UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("Run a bounded JavaScript or Lua script against the inspected world. See get_script_reference for examples and available functions. Recommend a separate backup before the first edit of each session. Use get_status.nextRequestId as requestId and poll get_operation for logs and diagnostics.")]
    public CallToolResult Execute(Guid instanceId, Guid requestId, Guid worldId, long worldRevision,
        string language, string source)
    {
        if (!server.Preferences.AllowEdits || !server.Preferences.AllowScripts ||
            http.HttpContext?.Items[typeof(McpProfiles)] is not Guid owner)
            return InspectionResult.Error("editing_disabled").ToProtocol();
        if (language is not ("javascript" or "lua")) return InspectionResult.Error("invalid_language").ToProtocol();
        if (string.IsNullOrWhiteSpace(source) || Encoding.UTF8.GetByteCount(source) > ScriptProtocol.SourceBytes)
            return InspectionResult.Error("invalid_source").ToProtocol();
        var expected = new WorldRevision(worldId, worldRevision);
        var input = new ScriptInput(language, source);
        var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { expected, input })));
        var admission = server.Operations.StartEdit(owner, instanceId, requestId, hash, ct => host.Run(input, expected, ct));
        return (admission.ErrorCode is string code ? InspectionResult.Error(code) : InspectionResult.Ok(admission.Operation!)).ToProtocol();
    }

    [McpServerTool(Name = "get_script_reference", ReadOnly = true, UseStructuredContent = true, OutputSchemaType = typeof(InspectionResult))]
    [Description("List the restricted MCP script API, limits, and runnable inspection, drawing and composition examples. Coordinates are inclusive. Drawing returns the number of tile writes. log.print accepts a string, number or boolean.")]
    public CallToolResult Reference(string language)
    {
        if (language is not ("javascript" or "lua")) return InspectionResult.Error("invalid_language").ToProtocol();
        var functions = ScriptWorker.Api.SelectMany(m => ScriptApiMetadata.Modules.Single(x => x.Name == m.Module)
            .Methods.Where(f => m.Methods.Contains(f.Name)).Select(f => new { module = m.Module, f.Signature }));
        return InspectionResult.Ok(new
        {
            functions,
            partialEdits = "Changes made before failure or cancellation remain in one undo step. Poll until terminal, then call get_status before another edit.",
            limits = new { startupMs = ScriptProtocol.StartupMs, recursionDepth = ScriptProtocol.RecursionLimit,
                instructionsOrStatements = ScriptProtocol.InstructionLimit, allocationBytes = ScriptProtocol.EngineMemoryBytes,
                workerCommitBytes = ScriptProtocol.ProcessMemoryBytes, sourceBytes = ScriptProtocol.SourceBytes, timeoutMs = ScriptProtocol.TimeoutMs,
                hostCalls = ScriptProtocol.CallLimit, candidateTiles = ScriptProtocol.WorkLimit, logChars = ScriptProtocol.MaxLogChars },
            examples = new
            {
                inspect = language == "lua"
                    ? "for x = 10, 12 do log.print(tile.getWall(x, 10)) end"
                    : "for (let x = 10; x <= 12; x++) log.print(tile.getWall(x, 10));",
                draw = "draw.rectangle(10,10,14,14,false,1,'tile',1,false,-1)",
                compose = "draw.rectangle(10,10,14,14,false,1,'tile',1,false,-1)\ndraw.fillRegion(11,11,13,13,'wall',1,false,-1)\nlog.print(tile.getWall(12,12))"
            }
        }).ToProtocol();
    }
}
