param(
    [string] $ReleasePath = ".\release",
    [string] $VersionPrefix = "5.0.0",
    [string] $VersionSuffix = $null
)

$publishPath = "publish\legacy"

$platforms = $(
    ## linux builds ##
    # "linux-arm64",
    # "linux-musl-x64",
    # "linux-x64",
    # "linux-musl-arm64",
    ## windows ##
    "win-x64"
    #"win-arm64",
    ## mac ##
    # "osx-x64",
    # "osx-arm64"
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
    )
    
    if (![String]::IsNullOrWhitespace($VersionSuffix)) {
        $buildArgs += "--version-suffix"
        $buildArgs += """$VersionSuffix"""
    }

    $buildArgs += ".\src\TEdit\TEdit.csproj"

    & dotnet $buildArgs

    if (Test-Path -Path ".\schematics") { Copy-Item -Path ".\schematics" -Destination ".\$publishPath\$_\" -Recurse }

    # Create ZIP Release

    $filename = ".\$ReleasePath\TEdit-$VersionPrefix.zip"
    
    if (![String]::IsNullOrWhitespace($VersionSuffix)) {
        $filename = ".\$ReleasePath\TEdit-$VersionPrefix-$VersionSuffix.zip"
    }

    Compress-Archive -Path ".\$publishPath\$_\*" -DestinationPath $filename
}
