#!/bin/bash

# IntVue Self-Contained Publisher
# Publishes IntVue as a standalone executable with bundled .NET runtime
# Usage: ./publish.sh
# Output: bin/Release/{Platform}/publish/IntVue.exe

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}IntVue Self-Contained Publisher${NC}"
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

# Clean previous publish artifacts
echo -e "${YELLOW}Cleaning previous publish artifacts...${NC}"
if [ -d "bin/Release/$Platform/publish" ]; then
  rm -rf "bin/Release/$Platform/publish"
fi

# Publish IntVue as self-contained single file
echo -e "${YELLOW}Publishing IntVue as self-contained...${NC}"
dotnet publish IntVue.csproj \
  -c Release \
  -p:Platform=$Platform \
  --self-contained \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false \
  -p:PublishReadyToRun=true \
  -p:DebugType=none \
  -p:DebugSymbols=false

# Check if publish succeeded
if [ $? -eq 0 ]; then
  echo -e "${GREEN}✓ Publish completed successfully${NC}"

  # Find the executable
  EXE_PATH="bin/Release/$Platform/publish/IntVue.exe"

  if [ -f "$EXE_PATH" ]; then
    EXE_SIZE=$(ls -lh "$EXE_PATH" | awk '{print $5}')
    echo -e "${GREEN}✓ Executable ready for deployment${NC}"
    echo ""
    echo -e "${GREEN}Location:${NC} $EXE_PATH"
    echo -e "${GREEN}Size:${NC} $EXE_SIZE"
    echo ""
    echo -e "${YELLOW}Next steps:${NC}"
    echo "1. Copy '$EXE_PATH' to your target machine"
    echo "2. Run: IntVue.exe"
    echo ""
    exit 0
  else
    echo -e "${RED}Error: Executable not found at $EXE_PATH${NC}"
    exit 1
  fi
else
  echo -e "${RED}✗ Publish failed${NC}"
  exit 1
fi
