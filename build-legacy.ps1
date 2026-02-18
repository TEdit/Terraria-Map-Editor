param(
    [string] $ReleasePath = ".\release",
    [string] $VersionPrefix = "5.1.0",
    [string] $VersionSuffix = $null,
    [string] $Channel = "stable"
)

$velopackPublishPath = "publish\velopack"
$zipPublishPath = "publish\legacy"

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

if (Test-Path -Path ".\publish") { Remove-Item -Path ".\publish" -Force -Recurse }

$fullVersion = $VersionPrefix
if (![String]::IsNullOrWhitespace($VersionSuffix)) {
    $fullVersion = "$VersionPrefix-$VersionSuffix"
}

$platforms | ForEach-Object {
    # 1. Velopack publish (non-single-file, needed for delta packages)
    $velopackArgs = @(
        "publish"
        "-c"
        "Release"
        "-r"
        $_
        "--self-contained"
        "true"
        "-o"
        ".\$velopackPublishPath\$_"
        "/p:VersionPrefix=""$VersionPrefix"""
    )

    if (![String]::IsNullOrWhitespace($VersionSuffix)) {
        $velopackArgs += "--version-suffix"
        $velopackArgs += """$VersionSuffix"""
    }

    $velopackArgs += ".\src\TEdit\TEdit.csproj"

    Write-Host "Publishing for Velopack ($_ )..."
    & dotnet $velopackArgs

    if (Test-Path -Path ".\schematics") { Copy-Item -Path ".\schematics" -Destination ".\$velopackPublishPath\$_\" -Recurse }

    # 2. Zip publish (single-file for portable users)
    $zipArgs = @(
        "publish"
        "-c"
        "Release"
        "-r"
        $_
        "--self-contained"
        "true"
        "-p:PublishSingleFile=true"
        "-o"
        ".\$zipPublishPath\$_"
        "/p:VersionPrefix=""$VersionPrefix"""
    )

    if (![String]::IsNullOrWhitespace($VersionSuffix)) {
        $zipArgs += "--version-suffix"
        $zipArgs += """$VersionSuffix"""
    }

    $zipArgs += ".\src\TEdit\TEdit.csproj"

    Write-Host "Publishing for portable zip ($_ )..."
    & dotnet $zipArgs

    if (Test-Path -Path ".\schematics") { Copy-Item -Path ".\schematics" -Destination ".\$zipPublishPath\$_\" -Recurse }

    # 3. Create ZIP release (portable)
    $filename = ".\$ReleasePath\TEdit-$VersionPrefix.zip"

    if (![String]::IsNullOrWhitespace($VersionSuffix)) {
        $filename = ".\$ReleasePath\TEdit-$VersionPrefix-$VersionSuffix.zip"
    }

    Compress-Archive -Path ".\$zipPublishPath\$_\*" -DestinationPath $filename

    # 4. Pack with Velopack
    Write-Host "Packing with Velopack (channel: $Channel)..."
    vpk pack -u TEdit -v $fullVersion -p ".\$velopackPublishPath\$_" --channel $Channel --icon ".\src\TEdit\Images\tedit.ico" --packAuthors "BinaryConstruct" -o ".\$ReleasePath"
}
