[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Session,
    [Parameter(Mandatory = $true)][string]$Request,
    [int]$TimeoutSeconds = 30
)
$ErrorActionPreference = 'Stop'
$descriptor = Get-Content -LiteralPath $Session -Raw | ConvertFrom-Json
if ($descriptor.protocol -ne 1) { throw 'Unsupported diagnostic protocol.' }
$null = $Request | ConvertFrom-Json
$pipe = [System.IO.Pipes.NamedPipeClientStream]::new('.', $descriptor.pipe, [System.IO.Pipes.PipeDirection]::InOut, [System.IO.Pipes.PipeOptions]::Asynchronous)
try {
    $pipe.Connect($TimeoutSeconds * 1000)
    $encoding = [System.Text.UTF8Encoding]::new($false)
    $reader = [System.IO.StreamReader]::new($pipe, $encoding, $false, 4096, $true)
    $writer = [System.IO.StreamWriter]::new($pipe, $encoding, 4096, $true)
    try {
        $writer.AutoFlush = $true
        $writer.WriteLine(($Request | ConvertFrom-Json | ConvertTo-Json -Compress -Depth 32))
        $pending = $reader.ReadLineAsync()
        if (-not $pending.Wait($TimeoutSeconds * 1000)) {
            throw 'Response timed out. The operation may still be running; inspect state before retrying a mutation.'
        }
        $line = $pending.GetAwaiter().GetResult()
        if ($null -eq $line) { throw 'Application disconnected without a response.' }
        $response = $line | ConvertFrom-Json
        if (-not $response.ok) { throw $response.error }
        $response.result | ConvertTo-Json -Depth 32
    }
    finally {
        $writer.Dispose()
        $reader.Dispose()
    }
}
finally {
    $pipe.Dispose()
}
