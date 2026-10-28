#!/usr/bin/env sh

# Run a repository's declarative .lint.sh. All discovery, selection and ordering live here.
set -eu
tools=$(CDPATH='' cd "$(dirname "$0")" && pwd)
root=$(git rev-parse --show-toplevel)
cd "$root"
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
trap 'exit 1' HUP INT TERM
for file in groups solutions unity before watch targets selected named; do : > "$work/$file"; done
mode=""
fix=""
for arg in "$@"; do
    case "$arg" in
        --fix) fix=--fix ;;
        --changed|--staged)
            if [ -n "$mode" ] && [ "$mode" != "$arg" ]; then
                echo '[lint] --changed and --staged cannot be combined.' >&2
                exit 1
            fi
            mode=$arg
            ;;
        --*) echo "[lint] Unknown argument: $arg" >&2; exit 1 ;;
        *) printf '%s\n' "$arg" >> "$work/named" ;;
    esac
done

# The config only declares repository inputs; it does not implement the runner.
lint_groups() { printf '%s\n' "$@" >> "$work/groups"; }
lint_solution() { printf '%s\n' "$@" >> "$work/solutions"; }
lint_watch() { printf '%s\n' "$@" >> "$work/watch"; }
lint_before_unity() { printf '%s\n' "$@" > "$work/before"; }
lint_unity() {
    lu_sln=""
    for lu_arg in "$@"; do
        case "$lu_arg" in --sln=*) lu_sln=${lu_arg#--sln=} ;; esac
    done
    for lu_host in "$@"; do
        case "$lu_host" in --sln=*) continue ;; esac
        lu_host=${lu_host%/}
        printf '%s\t%s\n' "$lu_host" "${lu_sln:-${lu_host##*/}.slnx}" >> "$work/unity"
    done
}
if [ ! -f .lint.sh ]; then
    echo '[lint] Declare lint_solution and/or lint_unity in .lint.sh.' >&2
    exit 1
fi
. ./.lint.sh

# Normalize paths without requiring their files (or generated Unity solutions) to exist yet.
lint_path() {
    printf '%s\n' "$1" | awk -F/ '
        /^\// { print "[lint] Paths must be repository-relative." > "/dev/stderr"; exit 1 }
        { n=0; for(i=1;i<=NF;i++) {
            if($i=="" || $i==".") continue;
            if($i=="..") { if(n==0) exit 1; n--; } else p[++n]=$i;
          }
          if(n==0) print "."; else { for(i=1;i<=n;i++) printf "%s%s", (i>1?"/":""),p[i]; print ""; }
        }'
}
lint_scope() (
    ls_dir=$1
    ls_scope=$1
    ls_length=0
    while IFS= read -r ls_group; do
        ls_group=$(lint_path "$ls_group")
        case "$ls_dir" in
            "$ls_group"|"$ls_group"/*)
                if [ "${#ls_group}" -gt "$ls_length" ]; then ls_scope=$ls_group; ls_length=${#ls_group}; fi
                ;;
        esac
    done < "$work/groups"
    printf '%s\n' "$ls_scope"
)
lint_target() {
    lt_type=$1
    lt_sln=$(lint_path "$2")
    lt_dir=$(lint_path "$3")
    lt_scope=$(lint_scope "$4")
    printf '%s\t%s\t%s\t%s\n' "$lt_type" "$lt_sln" "$lt_dir" "$lt_scope" >> "$work/targets"
}

# Read the actual project list instead of repeating project names in each repository's config.
while IFS= read -r solution; do
    solution=$(lint_path "$solution")
    dotnet sln "$solution" list > "$work/projects"
    case "$solution" in */*) solution_dir=${solution%/*} ;; *) solution_dir=. ;; esac
    # Root solutions are repository-wide inputs, including for Unity targets.
    case "$solution" in */*) ;; *) printf '%s\n' "$solution" >> "$work/watch" ;; esac
    while IFS= read -r project; do
        project=$(printf '%s' "$project" | tr -d '\r')
        case "$project" in *.csproj) ;; *) continue ;; esac
        project=$(lint_path "$solution_dir/$project")
        case "$project" in */*) dir=${project%/*} ;; *) dir=. ;; esac
        lint_target dotnet "$solution" "$dir" "$dir"
    done < "$work/projects"
done < "$work/solutions"

tab=$(printf '\t')
while IFS="$tab" read -r host solution; do
    host=$(lint_path "$host")
    if [ ! -f "$host/ProjectSettings/ProjectVersion.txt" ]; then
        echo "[lint] Not a Unity project: $host" >&2
        exit 1
    fi
    solution="$host/$solution"
    # Only authored code: never vendored plugins or the read-only copies of shared .NET projects.
    if [ -d "$host/Assets/Source" ]; then lint_target unity "$solution" "$host/Assets/Source" "$host"; fi
    for pkg in "$host"/Packages/*; do
        [ -f "$pkg/package.json" ] || continue
        for dir in "$pkg/Source" "$pkg/Tests"; do
            [ ! -d "$dir" ] || lint_target unity "$solution" "$dir" "$host"
        done
    done
done < "$work/unity"

# Validate explicit group/project names before executing anything.
while IFS= read -r name; do
    found=false
    while IFS="$tab" read -r type solution dir scope; do
        if [ "$name" = "$scope" ] || [ "$name" = "${scope##*/}" ]; then found=true; break; fi
    done < "$work/targets"
    if ! "$found"; then echo "[lint] Unknown group or project: $name" >&2; exit 1; fi
done < "$work/named"

. "$tools/lint-changes.sh"
if [ -n "$mode" ] && [ ! -s "$work/named" ]; then lint_changes_collect "$mode" "$work/changes"; fi
set -- .lint.sh
while IFS= read -r path; do set -- "$@" "$path"; done < "$work/watch"
while IFS="$tab" read -r type solution dir scope; do
    if [ -s "$work/named" ]; then
        if ! grep -Fxq "$scope" "$work/named" && ! grep -Fxq "${scope##*/}" "$work/named"; then continue; fi
    elif [ -n "$mode" ]; then
        lint_changes_match "$work/changes" "$scope" "$solution" "$@" || continue
    fi
    printf '%s\t%s\t%s\n' "$type" "$solution" "$dir" >> "$work/selected"
done < "$work/targets"
if [ ! -s "$work/selected" ]; then echo '[lint] No C# changes to lint.'; exit 0; fi

ok=0
for phase in dotnet unity; do
    awk -F '\t' -v phase="$phase" '$1==phase {print $2}' "$work/selected" | sort -u > "$work/run-solutions"
    if [ "$phase" = unity ] && [ -s "$work/run-solutions" ] && [ -s "$work/before" ]; then
        set --
        while IFS= read -r arg; do set -- "$@" "$arg"; done < "$work/before"
        "$@" || exit 1
    fi
    while IFS= read -r solution; do
        awk -F '\t' -v phase="$phase" -v solution="$solution" '$1==phase && $2==solution {print $3}' "$work/selected" | sort -u > "$work/run-dirs"
        set -- "--sln=$solution"
        while IFS= read -r dir; do set -- "$@" "--dir=$dir"; done < "$work/run-dirs"
        sh "$tools/lint-cs.sh" "$@" ${fix:+"$fix"} || ok=1
    done < "$work/run-solutions"
done
exit "$ok"
