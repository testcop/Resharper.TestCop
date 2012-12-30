@ECHO OFF
SET TARGET=%APPDATA%\JetBrains\ReSharper\v7.0\Plugins\TestCop

@ECHO ===============================================================
IF NOT EXIST %TARGET%\nul mkdir %TARGET%

@ECHO Releasing plugin to %TARGET%
COPY TestCop*.dll %TARGET%

@ECHO ===============================================================
@ECHO Plugin will register when Visual Studio next starts.
@ECHO To Uninstall simply delete this folder
@ECHO %TARGET%
@ECHO ===============================================================

PAUSE