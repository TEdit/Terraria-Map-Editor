using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TEdit.Mcp;

internal enum AgentProduct { VsCode, CopilotCli, ClaudeCode, ClaudeDesktop, Antigravity, CodexCli, CodexDesktop, Generic }

internal sealed record AgentSetup(string Target, string Preview, string Extension)
{
    internal void ExportNewFile(string path)
    {
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using var writer = new StreamWriter(file);
        writer.WriteLine(Preview);
    }

    internal static string Name(AgentProduct product) => product switch
    {
        AgentProduct.VsCode => "GitHub Copilot (VS Code)",
        AgentProduct.CopilotCli => "GitHub Copilot CLI",
        AgentProduct.ClaudeCode => "Claude Code",
        AgentProduct.ClaudeDesktop => "Claude Desktop",
        AgentProduct.Antigravity => "Antigravity IDE",
        AgentProduct.CodexCli => "Codex CLI",
        AgentProduct.CodexDesktop => "Codex desktop",
        _ => "MCP"
    };

    internal static bool SupportsWorkspace(AgentProduct p) => p is AgentProduct.VsCode or AgentProduct.ClaudeCode
        or AgentProduct.Antigravity or AgentProduct.CodexCli or AgentProduct.CodexDesktop;

    internal static AgentSetup Create(AgentProduct p, bool workspace, bool stdio, string endpoint, string executable, Guid? profile)
    {
        if (workspace && !SupportsWorkspace(p) || !stdio && (profile.HasValue || p == AgentProduct.ClaudeDesktop))
            throw new ArgumentException();
        if (stdio && !Path.IsPathFullyQualified(executable)) throw new ArgumentException(nameof(executable));

        string[] args = profile is Guid id ? [AgentBridge.Argument, endpoint, "--profile", id.ToString("N")] : [AgentBridge.Argument, endpoint];
        string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
        if (p is AgentProduct.CodexCli or AgentProduct.CodexDesktop)
        {
            static string Toml(string text) => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            string config = stdio ? $"command = {Toml(executable)}\nargs = [{string.Join(", ", args.Select(Toml))}]" : $"url = {Toml(endpoint)}";
            return new(workspace ? ".codex/config.toml" : "~/.codex/config.toml", "[mcp_servers.tedit]\n" + config + (stdio ? "\nstartup_timeout_sec = 30" : ""), "toml");
        }
        if (p == AgentProduct.ClaudeCode && !workspace)
        {
            string Quote(string s) => "'" + s.Replace("'", "''") + "'";
            return new("PowerShell", stdio ? $"claude mcp add --scope user --transport stdio tedit -- {Quote(executable)} {string.Join(" ", args.Select(Quote))}"
                : $"claude mcp add --scope user --transport http tedit {Quote(endpoint)}", "ps1");
        }

        var entry = new Dictionary<string, object>();
        if (p is AgentProduct.VsCode or AgentProduct.ClaudeCode or AgentProduct.CopilotCli)
            entry["type"] = stdio ? (p == AgentProduct.CopilotCli ? "local" : "stdio") : "http";
        if (stdio) { entry["command"] = executable; entry["args"] = args; }
        else entry[p == AgentProduct.Antigravity ? "serverUrl" : "url"] = endpoint;
        if (p == AgentProduct.CopilotCli) entry["tools"] = new[] { "*" };
        string target = p switch
        {
            AgentProduct.VsCode => workspace ? ".vscode/mcp.json" : "VS Code · MCP: Open User Configuration",
            AgentProduct.CopilotCli => "~/.copilot/mcp-config.json",
            AgentProduct.ClaudeCode => ".mcp.json",
            AgentProduct.ClaudeDesktop => "Claude Desktop · claude_desktop_config.json",
            AgentProduct.Antigravity => workspace ? ".agents/mcp_config.json" : "~/.gemini/config/mcp_config.json",
            _ => "MCP"
        };
        var servers = new { tedit = entry };
        return new(target, p == AgentProduct.Generic ? Json(entry) : p == AgentProduct.VsCode ? Json(new { servers }) : Json(new { mcpServers = servers }), "json");
    }
}
