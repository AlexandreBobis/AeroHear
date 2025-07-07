@echo off
setlocal

REM === Configuration ===
set PROJECT_NAME=AeroHear
set CONFIG=Release
set FRAMEWORK=net8.0-windows
set RUNTIME=win-x64
set OUTPUT=bin\%CONFIG%\%FRAMEWORK%\%RUNTIME%\publish
set ZIP_NAME=%PROJECT_NAME%.zip

echo.
echo 🔧 Build .exe for %PROJECT_NAME%
echo -----------------------------------------

REM === Restore & publish ===
dotnet restore
dotnet publish -c %CONFIG% -r %RUNTIME% --self-contained true ^
  /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true /p:PublishTrimmed=false

IF ERRORLEVEL 1 (
    echo ❌ Échec du build.
    pause
    exit /b 1
)

REM === Suppression de l’ancien zip ===
if exist %ZIP_NAME% del %ZIP_NAME%

REM === Compression ===
powershell -Command "Compress-Archive -Path '%OUTPUT%\*' -DestinationPath '%ZIP_NAME%'"

IF EXIST %ZIP_NAME% (
    echo ✅ Build terminé : %ZIP_NAME%
) ELSE (
    echo ❌ Erreur de compression ZIP.
)

pause
