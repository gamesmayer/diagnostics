#!/bin/bash

# Requires a compatible .NET SDK version to be active.
# The supported range is versioned together with GamesMayer.Diagnostics.
# Usage: ./require-dotnet.sh [min-version] [max-version]
# Examples:
#   ./require-dotnet.sh                 # Uses the framework defaults
#   ./require-dotnet.sh 3.0             # Override min version
#   ./require-dotnet.sh 6.0 7.0         # Override both versions

DEFAULT_MIN_VERSION="3.0"
DEFAULT_MAX_VERSION="10.0"

if ! command -v dotnet > /dev/null 2>&1; then
    echo "✗ Error: 'dotnet' is not installed. Install the .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

MIN_VERSION="${1:-$DEFAULT_MIN_VERSION}"
MAX_VERSION="${2:-$DEFAULT_MAX_VERSION}"

DOTNET_VERSION=$(dotnet --version)

# Returns 0 if version1 >= version2
version_gte() {
    printf '%s\n%s' "$2" "$1" | sort -V -r | head -n1 | grep -q "^$1"
    return $?
}

# Returns 0 if version1 <= version2
version_lte() {
    printf '%s\n%s' "$1" "$2" | sort -V -r | head -n1 | grep -q "^$2"
    return $?
}

if ! version_gte "$DOTNET_VERSION" "$MIN_VERSION"; then
    echo "✗ .NET SDK version $DOTNET_VERSION is below minimum required version $MIN_VERSION"
    exit 1
fi

if [ -n "$MAX_VERSION" ]; then
    if ! version_lte "$DOTNET_VERSION" "$MAX_VERSION"; then
        echo "✗ .NET SDK version $DOTNET_VERSION exceeds maximum allowed version $MAX_VERSION"
        exit 1
    fi
    echo "✓ .NET SDK version $DOTNET_VERSION is compatible (>= $MIN_VERSION and <= $MAX_VERSION)"
else
    echo "✓ .NET SDK version $DOTNET_VERSION is compatible (>= $MIN_VERSION)"
fi

exit 0
