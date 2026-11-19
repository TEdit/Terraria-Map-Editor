# Connect an agent to TEdit

Open the world you want your agent to inspect. Before each editing session, **save a separate backup copy of the world**. TEdit also asks connected agents to remind you before their first edit.

1. In TEdit settings, expand **MCP**. Enable the server and select **Share access with all local clients without a token**. Use this mode when you trust the apps running on your computer.
2. Drawing tools are enabled for new setups. For inspection only, clear **Allow drawing tools**. To run JavaScript or Lua, also enable scripts. Save your settings.
3. Reopen settings and expand **MCP** > **Connect an agent**. Choose your client. Select workspace settings if you want the connection to apply to one project. Leave the stdio option off for HTTP. Claude Desktop uses the bundled stdio bridge automatically.
4. Select **Check connection**. TEdit calls `get_status` through the selected transport. Then copy the preview or export it to a new file. For an existing client config, merge the single `tedit` entry into its server list. If the preview shows a PowerShell command, run that command.
5. Reload the client’s MCP servers or start a new agent session. Ask **“What are the dimensions of the world open in TEdit?”** The answer should match your world. Your agent may ask you to approve individual tool calls.

Codex CLI and a generic MCP client completed the full editing and reconnect test. Copilot CLI completed editing and undo. Its reconnect check is pending. The remaining entries provide setup instructions with partial or unverified compatibility.

| Client | Where to put the preview |
| --- | --- |
| GitHub Copilot in VS Code | Run **MCP: Open User Configuration** in the Command Palette, or use `.vscode/mcp.json` in your workspace. Start the **tedit** server and select its tools in Agent mode. [VS Code instructions](https://code.visualstudio.com/docs/agent-customization/mcp-servers) |
| GitHub Copilot CLI | Merge the entry into `~/.copilot/mcp-config.json`. To test a separate JSON file for one session, run `copilot --additional-mcp-config @tedit-agent.json`. [Copilot CLI instructions](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-mcp-servers) |
| Claude Code | Run the user-scope command from the preview, or merge the workspace preview into `.mcp.json`. Use `claude mcp list` to check registration. [Claude Code instructions](https://code.claude.com/docs/en/mcp) |
| Claude Desktop | Open **Settings** > **Developer** > **Edit Config**. Merge the entry into `claude_desktop_config.json`, then quit and reopen Claude Desktop. The bridge runs from the same installation as TEdit. [Desktop instructions](https://modelcontextprotocol.io/docs/develop/connect-local-servers) |
| Antigravity IDE | In the agent panel, open **…** > **MCP Servers** > **Manage MCP Servers** > **View raw config**. Use `~/.gemini/config/mcp_config.json` for your user or `.agents/mcp_config.json` for your workspace. [Antigravity IDE instructions](https://antigravity.google/docs/mcp?tab=ide) |
| Codex CLI and Codex desktop | Add the TOML table to `~/.codex/config.toml` or to `.codex/config.toml` in a trusted project. In the CLI, check `codex mcp list`. In the desktop app, check MCP settings for the project you opened. [Codex MCP settings](https://developers.openai.com/codex/mcp/) |
| Other MCP clients | For Streamable HTTP, enter the exact URL from TEdit. For stdio, use the absolute executable path and pass each argument as a separate array element. |

## Try an edit

On a backed-up test world, ask your agent to draw a filled 2 × 2 rectangle of stone wall starting at tile (10, 10). Ask it to read those four tiles back. Check the change in TEdit, then choose **Undo** to restore them.

For a script, ask the agent to call `get_script_reference` for the language you want. These examples print wall IDs for three tiles without changing them.

**JavaScript**

```javascript
for (let x = 10; x <= 12; x++) log.print(tile.getWall(x, 10));
```

**Lua**

```lua
for x = 10, 12 do log.print(tile.getWall(x, 10)) end
```

Each drawing or script request returns an operation ID. The agent can use `get_operation` to follow it or `cancel_operation` to stop it. Wait for the final result before asking for another edit. If a script fails or you cancel it, completed changes stay in the open world as one undo step. Use **Undo** to reverse those changes. Review the world and choose **Save** when you want to write it to your world file.

## Troubleshoot a connection or edit

**If the client shows only inspection tools**, check the drawing and script permissions in TEdit settings. Save, copy the current setup, and reconnect the client to refresh its tool list.

**If an edit reports a busy or changed world**, let the current operation finish, then ask the agent to inspect the world again. If the client lost a reply, it can retry the identical request with its original IDs to find the operation it already started.

**If TEdit restarts or you change its MCP settings**, copy the updated setup from the intended editor window. The address identifies a particular running editor. A client can reconnect to that editor with the same setup.

**For separate client histories**, turn off shared access, save, and reopen settings. Create or select a profile. The setup preview uses the bridge, which reads the selected profile from protected Windows storage. For a client that accepts only HTTP, put the issued `Bearer …` value in its private authorization-header setting. Keep it out of workspace files. To rotate access, revoke the old profile, create a replacement, and update the client entry.

To disconnect a client, remove its `tedit` entry. To disconnect all clients, turn off the MCP server in TEdit and save.
