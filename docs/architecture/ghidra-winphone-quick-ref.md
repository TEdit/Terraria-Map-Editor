# Ghidra Windows Phone quick reference

The reverse-engineering toolchain is installed only in the enclosing workspace,
not globally.

| Component | Workspace path |
|---|---|
| Ghidra 12.1.2 | `tools/ghidra_12.1.2_PUBLIC` |
| Ghidra projects | `tools/ghidra-projects` |
| v1.2.4.3 project | `WinPhoneTerraria` |
| v1.0.0.0 project | `WinPhoneTerraria1000` |
| Extraction script | `tools/ghidra-scripts/WinPhoneExtract.java` |
| ghidra-mcp v3.0.0 | `tools/ghidra-mcp` |
| MCP virtual environment | `tools/ghidra-mcp/.venv` |

The Ghidra archive SHA-256 is
`b62e81a0390618466c019c60d8c2f796ced2509c4c1aea4a37644a77272cf99d`.

## Extract from a saved project

Set the workspace-local application directories before running headless Ghidra:

```powershell
$env:APPDATA = 'D:\dev\ai\tedit\tools\ghidra-user\appdata'
$env:LOCALAPPDATA = 'D:\dev\ai\tedit\tools\ghidra-user\localappdata'

& 'D:\dev\ai\tedit\tools\ghidra_12.1.2_PUBLIC\support\analyzeHeadless.bat' `
  'D:\dev\ai\tedit\tools\ghidra-projects' 'WinPhoneTerraria1000' `
  -process 'WindowsPhone.exe' -noanalysis `
  -scriptPath 'D:\dev\ai\tedit\tools\ghidra-scripts' `
  -postScript WinPhoneExtract.java `
  'output:D:\dev\ai\tedit\Terraria-Map-Editor\docs\reverse-engineering\winphone\ghidra\v1.0.0.0' `
  save:0076758c savecalls:0076758c
```

Extraction arguments include:

- `ADDRESS`: print a decompilation.
- `save:ADDRESS`: save a decompilation in the selected output directory.
- `calls:ADDRESS`: print direct callees.
- `savecalls:ADDRESS`: save direct callees.
- `xref:ADDRESS`: list references to an address.
- `range:START:END`: list functions in an address range.
- `string:TEXT`: find defined strings and their callers.
- `scalar:VALUE`: find instructions using a numeric value.

Durable extracts and the current function index are in
`docs/reverse-engineering/winphone/ghidra/README.md`.

## Local MCP bridge

Start the workspace server with `tools/Start-GhidraMcp.ps1`. Configure a project-
local MCP client to run:

```text
D:\dev\ai\tedit\tools\ghidra-mcp\.venv\Scripts\python.exe
D:\dev\ai\tedit\tools\ghidra-mcp\bridge_mcp_ghidra.py
--ghidra-server http://127.0.0.1:8089/
```

Do not add this server or its Python environment to global configuration.
