# Local TEdit diagnostics

The diagnostic spider builds the legacy WPF editor only when requested. Its process runs on a separate Windows desktop and uses same-user named-pipe IPC. The diagnostic build redirects TEdit settings, logs, cache, undo, autosave, and backup paths into a profile under `%LOCALAPPDATA%\TEdit\Diagnostics`; the generated world is a private copy of the repository's `v1.4.4.4.wld.zip` fixture. All file-bearing IPC operations enforce their profile subdirectory.

## Start and inspect

From the repository root:

```powershell
./tools/diagnostics/diagnostics.ps1 -Action start
./tools/diagnostics/diagnostics.ps1 -Action status
./tools/diagnostics/diagnostics.ps1 -Action state
```

`start` builds `src/TEdit/TEdit.csproj` with `LocalDiagnostics=true`, seeds an isolated profile and fixture, disables telemetry prompts and update checks in that profile, launches the editor on a private desktop, and verifies IPC. Calling it again reuses a responsive session. Build output is stored at `<profile>/artifacts/build.log`.

## Repeat the world edit and settings scenario

```powershell
$state = ./tools/diagnostics/diagnostics.ps1 -Action state | ConvertFrom-Json
$fixture = Join-Path $state.profile 'fixtures/diagnostic-world.wld'
$null = ./tools/diagnostics/diagnostics.ps1 -Action request -Request (@{ op = 'open-world'; path = $fixture } | ConvertTo-Json -Compress)
do {
    Start-Sleep -Seconds 1
    $state = ./tools/diagnostics/diagnostics.ps1 -Action state | ConvertFrom-Json
    if ($state.windows.title -contains 'Load Legacy World?') {
        ./tools/diagnostics/diagnostics.ps1 -Action request -Request '{"op":"resolve-dialog","title":"Load Legacy World?","choice":"Yes"}'
    }
    if ($state.operation -like 'failed:*' -or $state.operation -like 'timed-out:*') { throw $state.operation }
} while ($state.operation -notlike 'completed:open-world:*')
$x = $state.world.spawnX + 12
$y = $state.world.spawnY - 20
./tools/diagnostics/diagnostics.ps1 -Action request -Request (@{ op = 'paint-tile'; x = $x; y = $y; type = 0 } | ConvertTo-Json -Compress)
./tools/diagnostics/diagnostics.ps1 -Action request -Request (@{ op = 'inspect-tile'; x = $x; y = $y } | ConvertTo-Json -Compress)
./tools/diagnostics/diagnostics.ps1 -Action request -Request '{"op":"setting","name":"MinimapBackground","value":true}'
./tools/diagnostics/diagnostics.ps1 -Action request -Request '{"op":"save"}'
./tools/diagnostics/diagnostics.ps1 -Action screenshot
```

`paint-tile` uses `WorldEditor.SetPixel`, then `save` awaits TEdit's own `SaveWorldFileAsync`. `inspect-tile` reads back the live world model. To verify persistence, rerun the open-and-poll block above, then repeat `inspect-tile` with `$x` and `$y`; the same tile should remain active after reloading. `screenshot` saves a WPF window image and a separate actual MonoGame backbuffer readback named `*-viewport.png`; the WPF image does not contain the native XNA child window.

Supported requests are `state`, `open-world`, `resolve-dialog`, `paint-tile`, `inspect-tile`, `save`, `setting`, `screenshot`, and `shutdown`. `resolve-dialog` requires the exact visible title and one of its actual `Yes`, `No`, or `Close` buttons. `setting` currently accepts the isolated `MinimapBackground` boolean. World reads/writes are confined to `<profile>/fixtures`; screenshots are confined to `<profile>/artifacts`.

## Stop

```powershell
./tools/diagnostics/diagnostics.ps1 -Action stop
```

The session descriptor is deleted by the owned app process on normal exit. The wrapper does not kill unrelated TEdit processes or remove the profile. Preserve the profile when inspecting logs or the saved world. The native GPU capture is limited to the MonoGame backbuffer; it does not capture other windows, desktop composition, or native dialog surfaces. The isolated desktop prevents TEdit from taking focus or sending input to the user's desktop.
