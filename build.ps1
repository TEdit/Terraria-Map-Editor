param(
    [string] $ReleasePath = ".\release",
    [string] $Version = ""
)

# Derive version from git tags if not explicitly provided
if ([String]::IsNullOrWhitespace($Version)) {
    $desc = git describe --tags --long 2>$null
    if ($LASTEXITCODE -eq 0 -and $desc) {
        $parts = $desc -split '-'
        $ahead = [int]$parts[-2]
        $tag = ($parts[0..($parts.Length - 3)] -join '-')

        if ($ahead -eq 0) {
            $Version = $tag
        } elseif ($tag -match '-') {
            # Tag has pre-release suffix (e.g. 5.0.0-beta2) -> 5.0.0-beta2.ci.3
            $Version = "$tag.ci.$ahead"
        } else {
            # Stable tag (e.g. 5.0.0) -> 5.0.0-ci.3
            $Version = "$tag-ci.$ahead"
        }
    } else {
        $Version = "0.0.0-dev"
    }
    Write-Host "Derived version: $Version"
}

$versionfixed = $Version.Replace("/", "_");
$versionSplit = $versionfixed.Split("-");

$VersionPrefix = $versionSplit[0]

if ($versionSplit.Length -gt 1) {
    $VersionSuffix = $versionSplit[1]
} else {
    $VersionSuffix = ""
}

if (Test-Path -Path ".\$ReleasePath") { Remove-Item -Path ".\$ReleasePath" -Force -Recurse }
New-Item -Path ".\$ReleasePath\" -Force -ItemType "directory"

.\build-legacy.ps1 -ReleasePath $ReleasePath -VersionPrefix $VersionPrefix -VersionSuffix $VersionSuffix
# .\build-avalonia.ps1 -ReleasePath $ReleasePath -VersionPrefix $VersionPrefix -VersionSuffix $VersionSuffix
