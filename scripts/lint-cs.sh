#!/usr/bin/env sh

set -e

FIX=false
DIR="."
SLN=""

for arg in "$@"; do
    case "$arg" in
        --fix) FIX=true ;;
        --dir=*) DIR="${arg#--dir=}" ;;
        --sln=*) SLN="${arg#--sln=}" ;;
    esac
done

if [ -z "$SLN" ]; then
    set -- *.slnx
    if [ "$#" -ne 1 ] || [ ! -f "$1" ]; then
        echo "[C# Linter] Expected exactly one .slnx in $(pwd). Pass --sln=<file> to choose one."
        exit 1
    fi
    SLN="$1"
fi

if [ "$DIR" = "." ]; then
    INCLUDE_PATTERN="**/*.cs"
else
    INCLUDE_PATTERN="$DIR/**/*.cs"
fi

if $FIX; then
    echo "[C# Linter] Fixing files..."
    VERIFY_ARG=""
else
    echo "[C# Linter] Checking files..."
    VERIFY_ARG="--verify-no-changes"
fi

if $FIX; then
    pre_checksums=$(mktemp)
    find "$DIR" -name "*.cs" | sort | xargs cksum 2>/dev/null > "$pre_checksums" || true
fi

file_count=$(find "$DIR" -name "*.cs" | wc -l | tr -d ' ')
echo "[C# Linter] Analyzing $file_count file(s)..."

exit_code=0
output_style=$(dotnet format style "$SLN" --include "$INCLUDE_PATTERN" --severity warn $VERIFY_ARG 2>&1) || exit_code=1
output_analyzers=$(dotnet format analyzers "$SLN" --include "$INCLUDE_PATTERN" --severity warn $VERIFY_ARG 2>&1) || exit_code=1
output=$(printf '%s\n%s' "$output_style" "$output_analyzers")
printf '%s\n' "$output" | sed 's| \[[^]]*\.csproj\]$||' | grep -v "Warnings were encountered while loading the workspace" || true
issue_count=$(printf '%s\n' "$output" | grep -c ': warning\|: error\|Unable to fix' || true)

if $FIX; then
    post_checksums=$(mktemp)
    find "$DIR" -name "*.cs" | sort | xargs cksum 2>/dev/null > "$post_checksums" || true
    fixed_files=$(diff "$pre_checksums" "$post_checksums" | grep '^>' | awk '{print $NF}')
    rm -f "$pre_checksums" "$post_checksums"
    fixed_count=$(printf '%s\n' "$fixed_files" | grep -c '.' || true)
    if [ "$fixed_count" -gt 0 ]; then
        echo ""
        echo "[C# Linter] Fixed $fixed_count file(s):"
        printf '%s\n' "$fixed_files" | sed 's/^/  /'
    fi
fi

if [ "$exit_code" -ne 0 ]; then
    echo ""
    if $FIX; then
        echo "[C# Linter] $issue_count issue(s) found. Fix the errors above."
    else
        echo "[C# Linter] $issue_count issue(s) found. Run with --fix to apply fixes automatically."
    fi
    exit 1
fi

if $FIX; then
    if [ "$issue_count" -gt 0 ]; then
        echo ""
        echo "[C# Linter] $issue_count issue(s) could not be fixed automatically. Fix them manually."
        exit 1
    elif [ "$fixed_count" -gt 0 ]; then
        echo "[C# Linter] Done."
    else
        echo "[C# Linter] No issues found."
    fi
else
    echo "[C# Linter] No issues found."
fi