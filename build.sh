#!/bin/bash

# IntVue Debug Self-Contained Builder
# Builds IntVue in Debug mode as a self-contained executable for testing
# Usage: ./build.sh
# Output: bin/Debug/{TargetFramework}/win-{Platform}/publish/IntVue.exe

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}IntVue Debug Self-Contained Builder${NC}"
echo "========================================"

# Detect platform architecture
PROCESSOR=$(uname -m)
case "$PROCESSOR" in
  x86_64)
    Platform="x64"
    ;;
  i686|i386)
    Platform="x86"
    ;;
  aarch64|arm64)
    Platform="ARM64"
    ;;
  *)
    echo -e "${RED}Error: Unsupported architecture: $PROCESSOR${NC}"
    exit 1
    ;;
esac

echo -e "${GREEN}Detected Platform: $Platform${NC}"

# Verify we're in the right directory
if [ ! -f "IntVue.csproj" ]; then
  echo -e "${RED}Error: IntVue.csproj not found. Please run this script from the IntVue root directory.${NC}"
  exit 1
fi

# Clean previous debug artifacts
echo -e "${YELLOW}Cleaning previous debug artifacts...${NC}"
if [ -d "bin/Debug" ]; then
  find bin/Debug -name "publish" -type d -exec rm -rf {} + 2>/dev/null || true
fi

# Build IntVue as self-contained single file (Debug)
echo -e "${YELLOW}Building IntVue as self-contained (Debug)...${NC}"
dotnet publish IntVue.csproj \
  -c Debug \
  -p:Platform=$Platform \
  -r win-$Platform \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false \
  -p:PublishReadyToRun=false \
  -p:DebugType=embedded \
  -p:DebugSymbols=true

# Check if build succeeded
if [ $? -eq 0 ]; then
  echo -e "${GREEN}✓ Build completed successfully${NC}"

  # Find the executable (search dynamically since path includes target framework)
  EXE_PATH=$(find bin/Debug -name "IntVue.exe" -type f 2>/dev/null | head -1)

  if [ -n "$EXE_PATH" ] && [ -f "$EXE_PATH" ]; then
    EXE_SIZE=$(ls -lh "$EXE_PATH" | awk '{print $5}')
    BUILD_TIME=$(date +"%Y-%m-%d %H:%M:%S")

    echo -e "${GREEN}✓ Debug executable ready for testing${NC}"
    echo ""
    echo -e "${BLUE}Build Information:${NC}"
    echo -e "  ${GREEN}Location:${NC} $EXE_PATH"
    echo -e "  ${GREEN}Size:${NC} $EXE_SIZE"
    echo -e "  ${GREEN}Built:${NC} $BUILD_TIME"
    echo -e "  ${GREEN}Configuration:${NC} Debug (with embedded symbols)"
    echo ""
    echo -e "${YELLOW}Next steps:${NC}"
    echo "1. Copy '$EXE_PATH' to your target machine"
    echo "2. Run: IntVue.exe"
    echo "3. Test camera preview, recording, and other features"
    echo ""
    echo -e "${BLUE}Debug Build Notes:${NC}"
    echo "- Embedded debug symbols for better diagnostics"
    echo "- Full .NET runtime included (self-contained)"
    echo "- No trimming applied for complete functionality"
    echo "- Suitable for testing and debugging on target machines"
    echo ""
    exit 0
  else
    echo -e "${RED}Error: Executable not found in bin/Debug directory${NC}"
    echo -e "${RED}Please check the build output in bin/Debug/${NC}"
    exit 1
  fi
else
  echo -e "${RED}✗ Build failed${NC}"
  exit 1
fi
