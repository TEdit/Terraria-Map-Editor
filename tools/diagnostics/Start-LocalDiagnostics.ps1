[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Executable,
    [Parameter(Mandatory = $true)][string]$Session,
    [Parameter(Mandatory = $true)][string]$ProfileDirectory,
    [int]$TimeoutSeconds = 120,
    [switch]$Worker
)
$ErrorActionPreference = 'Stop'
$Executable = (Resolve-Path -LiteralPath $Executable).Path
$Session = [IO.Path]::GetFullPath($Session)
$ProfileDirectory = [IO.Path]::GetFullPath($ProfileDirectory)
if ($Worker) {
    $env:TEDIT_LOCAL_PROFILE = $ProfileDirectory
    $env:TEDIT_LOCAL_SESSION = $Session
    if (-not ('TEdit.LocalDesktop' -as [type])) {
        Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
namespace TEdit {
    public static class LocalDesktop {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct StartupInfo { public int cb; public string reserved, desktop, title; public int x, y, width, height, xChars, yChars, fill, flags; public short show, reservedBytes; public IntPtr reservedPointer, input, output, error; }
        [StructLayout(LayoutKind.Sequential)] private struct ProcessInfo { public IntPtr process, thread; public int pid, tid; }
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateDesktop(string name, IntPtr device, IntPtr mode, int flags, uint access, IntPtr security);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool CloseDesktop(IntPtr desktop);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool CreateProcess(string application, StringBuilder command, IntPtr processAttributes, IntPtr threadAttributes, bool inherit, uint flags, IntPtr environment, string directory, ref StartupInfo startup, out ProcessInfo process);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
        public static void Run(string executable, string session) {
            string name = "TEdit-" + Guid.NewGuid().ToString("N");
            IntPtr desktop = CreateDesktop(name, IntPtr.Zero, IntPtr.Zero, 0, 0x01FF, IntPtr.Zero);
            if (desktop == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            try {
                var startup = new StartupInfo { cb = Marshal.SizeOf<StartupInfo>(), desktop = @"winsta0\" + name };
                var command = new StringBuilder("\"" + executable + "\" --local-control \"" + session + "\"");
                if (!CreateProcess(executable, command, IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, System.IO.Path.GetDirectoryName(executable), ref startup, out var process)) throw new Win32Exception(Marshal.GetLastWin32Error());
                CloseHandle(process.thread);
                WaitForSingleObject(process.process, 0xFFFFFFFF);
                CloseHandle(process.process);
            }
            finally { CloseDesktop(desktop); }
        }
    }
}
'@
    }
    [TEdit.LocalDesktop]::Run($Executable, $Session)
    return
}

if (Test-Path -LiteralPath $Session) { throw "Session already exists: $Session" }
$null = New-Item -ItemType Directory -Path $ProfileDirectory -Force
$fixtureDirectory = Join-Path $ProfileDirectory 'fixtures'
$artifactDirectory = Join-Path $ProfileDirectory 'artifacts'
$null = New-Item -ItemType Directory -Path $fixtureDirectory -Force
$null = New-Item -ItemType Directory -Path $artifactDirectory -Force
$fixturePath = Join-Path $fixtureDirectory 'diagnostic-world.wld'
if (-not (Test-Path -LiteralPath $fixturePath)) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $fixtureArchive = Join-Path (Join-Path $PSScriptRoot '../..') 'src/TEdit.Tests/WorldFiles/v1.4.4.4.wld.zip'
    $archive = [IO.Compression.ZipFile]::OpenRead($fixtureArchive)
    try {
        $worldEntry = $archive.Entries | Where-Object { $_.FullName.EndsWith('.wld', [StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
        if ($null -eq $worldEntry) { throw "No .wld fixture found in $fixtureArchive" }
        $input = $worldEntry.Open()
        try {
            $output = [IO.File]::Create($fixturePath)
            try { $input.CopyTo($output) } finally { $output.Dispose() }
        }
        finally { $input.Dispose() }
    }
    finally { $archive.Dispose() }
}
$settingsPath = Join-Path $ProfileDirectory 'userSettings.json'
if (-not (Test-Path -LiteralPath $settingsPath)) {
    $version = [Diagnostics.FileVersionInfo]::GetVersionInfo($Executable).ProductVersion
    @{ UpdateMode = 'Disabled'; ShowSplashScreen = $false; ShowTextureLoadingNotice = $false; ShowAllWeaponRackItems = $false; Telemetry = 0; TelemetryDeclinedVersion = $version; MaxBackups = 1 } | ConvertTo-Json | Set-Content -LiteralPath $settingsPath -Encoding utf8
}
$hostExecutable = (Get-Process -Id $PID).Path
$arguments = '-NoProfile -ExecutionPolicy Bypass -File "{0}" -Executable "{1}" -Session "{2}" -ProfileDirectory "{3}" -Worker' -f $PSCommandPath, $Executable, $Session, $ProfileDirectory
$supervisor = Start-Process -FilePath $hostExecutable -ArgumentList $arguments -WindowStyle Hidden -PassThru
$deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
while ([DateTime]::UtcNow -lt $deadline) {
    if (Test-Path -LiteralPath $Session) {
        try {
            $health = & (Join-Path $PSScriptRoot 'Invoke-LocalDiagnostics.ps1') -Session $Session -Request '{"op":"health"}' -TimeoutSeconds 5 | ConvertFrom-Json
            $health | ConvertTo-Json -Depth 16
            return
        }
        catch { Write-Verbose $_ }
    }
    if ($supervisor.HasExited) { throw "Diagnostic process exited before the IPC channel was ready (supervisor exit $($supervisor.ExitCode)). Check $ProfileDirectory\Logs and $ProfileDirectory\artifacts." }
    Start-Sleep -Milliseconds 200
}
throw "Startup timed out. Supervisor PID $($supervisor.Id) may still be running; inspect $Session before retrying."
