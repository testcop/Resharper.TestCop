REM Called from AfterBuild Step within csproj
@ECHO === === === === === === === ===
SET EnableNuGetPackageRestore=true
SET NUGET=..\..\..\..\InstallSupport\NuGet.exe
SET OUTDIR=C:\SourceCode\
SET VER=%1-EAP

IF NOT EXIST %OUTDIR%\NUL SET OUTDIR=%TEMP%

IF NOT EXIST %OUTDIR%\NUL OUTDIR=%TEMP%

REM Extract Wave Version from Package. JetBrains.Platform.Sdk.*.*.*.*
SET VV=DIR /B ..\..\..\..\..\Packages\JetBrains.Platform.Sdk.*
for /f "usebackq tokens=4 delims=." %%a in (`%VV%`) do SET WAVEID=%%a
@ECHO Resharper Platform WAVE Id=%WAVEID%

@ECHO ===NUGET Publishing Version %VER% to %OUTDIR%
%NUGET% pack -Symbols -Version %VER% TestCop.nuspec -properties WAVEID=%WAVEID%
%NUGET% push Resharper.TestCop.R9.%VER%.symbols.nupkg -ApiKey XXX -Source %OUTDIR%
@ECHO === === === === === === === ===
