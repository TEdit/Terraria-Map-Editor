# Run from a fresh portable publish in a signed-in Windows desktop session.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$PublishDirectory)
$ErrorActionPreference = 'Stop'
$package = (Resolve-Path -LiteralPath $PublishDirectory).Path
$exe = Join-Path $package 'TEdit.exe'
$settings = Join-Path $package 'Data'
$profiles = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'TEdit\Mcp'
if ((Test-Path $settings) -or (Test-Path (Join-Path $profiles 'endpoint.json')) -or
    (Test-Path (Join-Path $profiles '*.profile'))) {
    throw 'Use a fresh portable publish. Existing MCP discovery and client profiles must be preserved.'
}
foreach ($file in 'TEdit.exe','coreclr.dll','hostfxr.dll','ModelContextProtocol.AspNetCore.dll','Microsoft.AspNetCore.Server.Kestrel.Core.dll') {
    if (!(Test-Path (Join-Path $package $file))) { throw "Missing published file: $file" }
}
Add-Type -AssemblyName System.Security
[void](New-Item -ItemType Directory -Path $settings,$profiles -Force)
$profileId = [guid]::NewGuid()
$secret = New-Object byte[] 32
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
try { $rng.GetBytes($secret) } finally { $rng.Dispose() }
$entry = @{ Name='Smoke test'; Token=[Convert]::ToBase64String($secret) } | ConvertTo-Json -Compress
$cipher = [Security.Cryptography.ProtectedData]::Protect([Text.Encoding]::UTF8.GetBytes($entry),$null,'CurrentUser')
$profileFile = Join-Path $profiles "$profileId.profile"
[IO.File]::WriteAllBytes($profileFile,$cipher)
$headers = @{ Authorization=('Bearer ' + $profileId.ToString('N') + '.' + ([BitConverter]::ToString($secret) -replace '-','')); Accept='application/json, text/event-stream' }
$worlds = (New-Item -ItemType Directory -Path (Join-Path $settings 'Worlds')).FullName
$port = 48190
@{ McpServer=@{ Enabled=$true; Port=$port }; WorldsPath=$worlds; SteamWorldsPath=$worlds; Telemetry=0; TelemetryDeclinedVersion=(Get-Item (Join-Path $package 'TEdit.dll')).VersionInfo.ProductVersion; ShowSplashScreen=$false; ShowTextureLoadingNotice=$false } |
    ConvertTo-Json | Set-Content (Join-Path $settings 'userSettings.json') -Encoding UTF8
$process = Start-Process -FilePath $exe -WorkingDirectory $package -WindowStyle Hidden -PassThru
try {
    $discovery = Join-Path $profiles 'endpoint.json'
    $deadline = (Get-Date).AddSeconds(60)
    while (!(Test-Path $discovery) -and !$process.HasExited -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 500 }
    if (!(Test-Path $discovery)) { throw 'The published editor did not start its host within 60 seconds.' }
    $connection = Get-Content -Raw $discovery | ConvertFrom-Json
    $expected = "http://127.0.0.1:$port/mcp/" + ([guid]$connection.instanceId).ToString('N')
    if ($connection.endpoint -cne $expected) { throw 'Unexpected discovery endpoint' }
    function Invoke-SmokeRpc($id,$method,$params) {
        $body = @{jsonrpc='2.0';id=$id;method=$method;params=$params} | ConvertTo-Json -Depth 8 -Compress
        $response = Invoke-WebRequest -UseBasicParsing -TimeoutSec 10 -MaximumRedirection 0 -Uri $expected -Method Post -Headers $headers -ContentType 'application/json' -Body $body
        $text = $response.Content
        if ($text -is [byte[]]) { $text = [Text.Encoding]::UTF8.GetString($text) }
        if ($text -match '(?m)^data: ?(.+)$') { $text = $Matches[1] }
        $rpc = $text | ConvertFrom-Json
        if ($rpc.error -or $null -eq $rpc.result) { throw "RPC failed for $method" }
        return $rpc.result
    }
    $init = Invoke-SmokeRpc 1 'initialize' @{protocolVersion='2025-11-25';capabilities=@{};clientInfo=@{name='tedit-publish-smoke';version='1'}}
    if ($init.instructions -notlike '*backup copy*') { throw 'Missing pre-edit backup recommendation' }
    $headers['MCP-Protocol-Version'] = $init.protocolVersion
    $initialized = '{"jsonrpc":"2.0","method":"notifications/initialized"}'
    [void](Invoke-WebRequest -UseBasicParsing -TimeoutSec 10 -MaximumRedirection 0 -Uri $expected -Method Post -Headers $headers -ContentType 'application/json' -Body $initialized)
    $tools = Invoke-SmokeRpc 2 'tools/list' @{}
    if ($tools.tools.Count -ne 8) { throw 'Unexpected tool count' }
    # The listener can be ready while WPF is still loading editor assets.
    for ($attempt = 0; $attempt -lt 4; $attempt++) {
        $status = Invoke-SmokeRpc (3 + $attempt) 'tools/call' @{name='get_status';arguments=@{}}
        if ($status.structuredContent.errorCode -ne 'editor_timeout') { break }
    }
    if ($status.isError) { throw "Status tool failed: $($status.content[0].text)" }
    if ($status.structuredContent.data.instanceId -ne $connection.instanceId) { throw "Unexpected status result: $($status | ConvertTo-Json -Compress -Depth 6)" }
    $world = Invoke-SmokeRpc 4 'tools/call' @{name='get_world_info';arguments=@{}}
    if ($world.structuredContent.errorCode -ne 'no_world') { throw 'Expected no_world' }
    [pscustomobject]@{ Result='Passed'; OS=[Environment]::OSVersion.VersionString; Protocol=$init.protocolVersion; Tools=$tools.tools.Count; AssemblySHA256=(Get-FileHash (Join-Path $package 'TEdit.dll')).Hash }
} finally {
    if (!$process.HasExited) { Stop-Process -Id $process.Id; $process.WaitForExit(5000) | Out-Null }
    if ($connection -and (Test-Path $discovery)) {
        $published = Get-Content -Raw $discovery | ConvertFrom-Json
        if ($published.instanceId -eq $connection.instanceId) { Remove-Item -LiteralPath $discovery }
    }
    Remove-Item -LiteralPath $profileFile -ErrorAction SilentlyContinue
}
