param(
    [string] $VersionPrefix = "5.0.0",
    [string] $VersionSuffix = "alpha2"
)

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

if (Test-Path -Path ".\publish\TEdit*.zip") { Remove-Item -Path ".\publish\TEdit*.zip" }

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
        ".\publish\$_"
        "/p:VersionPrefix=""$VersionPrefix"""
        "--version-suffix"
        "$VersionSuffix"
        ".\src\TEdit.Desktop\TEdit.Desktop.csproj"
    )

    & dotnet $buildArgs

    Compress-Archive -Path ".\publish\$_\*" -DestinationPath ".\publish\TEdit-$VersionPrefix-$VersionSuffix-$_-x64.zip"
}
