param(
    [string] $VersionPrefix = "4.5.0",
    [string] $VersionSuffix = "beta4"
)

$buildArgs = @(
    "publish", 
    "-c"
    "Release"
    ".\src\TEdit.sln", 
    '/p:signcert="BC Code Signing"')

if ($null -ne $VersionPrefix) {
    $buildArgs += "/p:VersionPrefix=""$VersionPrefix"""
}

if ($null -ne $VersionSuffix) {
    $buildArgs += "/p:VersionSuffix=""$VersionSuffix"""
}

& dotnet $buildArgs

Remove-Item -Path ".\release" -Recurse -Force
Remove-Item -Path ".\TEdit*.zip"
Remove-Item -Path ".\TEdit*.msi"

mkdir -Path ".\release"

Copy-Item -Path ".\src\TEdit\bin\Release\net462\publish" -Destination ".\release\TEdit-$VersionPrefix-$VersionSuffix\" -Recurse -Force
Copy-Item -Path ".\schematics" -Destination ".\release" -Recurse

# Create Installer
$env:VERSION_PREFIX = $VersionPrefix
$env:VERSION_SUFFIX = $VersionSuffix
& dotnet build -c Release ".\src\Setup\Setup.csproj" 
signtool.exe sign /v /fd sha256 /n "BC Code Signing" /t http://timestamp.digicert.com ".\src\Setup\TEdit-$VersionPrefix-$VersionSuffix.msi"
Move-Item ".\src\Setup\TEdit-$VersionPrefix-$VersionSuffix.msi" ".\"

# Create ZIP Release
Compress-Archive -Path ".\release\*" -DestinationPath ".\TEdit-$VersionPrefix-$VersionSuffix.zip"