#!/bin/bash
set -euo pipefail

#
# Build and test script for IntVue project (Bash/Linux/WSL)
#
# This script detects the platform architecture, builds the IntVue project in Debug mode,
# and runs the test suite if the build succeeds.
#
# Usage: ./build-and-test.sh
#

# Colors for output
readonly COLOR_GREEN='\033[0;32m'
readonly COLOR_RED='\033[0;31m'
readonly COLOR_CYAN='\033[0;36m'
readonly COLOR_YELLOW='\033[1;33m'
readonly COLOR_RESET='\033[0m'

# Output functions
write_info() {
    echo -e "${COLOR_CYAN}ℹ️  $1${COLOR_RESET}"
}

write_success() {
    echo -e "${COLOR_GREEN}✅ $1${COLOR_RESET}"
}

write_error() {
    echo -e "${COLOR_RED}❌ $1${COLOR_RESET}"
}

write_warning() {
    echo -e "${COLOR_YELLOW}⚠️  $1${COLOR_RESET}"
}

# Detect platform
write_info "Detecting platform architecture..."

# Determine architecture
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux/WSL detection
    ARCH=$(uname -m)
    case "$ARCH" in
        x86_64)
            PLATFORM="x64"
            ;;
        aarch64)
            PLATFORM="arm64"
            ;;
        armv7l)
            PLATFORM="arm"
            ;;
        *)
            PLATFORM="$ARCH"
            ;;
    esac
elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    # Git Bash / Cygwin on Windows
    if [[ "${PROCESSOR_ARCHITECTURE}" == "AMD64" ]]; then
        PLATFORM="x64"
    else
        PLATFORM="${PROCESSOR_ARCHITECTURE}"
    fi
else
    # Default to x64
    PLATFORM="x64"
fi

write_success "Platform detected: $PLATFORM"

# Get project root (directory where this script is located)
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
write_info "Project root: $PROJECT_ROOT"

# Change to project directory
cd "$PROJECT_ROOT"

# Build
write_info ""
write_info "Building IntVue project in Debug mode..."
write_info "Running: dotnet build -c Debug -p:Platform=$PLATFORM"
write_info ""

if dotnet build -c Debug -p:Platform=$PLATFORM; then
    write_success ""
    write_success "Build succeeded!"
    write_info ""

    # Run tests
    write_info "Running tests..."
    write_info "Running: dotnet test Tests/IntVue.Tests/IntVue.Tests.csproj -c Debug -p:Platform=$PLATFORM"
    write_info ""

    test_output=$(dotnet test Tests/IntVue.Tests/IntVue.Tests.csproj -c Debug -p:Platform=$PLATFORM 2>&1)
    test_exit_code=$?

    if [ $test_exit_code -eq 0 ]; then
        write_success ""
        # Extract test count from output
        if test_count=$(echo "$test_output" | grep -oP 'Passed:\s+\K\d+' | head -1); then
            if [ -n "$test_count" ]; then
                write_success "Tests passed! All $test_count tests are working correctly."
            else
                write_success "Tests passed! All tests are working correctly."
            fi
        else
            write_success "Tests passed! All tests are working correctly."
        fi
        write_info ""
        exit 0
    else
        write_error ""
        write_error "Tests failed! Please review the output above."
        write_info ""
        exit 1
    fi
else
    write_error ""
    write_error "Build failed! Please fix the errors above before running tests."
    write_info ""
    exit 1
fi
