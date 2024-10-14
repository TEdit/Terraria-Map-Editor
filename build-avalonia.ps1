param(
    [string] $ReleasePath = ".\release",
    [string] $VersionPrefix = "5.0.0",
    [string] $VersionSuffix = $null
)

$publishPath = "publish\avalonia"

$platforms = $(
    # linux builds
    "linux-arm64",
    "linux-musl-x64",
    "linux-x64",
    "linux-musl-arm64",
    # windows
    "win-x64",
    "win-arm64",
    # mac
    "osx-x64",
    "osx-arm64"
)

if (Test-Path -Path ".\$publishPath") { Remove-Item -Path ".\$publishPath" -Force -Recurse }

$platforms | ForEach-Object {
    $buildArgs = @(
        "publish"
        "-c"
        "Release"
        "-r"
        $_
        "--self-contained"
        "true"
        "-p:PublishSingleFile=true"
        "-o"
        ".\$publishPath\$_"
        "/p:VersionPrefix=""$VersionPrefix"""
        "--version-suffix"
        "$VersionSuffix"
        ".\src\TEdit5\TEdit5.csproj"
    )

    & dotnet $buildArgs

    Compress-Archive -Path ".\$publishPath\$_\*" -DestinationPath ".\$ReleasePath\TEditAvalonia-$VersionPrefix-$VersionSuffix-$_.zip"
}
