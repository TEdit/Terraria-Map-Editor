@echo off



echo Building Solution...
set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
%msbuild% .\Build.proj /property:Configuration=Release;Platform=x86 

echo Copying Files...
if exist ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" copy ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" .\BIN\