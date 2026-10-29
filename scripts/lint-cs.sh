#!/usr/bin/env sh
set -eu

work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
trap 'exit 1' HUP INT TERM
: > "$work/dirs"
: > "$work/watch"
fix=false
sln=""
mode=""
for arg in "$@"; do
    case "$arg" in
        --fix) fix=true ;;
        --dir=*) printf '%s\n' "${arg#--dir=}" >> "$work/dirs" ;;
        --sln=*) sln=${arg#--sln=} ;;
        --watch=*) printf '%s\n' "${arg#--watch=}" >> "$work/watch" ;;
        --changed|--staged)
            if [ -n "$mode" ] && [ "$mode" != "$arg" ]; then
                echo '[C# Linter] --changed and --staged cannot be combined.' >&2
                exit 1
            fi
            mode=$arg
            ;;
        *) echo "[C# Linter] Unknown argument: $arg" >&2; exit 1 ;;
    esac
done
[ -s "$work/dirs" ] || printf '.\n' > "$work/dirs"

if [ -n "$mode" ]; then
    . "$(dirname "$0")/lint-changes.sh"
    lint_changes_collect "$mode" "$work/changes"
    if [ -n "$sln" ]; then
        printf '%s\n' "$sln" >> "$work/watch"
    else
        for candidate in ./*.slnx; do
            [ ! -f "$candidate" ] || printf '%s\n' "$candidate" >> "$work/watch"
        done
    fi
    set --
    while IFS= read -r path; do set -- "$@" "$path"; done < "$work/watch"
    : > "$work/selected"
    while IFS= read -r dir; do
        if lint_changes_match "$work/changes" "$dir" "$@"; then
            printf '%s\n' "$dir" >> "$work/selected"
        fi
    done < "$work/dirs"
    mv "$work/selected" "$work/dirs"
    if [ ! -s "$work/dirs" ]; then
        echo '[C# Linter] No relevant changes.'
        exit 0
    fi
fi

if [ -z "$sln" ]; then
    set -- *.slnx
    if [ "$#" -ne 1 ] || [ ! -f "$1" ]; then
        echo "[C# Linter] Expected exactly one .slnx in $(pwd). Pass --sln=<file> to choose one." >&2
        exit 1
    fi
    sln=$1
fi

# dotnet format accepts directory paths; pass them as separate arguments, without glob patterns.
set --
while IFS= read -r dir; do set -- "$@" "$dir"; done < "$work/dirs"
snapshot_sources() {
    set --
    while IFS= read -r dir; do set -- "$@" "$dir"; done < "$work/dirs"
    find "$@" -name '*.cs' -exec cksum {} + | sort
}
if "$fix"; then snapshot_sources > "$work/before"; fi
file_count=$(find "$@" -name '*.cs' | wc -l | tr -d ' ')
printf '[C# Linter] %s: %s file(s)\n' "$sln" "$file_count"
# A trailing slash makes each include a directory prefix in dotnet format.
set -- "$sln" --include
while IFS= read -r dir; do set -- "$@" "${dir%/}/"; done < "$work/dirs"
set -- "$@" --severity warn
if ! "$fix"; then set -- "$@" --verify-no-changes; fi
exit_code=0
output_style=$(dotnet format style "$@" 2>&1) || exit_code=1
if "$fix"; then
    # Fixing a layout anchor can expose diagnostics already processed by dotnet format
    # (e.g. a fluent-chain indent moves the opening parenthesis after its arguments were fixed).
    # Style runs once; repeat analyzer fixes on their own output until it stops changing.
    pass=0
    while :; do
        snapshot_sources > "$work/pass-before"
        output_analyzers=$(dotnet format analyzers "$@" 2>&1) || exit_code=1
        snapshot_sources > "$work/pass-after"
        pass=$((pass + 1))
        if [ "$exit_code" -ne 0 ] || cmp -s "$work/pass-before" "$work/pass-after"; then break; fi
        if [ "$pass" -ge 6 ]; then
            echo '[C# Linter] Analyzer fixes did not stabilize after 6 passes.' >&2
            exit_code=1
            break
        fi
    done
    # Success means the corrected source passes a fresh check, not merely that fixers ran.
    output_style_check=$(dotnet format style "$@" --verify-no-changes 2>&1) || exit_code=1
    output_analyzers_check=$(dotnet format analyzers "$@" --verify-no-changes 2>&1) || exit_code=1
    output_style=$(printf '%s\n%s' "$output_style" "$output_style_check")
    output_analyzers=$(printf '%s\n%s' "$output_analyzers" "$output_analyzers_check")
else
    output_analyzers=$(dotnet format analyzers "$@" 2>&1) || exit_code=1
fi
output=$(printf '%s\n%s' "$output_style" "$output_analyzers")
printf '%s\n' "$output" | sed 's| \[[^]]*\.csproj\]$||' | grep -v 'Warnings were encountered while loading the workspace' || true
issue_count=$(printf '%s\n' "$output" | grep -c ': warning\|: error\|Unable to fix' || true)

if "$fix"; then
    snapshot_sources > "$work/after"
    fixed_files=$(diff "$work/before" "$work/after" | grep '^>' | sed 's/^> [0-9]* [0-9]* //')
    fixed_count=$(printf '%s\n' "$fixed_files" | grep -c '.' || true)
    if [ "$fixed_count" -gt 0 ]; then
        echo "[C# Linter] Fixed $fixed_count file(s):"
        printf '%s\n' "$fixed_files" | sed 's/^/  /'
    fi
fi
if [ "$exit_code" -ne 0 ] || { "$fix" && [ "$issue_count" -gt 0 ]; }; then
    echo "[C# Linter] $issue_count issue(s) found. Run with --fix or fix the remaining errors manually."
    exit 1
fi
echo '[C# Linter] No issues found.'
