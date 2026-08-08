# Windows Phone Ghidra extracts

These files are raw Ghidra decompilations from the local Windows Phone Terraria
projects. They are retained so format work can cite native functions without
rerunning headless analysis or relying on terminal output.

## Directory layout

- `v1.0.0.0/` contains the legacy and compact world readers/writers, metadata
  helpers, tile codecs, chest codecs, and sign codecs from `WinPhoneTerraria1000`.
- `v1.2.4.3/` contains the container, body, tile, chest, sign, CRC, and transformed
  body functions from `WinPhoneTerraria`.
- `v1.0.0.0/world-state-xrefs/` contains durable cross-reference reports for
  progression-adjacent orb, meteor, altar, ore, hardmode, time-state, and
  invasion globals, plus decompiled callers used to identify their semantics.
- `*.c` is raw decompiler output named by function address.
- `*.calls.txt` lists direct callees for the corresponding orchestration function.

The decompiler names are not authoritative. Confirm field meanings from paired
read/write order, version gates, fixture values, and observable editor behavior.

## Key v1.0.0.0 functions

| Address | Role |
|---|---|
| `0076758c` | Write complete native world body |
| `00767f5c` | Read pre-v58 world body |
| `00768798` | Read v58+ world body |
| `0073527c` / `00735318` | Write/read compact time and moon state |
| `00749890` / `00749d20` | Write/read weather state |
| `006598b4` / `00659a38` | Write/read progression flags |
| `00733bac` | Read legacy tile records |
| `00733858` / `00733e90` | Read/write compact tile records |
| `00605a60` / `00605b9c` | Read/write one chest inventory |
| `006082a4` / `00606768` | Read/write sparse chest table |
| `00726440` / `007260ec` | Read/write one sign slot |

## Key v1.2.4.3 functions

| Address | Role |
|---|---|
| `0078d7b4` / `00789a48` | Read/write native container header |
| `0078d0d8` / `0078dc9c` | Read/write complete world body |
| `0075bb00` / `0075c298` | Read/write compact tile records |
| `005ec460` / `005e9ef8` | Read/write sparse chest entries |
| `00749380` / `0074904c` | Read/write sign entries |
| `0078e920` | Verify CRC and dispatch transformed bodies |
| `007783c8` | Decode transformed v66+ body |

## Regeneration

The workspace extraction script supports an output directory followed by
`save:` and `savecalls:` arguments:

```powershell
& 'D:\dev\ai\tedit\tools\ghidra_12.1.2_PUBLIC\support\analyzeHeadless.bat' `
  'D:\dev\ai\tedit\tools\ghidra-projects' 'WinPhoneTerraria1000' `
  -process 'WindowsPhone.exe' -noanalysis `
  -scriptPath 'D:\dev\ai\tedit\tools\ghidra-scripts' `
  -postScript WinPhoneExtract.java `
  'output:D:\dev\ai\tedit\Terraria-Map-Editor\docs\reverse-engineering\winphone\ghidra\v1.0.0.0' `
  save:0076758c savecalls:0076758c
```

Set workspace-local `APPDATA` and `LOCALAPPDATA` as described by the Ghidra
quick reference before running headless analysis.
