@echo off

echo Building Solution...
set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
%msbuild% .\TEditNoInstaller.sln /property:Configuration=Release;Platform=AnyCPU 

REM echo Copying Files...
REM if exist ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" copy ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" .\BIN\

xcopy .\TEditXna\bin\Release .\BIN\Portable /S /Y /I /EXCLUDE:exclude.txt

pause