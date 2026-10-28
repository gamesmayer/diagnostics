#!/usr/bin/env sh

# Shared by lint-cs.sh and repository adapters. Collect once, then match any number of scopes.
# collect: 0 success, 2 Git error. match: 0 selected, 1 unchanged.
lint_changes_collect() {
    LINT_CHANGES_PREFIX=$(git rev-parse --show-prefix) || return 2
    LINT_CHANGES_ROOT=$(git rev-parse --show-toplevel) || return 2
    case "$1" in
        --staged)
            git -C "$LINT_CHANGES_ROOT" -c core.quotepath=false diff --cached --no-ext-diff --no-relative --name-only --no-renames > "$2" || return 2
            ;;
        --changed)
            git -C "$LINT_CHANGES_ROOT" -c core.quotepath=false diff --no-ext-diff --no-relative --name-only --no-renames HEAD > "$2" || return 2
            git -C "$LINT_CHANGES_ROOT" -c core.quotepath=false ls-files --full-name --others --exclude-standard >> "$2" || return 2
            ;;
        *) echo "[C# Linter] Expected --changed or --staged." >&2; return 2 ;;
    esac
}

lint_changes_match() (
    lc_paths=$1
    shift
    while IFS= read -r lc_path; do
        # Git quotes unusual names (including newlines). Select conservatively rather than miss a change.
        case "$lc_path" in \"*) return 0 ;; esac
        for lc_scope in "$@"; do
            while [ "${lc_scope#./}" != "$lc_scope" ]; do lc_scope=${lc_scope#./}; done
            lc_scope=${LINT_CHANGES_PREFIX:-}${lc_scope%/}
            case "$lc_scope" in */.) lc_scope=${lc_scope%/.} ;; esac
            # An explicitly watched file can be JSON or another non-C# input.
            [ "$lc_path" != "$lc_scope" ] || return 0
            case "$lc_path" in
                *.cs|*.csproj|*.props|*.targets|*.asmdef|*.asmref|*.slnx|*.sln|*.editorconfig|*.globalconfig|*.ruleset|*/global.json|global.json) ;;
                *) continue ;;
            esac
            case "$lc_scope" in ''|.) return 0 ;; esac
            case "$lc_path" in "$lc_scope"/*) return 0 ;; esac
            # Settings in an ancestor directory apply to its descendants too.
            case "$lc_path" in
                *.editorconfig|*.props|*.targets|*.globalconfig|*/global.json|global.json)
                    case "$lc_path" in */*) lc_parent=${lc_path%/*} ;; *) return 0 ;; esac
                    case "$lc_scope" in "$lc_parent"|"$lc_parent"/*) return 0 ;; esac
                    ;;
            esac
        done
    done < "$lc_paths"
    return 1
)
