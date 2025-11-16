@echo off
REM EasyLogger Build Script
REM Builds a single-file executable for Windows

echo ======================================================================
echo Building EasyLogger
echo ======================================================================
echo.

REM Check if dotnet is available
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found
    echo Please install .NET SDK 8.0 or later from:
    echo https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

REM Show .NET version
echo .NET SDK version:
dotnet --version
echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
echo.

REM Restore dependencies
echo Restoring dependencies...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Failed to restore dependencies
    pause
    exit /b 1
)
echo.

REM Build the project
echo Building release version...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

REM Copy appsettings.json and log_viewer.html to output directory
echo Copying configuration and viewer files...
copy /Y "appsettings.json" "bin\Release\net8.0-windows\win-x64\publish\" >nul
copy /Y "log_viewer.html" "bin\Release\net8.0-windows\win-x64\publish\" >nul
echo.

echo ======================================================================
echo Build completed successfully!
echo ======================================================================
echo.
echo Output location:
echo   bin\Release\net8.0-windows\win-x64\publish\EasyLogger.exe
echo.
echo File size:
dir "bin\Release\net8.0-windows\win-x64\publish\EasyLogger.exe" | find "EasyLogger.exe"
echo.

REM Check if we should open the folder
echo Press any key to open output folder, or Ctrl+C to exit...
pause >nul
explorer "bin\Release\net8.0-windows\win-x64\publish"
