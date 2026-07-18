[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [string] $Path,

    [Parameter(Position = 1)]
    [string] $FixtureName
)

$ErrorActionPreference = "Stop"

$sourcePath = (Resolve-Path -LiteralPath $Path).Path
if ([IO.Path]::GetExtension($sourcePath) -ine ".wld") {
    throw "Test world fixtures must use the .wld extension: $sourcePath"
}

$fileInfo = Get-Item -LiteralPath $sourcePath
if ($fileInfo.Length -lt 500) {
    throw "The source is too small to be a Terraria world file: $sourcePath"
}

$prefix = [Text.Encoding]::ASCII.GetString([IO.File]::ReadAllBytes($sourcePath), 0, 8)
if ($prefix -eq "version ") {
    throw "The source is a Git LFS pointer, not a Terraria world file: $sourcePath"
}

if ([string]::IsNullOrWhiteSpace($FixtureName)) {
    $FixtureName = [IO.Path]::GetFileName($sourcePath)
}

$FixtureName = $FixtureName.Replace('\', '/')
if (-not $FixtureName.EndsWith(".wld", [StringComparison]::OrdinalIgnoreCase)) {
    throw "FixtureName must end in .wld: $FixtureName"
}

if ([IO.Path]::IsPathRooted($FixtureName) -or $FixtureName.Split('/') -contains '..') {
    throw "FixtureName must be a path relative to the test WorldFiles directory: $FixtureName"
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$worldFilesRoot = Join-Path $repoRoot "src/TEdit.Tests/WorldFiles"
$worldPath = Join-Path $worldFilesRoot $FixtureName
$archivePath = "$worldPath.zip"

if (Test-Path -LiteralPath $archivePath) {
    throw "The immutable fixture archive already exists: $archivePath"
}

$archiveDirectory = Split-Path -Parent $archivePath
[IO.Directory]::CreateDirectory($archiveDirectory) | Out-Null
$temporaryPath = "$archivePath.tmp"

try {
    $archiveStream = [IO.File]::Create($temporaryPath)
    try {
        $archive = [IO.Compression.ZipArchive]::new(
            $archiveStream,
            [IO.Compression.ZipArchiveMode]::Create,
            $false)
        try {
            $entry = $archive.CreateEntry(
                [IO.Path]::GetFileName($worldPath),
                [IO.Compression.CompressionLevel]::SmallestSize)
            $entry.LastWriteTime = [DateTimeOffset]::new(1980, 1, 1, 0, 0, 0, [TimeSpan]::Zero)

            $inputStream = [IO.File]::OpenRead($sourcePath)
            try {
                $outputStream = $entry.Open()
                try {
                    $inputStream.CopyTo($outputStream)
                }
                finally {
                    $outputStream.Dispose()
                }
            }
            finally {
                $inputStream.Dispose()
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $archiveStream.Dispose()
    }

    Move-Item -LiteralPath $temporaryPath -Destination $archivePath
}
finally {
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Force
    }
}

$relativeArchive = [IO.Path]::GetRelativePath($repoRoot, $archivePath).Replace('\', '/')
Write-Host "Created immutable test fixture: $relativeArchive"
Write-Host "Run tests to verify it, then commit the archive:"
Write-Host "  git add -- $relativeArchive"
