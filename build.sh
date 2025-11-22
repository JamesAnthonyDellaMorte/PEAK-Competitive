#!/bin/bash
# Build script for PEAK Competitive mod

set -e  # Exit on error

echo "======================================"
echo "  PEAK Competitive - Build Script"
echo "======================================"
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found!"
    echo "Please install it from: https://dotnet.microsoft.com/download/dotnet/8.0"
    echo "Or run: brew install --cask dotnet-sdk"
    exit 1
fi

echo "✓ .NET SDK found: $(dotnet --version)"
echo ""

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf artifacts/bin
echo "✓ Cleaned"
echo ""

# Restore dependencies
echo "Restoring NuGet packages..."
dotnet restore src/PEAKCompetitive/PEAKCompetitive.csproj
echo "✓ Restored"
echo ""

# Build Release
echo "Building Release configuration..."
dotnet build src/PEAKCompetitive/PEAKCompetitive.csproj -c Release
echo "✓ Built"
echo ""

# Check if DLL was created
DLL_PATH="artifacts/bin/Release/PEAKCompetitive.dll"
if [ -f "$DLL_PATH" ]; then
    echo "======================================"
    echo "  Build Successful!"
    echo "======================================"
    echo ""
    echo "Output: $DLL_PATH"
    echo "Size: $(du -h "$DLL_PATH" | cut -f1)"
    echo ""
    echo "To install:"
    echo "  Copy to: PEAK/BepInEx/plugins/PEAKCompetitive.dll"
    echo ""
else
    echo "Error: Build failed - DLL not found"
    exit 1
fi
