param(
    [string] $ReleasePath = ".\release",
    [string] $Version = "5.0.0-dev"
)

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
.\build-avalonia.ps1 -ReleasePath $ReleasePath -VersionPrefix $VersionPrefix -VersionSuffix $VersionSuffix
