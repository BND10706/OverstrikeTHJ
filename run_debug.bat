@echo off
echo -----------------------------------------------
echo Overstrike - Build and Run with Debug Console
echo -----------------------------------------------

REM Set the startup project
set PROJECT=Overstrike\Overstrike.csproj

REM Clean up any previous logs
if exist "Overstrike\bin\Debug\net8.0-windows\overstrike_debug.log" del "Overstrike\bin\Debug\net8.0-windows\overstrike_debug.log"

echo Building solution...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo Build failed with error code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
echo.
echo Starting application with console output...
echo Look for the application window - it should appear shortly.
echo If nothing appears, check the debug log for errors.
echo.
echo Press Ctrl+C to terminate or close this window when finished.
echo.

REM Run the application
dotnet run --project %PROJECT%

echo.
if exist "Overstrike\bin\Debug\net8.0-windows\overstrike_debug.log" (
    echo Debug log file contents:
    echo ------------------------
    type "Overstrike\bin\Debug\net8.0-windows\overstrike_debug.log"
) else (
    echo No debug log file was created.
)

echo.
pause
