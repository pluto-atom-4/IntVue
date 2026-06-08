#!/bin/bash
#
# SimpleCapture Self-Contained Publish Script
# Publishes as standalone exe with .NET runtime included
#
# Usage:
#   ./publish.sh              (defaults: Debug, x64)
#   ./publish.sh Release x64
#   ./publish.sh Debug ARM64
#

set -e

# Configuration
CONFIG=${1:-Debug}
PLATFORM=${2:-x64}
RID="win-$PLATFORM"
PUBLISH_DIR="bin/$PLATFORM/$CONFIG/net10.0-windows10.0.26100.0/$RID/publish"
EXE_NAME="SimpleCapture.exe"

echo "=============================================="
echo "SimpleCapture Self-Contained Publish"
echo "=============================================="
echo ""
echo "Configuration: $CONFIG"
echo "Platform: $PLATFORM"
echo "Self-Contained: Yes (includes .NET runtime)"
echo ""

# Kill running processes
echo "Stopping any running SimpleCapture instances..."
pkill -f SimpleCapture.exe 2>/dev/null || true
sleep 1
echo "Done"
echo ""

# Clean
echo "Cleaning previous build..."
dotnet clean -c "$CONFIG" -p:Platform="$PLATFORM" > /dev/null 2>&1 || true
echo "Done"
echo ""

# Publish
echo "Publishing as self-contained executable..."
echo "This may take 1-2 minutes (bundling .NET runtime)..."
echo ""

if dotnet publish -c "$CONFIG" -p:Platform="$PLATFORM" --self-contained true --runtime "$RID"; then
    echo ""
    echo "✓ Publish succeeded!"
    echo ""
else
    echo ""
    echo "✗ Publish failed!"
    exit 1
fi

# Verify executable exists
if [ -f "$PUBLISH_DIR/$EXE_NAME" ]; then
    EXE_SIZE=$(du -h "$PUBLISH_DIR/$EXE_NAME" | cut -f1)
    echo "✓ SimpleCapture.exe ($EXE_SIZE)"
else
    echo "✗ SimpleCapture.exe not found!"
    exit 1
fi

# Count DLLs
DLL_COUNT=$(find "$PUBLISH_DIR" -name "*.dll" | wc -l)
echo "✓ $DLL_COUNT DLLs included (.NET runtime + WinUI)"

# Total size
TOTAL_SIZE=$(du -sh "$PUBLISH_DIR" | cut -f1)
echo "✓ Total publish size: $TOTAL_SIZE"
echo ""

echo "Published files located at:"
echo "  $PUBLISH_DIR"
echo ""
echo "Next steps:"
echo "  1. Copy entire publish folder to USB drive"
echo "  2. Transfer to Surface Pro 7"
echo "  3. Run: SimpleCapture.exe"
echo "     (No .NET installation needed!)"
echo ""
