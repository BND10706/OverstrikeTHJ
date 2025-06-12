#!/bin/zsh
echo "Building and running Overstrike with console output..."
dotnet build
if [ $? -ne 0 ]; then
    echo "Build failed"
    read -p "Press any key to continue..."
    exit 1
fi

echo "Build successful. Running application..."
dotnet run --project Overstrike/Overstrike.csproj

echo "Application started. Press any key to close this window."
read -p "Press any key to continue..."
