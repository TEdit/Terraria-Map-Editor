param(
    [string] $ReleasePath = ".\release",
    [string] $VersionPrefix = "5.0.0",
    [string] $VersionSuffix = "beta1"
)

if (Test-Path -Path ".\$ReleasePath") { Remove-Item -Path ".\$ReleasePath" -Force -Recurse }
New-Item -Path ".\$ReleasePath\" -Force -ItemType "directory"

.\build-legacy.ps1 -ReleasePath $ReleasePath -VersionPrefix $VersionPrefix -VersionSuffix $VersionSuffix
.\build-avalonia.ps1 -ReleasePath $ReleasePath -VersionPrefix $VersionPrefix -VersionSuffix $VersionSuffix
