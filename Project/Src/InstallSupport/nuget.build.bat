REM Called from AfterBuild Step within csproj
@ECHO === === === === === === === ===
SET NUGET=..\..\..\..\InstallSupport\NuGet.exe
SET OUTDIR=D:\temp
SET VER=%1

@ECHO ===NUGET Publishing Version %VER% to %OUTDIR%
%NUGET% pack -Symbols -Version %VER% TestCop.nuspec
%NUGET% push Resharper.TestCop.%VER%.symbols.nupkg -ApiKey XXX -Source %OUTDIR%
@ECHO === === === === === === === ===
