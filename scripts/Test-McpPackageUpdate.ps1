# Exercise a disposable TEdit.Mcp.Phase6.Check installation against an already running shared-local editor.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InstallDirectory,
    [Parameter(Mandatory)][string]$UpdatePackage
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath $InstallDirectory).Path
$package = (Resolve-Path -LiteralPath $UpdatePackage).Path
$product = 'TEdit.Mcp.Phase6.Check'
$current = Join-Path $root 'current'
$manifest = Join-Path $current 'sq.version'
function Get-InstalledVersion {
    $metadata = ([xml](Get-Content -LiteralPath $manifest -Raw)).package.metadata
    if ($metadata.id -cne $product) { throw 'Select the disposable MCP verification installation.' }
    $metadata.version
}

$before = Get-InstalledVersion
$archive = [IO.Compression.ZipFile]::OpenRead($package)
try {
    $spec = @($archive.Entries | Where-Object { $_.FullName -match '^[^/]+\.nuspec$' })
    if ($spec.Count -ne 1) { throw 'Expected a full Velopack package.' }
    $reader = [IO.StreamReader]::new($spec[0].Open())
    try { $next = ([xml]($reader.ReadToEnd())).package.metadata } finally { $reader.Dispose() }
    if ($next.id -cne $product -or [version]$next.version -le [version]$before) { throw 'Select a newer disposable verification package.' }
} finally { $archive.Dispose() }

$discovery = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'TEdit/Mcp/endpoint.json'
$editor = Get-Content -LiteralPath $discovery -Raw | ConvertFrom-Json
$settings = Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'TEdit/userSettings.json'
function Get-SettingsHash {
    if (Test-Path -LiteralPath $settings) { (Get-FileHash -LiteralPath $settings).Hash }
}
$settingsBefore = Get-SettingsHash
$export = @{ command=(Join-Path $current 'TEdit.exe'); args=@('--mcp-stdio',$editor.endpoint) } | ConvertTo-Json -Compress

function Test-Bridge {
    $entry = $export | ConvertFrom-Json
    $info = [Diagnostics.ProcessStartInfo]::new($entry.command)
    $info.UseShellExecute = $false
    $info.RedirectStandardInput = $true
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.CreateNoWindow = $true
    foreach ($arg in $entry.args) { $info.ArgumentList.Add($arg) }
    $process = [Diagnostics.Process]::Start($info)
    try {
        $stderr = $process.StandardError.ReadToEndAsync()
        function Send-Message($message) {
            $process.StandardInput.WriteLine(($message | ConvertTo-Json -Depth 8 -Compress))
            $process.StandardInput.Flush()
        }
        function Read-Reply([int]$id) {
            $line = $process.StandardOutput.ReadLineAsync().WaitAsync([TimeSpan]::FromSeconds(15)).GetAwaiter().GetResult()
            if (!$line) { throw 'The packaged bridge closed before responding. Check that the shared-local editor is running.' }
            $response = $line | ConvertFrom-Json
            if ($response.id -ne $id -or !$response.result) { throw "Unexpected MCP response to request $id." }
            $response.result
        }
        Send-Message @{ jsonrpc='2.0'; id=1; method='initialize'; params=@{
            protocolVersion='2025-11-25'; capabilities=@{}; clientInfo=@{name='TEdit package smoke';version='1'} } }
        $init = Read-Reply 1
        if ($init.instructions -notlike '*backup copy*') { throw 'Missing backup reminder.' }
        Send-Message @{jsonrpc='2.0'; method='notifications/initialized'}
        Send-Message @{jsonrpc='2.0'; id=2; method='tools/call'; params=@{name='get_status';arguments=@{}}}
        $status = (Read-Reply 2).structuredContent.data
        if ($status.connectionMode -ne 'shared_local' -or $status.instanceId -ne $editor.instanceId) { throw 'Unexpected MCP instance or access mode.' }
        $process.StandardInput.Close()
        if (!$process.WaitForExit(5000) -or $process.ExitCode -ne 0) { throw 'Bridge did not exit cleanly.' }
        $null = $stderr.GetAwaiter().GetResult()
        $init.protocolVersion
    } finally {
        if (!$process.HasExited) { $process.Kill($true); $process.WaitForExit() }
        $process.Dispose()
    }
}

$protocol = Test-Bridge
$update = [Diagnostics.ProcessStartInfo]::new((Join-Path $root 'Update.exe'))
$update.UseShellExecute = $false
$update.CreateNoWindow = $true
foreach ($arg in @('apply','--silent','--norestart','--package',$package)) { $update.ArgumentList.Add($arg) }
$updater = [Diagnostics.Process]::Start($update)
try {
    if (!$updater.WaitForExit(60000) -or $updater.ExitCode -ne 0) { throw 'Disposable update failed or timed out.' }
} finally {
    if (!$updater.HasExited) { $updater.Kill($true); $updater.WaitForExit() }
    $updater.Dispose()
}
$after = Get-InstalledVersion
if ($after -ne $next.version -or (Test-Bridge) -ne $protocol) { throw 'Updated package or bridge failed verification.' }
if ((Get-SettingsHash) -ne $settingsBefore) { throw 'The installed user settings changed.' }
[pscustomobject]@{ From=$before; To=$after; Protocol=$protocol; SettingsPreserved=$true; BridgePathPreserved=$true }
