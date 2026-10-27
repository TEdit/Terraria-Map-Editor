[CmdletBinding()]
param(
    [ValidateSet('start', 'status', 'state', 'screenshot', 'request', 'stop')][string]$Action = 'start',
    [string]$Session,
    [string]$ProfileDirectory,
    [string]$Request,
    [string]$OutputPath,
    [ValidateSet('main', 'active')][string]$WindowScope = 'main',
    [int]$TimeoutSeconds = 30,
    [switch]$SkipBuild
)
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$executable = Join-Path $repository 'src/TEdit/bin/Release/net10.0-windows10.0.19041.0/win-x64/TEdit.exe'
if (-not $Session) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $key = ([Convert]::ToHexString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($repository.ToLowerInvariant())))).Substring(0, 12) }
    finally { $sha.Dispose() }
    $Session = Join-Path $env:LOCALAPPDATA "TEdit/Diagnostics/$key/session.json"
}
$Session = [IO.Path]::GetFullPath($Session)
if (-not $ProfileDirectory) { $ProfileDirectory = $Session + '.profile' }
$ProfileDirectory = [IO.Path]::GetFullPath($ProfileDirectory)
$sessionDirectory = Split-Path -Parent $Session
$invoke = Join-Path $PSScriptRoot 'Invoke-LocalDiagnostics.ps1'

function Send-Request([string]$Json, [int]$Timeout = 30) {
    & $invoke -Session $Session -Request $Json -TimeoutSeconds $Timeout | ConvertFrom-Json
}

$descriptor = $null
if (Test-Path -LiteralPath $Session) {
    try { $descriptor = Get-Content -LiteralPath $Session -Raw | ConvertFrom-Json }
    catch { throw "Unreadable session descriptor: $Session. Inspect before removing it." }
    $process = Get-Process -Id $descriptor.pid -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        if ($Action -eq 'start') {
            Remove-Item -LiteralPath $Session
            $descriptor = $null
        }
        else { throw "Diagnostic PID $($descriptor.pid) has exited; stale session descriptor remains at $Session." }
    }
    elseif ([IO.Path]::GetFullPath($descriptor.executable) -ne [IO.Path]::GetFullPath($executable) -or [IO.Path]::GetFullPath($process.Path) -ne [IO.Path]::GetFullPath($executable)) {
        throw "Session executable differs from the requested diagnostic build. Use another -Session or stop the existing session: $Session"
    }
}

if ($Action -eq 'start') {
    $reused = $null -ne $descriptor
    if (-not $reused) {
        if (-not $SkipBuild) {
            $buildLog = Join-Path (Join-Path $ProfileDirectory 'artifacts') 'build.log'
            $null = New-Item -ItemType Directory -Force (Split-Path -Parent $buildLog)
            & dotnet build (Join-Path $repository 'src/TEdit/TEdit.csproj') -c Release -p:LocalDiagnostics=true -p:RuntimeIdentifier=win-x64 -v minimal *> $buildLog
            if ($LASTEXITCODE -ne 0) { throw "Diagnostic build failed; inspect $buildLog" }
        }
        if (-not (Test-Path -LiteralPath $executable)) { throw "Diagnostic executable not found: $executable. Build with dotnet 10 and the Windows desktop workload." }
        $null = & (Join-Path $PSScriptRoot 'Start-LocalDiagnostics.ps1') -Executable $executable -Session $Session -ProfileDirectory $ProfileDirectory | ConvertFrom-Json
        $descriptor = Get-Content -LiteralPath $Session -Raw | ConvertFrom-Json
    }
    $health = Send-Request '{"op":"health"}' -Timeout 5
    @{ status = 'ready'; reused = $reused; session = $Session; descriptor = $descriptor; health = $health } | ConvertTo-Json -Depth 16
    return
}

if ($Action -eq 'status') {
    if ($null -eq $descriptor) { @{ status = 'stopped'; session = $Session; executable = $executable } | ConvertTo-Json; return }
    @{ status = 'ready'; session = $Session; descriptor = $descriptor; state = (Send-Request '{"op":"state"}') } | ConvertTo-Json -Depth 16
    return
}
if ($null -eq $descriptor) { throw 'No diagnostic session. Run tools/diagnostics/diagnostics.ps1 -Action start first.' }

switch ($Action) {
    'state' { Send-Request '{"op":"state"}' | ConvertTo-Json -Depth 16 }
    'request' {
        if (-not $Request) { throw '-Request is required.' }
        Send-Request $Request -Timeout $TimeoutSeconds | ConvertTo-Json -Depth 32
    }
    'screenshot' {
        if (-not $OutputPath) { $OutputPath = Join-Path (Join-Path $ProfileDirectory 'artifacts') ('ui-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '.png') }
        $path = [IO.Path]::GetFullPath($OutputPath)
        $payload = @{ op = 'screenshot'; path = $path; scope = $WindowScope } | ConvertTo-Json -Compress
        Send-Request $payload -Timeout 20 | ConvertTo-Json -Depth 16
    }
    'stop' {
        $ownedPid = [int]$descriptor.pid
        $null = Send-Request '{"op":"shutdown"}' -Timeout 10
        $deadline = [DateTime]::UtcNow.AddSeconds(15)
        while ((Get-Process -Id $ownedPid -ErrorAction SilentlyContinue) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 150 }
        if (Get-Process -Id $ownedPid -ErrorAction SilentlyContinue) { throw "Shutdown requested but owned PID $ownedPid is still active; inspect status and logs. It was not force-killed." }
        if (Test-Path -LiteralPath $Session) {
            $stale = Get-Content -LiteralPath $Session -Raw | ConvertFrom-Json
            if ([int]$stale.pid -eq $ownedPid -and [IO.Path]::GetFullPath($stale.executable) -eq [IO.Path]::GetFullPath($executable)) {
                Remove-Item -LiteralPath $Session
            }
        }
        @{ status = 'stopped'; session = $Session } | ConvertTo-Json
    }
}
