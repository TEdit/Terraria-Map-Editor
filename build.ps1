param(
    [string] $VersionPrefix = "4.11.0",
    [string] $VersionSuffix = "beta2"
)

$buildArgs = @(
    "publish"
    "-c"
    "Release"
    "-f"
    "net462"
    ".\src\TEdit.sln"
    '/p:signcert="BC Code Signing"')


$filename = "TEdit-$VersionPrefix"


if (![String]::IsNullOrWhitespace($VersionPrefix)) {
    $buildArgs += "/p:VersionPrefix=""$VersionPrefix"""
}

if (![String]::IsNullOrWhitespace($VersionSuffix)) {
    $buildArgs += "/p:VersionSuffix=""$VersionSuffix"""
    $filename = "TEdit-$VersionPrefix-$VersionSuffix"
}

& dotnet $buildArgs

if (Test-Path -Path ".\release") { Remove-Item -Path ".\release" -Recurse -Force }
if (Test-Path -Path ".\TEdit*.zip") { Remove-Item -Path ".\TEdit*.zip" }
if (Test-Path -Path ".\TEdit*.msi") { Remove-Item -Path ".\TEdit*.msi" }

mkdir -Path ".\release"

Copy-Item -Path ".\src\TEdit\bin\Release\net462\zh-CN" -Destination ".\src\TEdit\bin\Release\net462\publish\zh-CN\" -Recurse -Force
Copy-Item -Path ".\src\TEdit\bin\Release\net462\publish" -Destination ".\release\$filename\" -Recurse -Force
Copy-Item -Path ".\schematics" -Destination ".\release" -Recurse

# Create Installer
# $env:VERSION_PREFIX = $VersionPrefix
# $env:VERSION_SUFFIX = $VersionSuffix
# & dotnet build -c Release ".\src\Setup\Setup.csproj" 
# signtool.exe sign /v /fd sha256 /n "BC Code Signing" /t http://timestamp.digicert.com ".\src\Setup\$filename.msi"
# Move-Item ".\src\Setup\$filename.msi" ".\"

# Create ZIP Release
Compress-Archive -Path ".\release\*" -DestinationPath ".\$filename.zip"