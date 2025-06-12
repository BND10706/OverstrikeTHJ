@echo off
echo Building and running Overstrike with console output...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo Build failed with error %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo Build successful. Running application...
start dotnet run --project Overstrike\Overstrike.csproj

echo Application started. Press any key to close this window.
pause
