#!/usr/bin/env sh
set -eu
scripts=$(CDPATH='' cd "$(dirname "$0")/.." && pwd)
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
trap 'exit 1' HUP INT TERM
mkdir -p "$work/bin"
export LINT_TEST_LOG="$work/calls" LINT_TEST_EVENTS="$work/events"
cat > "$work/bin/dotnet" <<'STUB'
#!/usr/bin/env sh
if [ "$1" = sln ]; then
    [ "${LINT_TEST_LIST_FAIL:-0}" = 0 ] || exit 1
    printf 'Project(s)\n----------\n'
    sed -n 's/.*<Project Path="\([^"]*\)".*/\1/p' "$2"
    exit 0
fi
[ "$1" = format ] || exit 1
printf '<%s> ' "$@" >> "$LINT_TEST_LOG"
printf '\n' >> "$LINT_TEST_LOG"
printf '%s %s %s\n' "$1" "$2" "$3" >> "$LINT_TEST_EVENTS"
case "$*" in *"${LINT_TEST_FAIL:-never-match}"*) exit 1 ;; esac
STUB
cat > "$work/bin/sync" <<'STUB'
#!/usr/bin/env sh
printf 'sync\n' >> "$LINT_TEST_EVENTS"
exit "${LINT_TEST_SYNC_FAIL:-0}"
STUB
chmod +x "$work/bin/dotnet" "$work/bin/sync"
export PATH="$work/bin:$PATH" LINT_TEST_SYNC="$work/bin/sync"

init() {
    mkdir -p "$work/$1"
    cd "$work/$1"
    git init -q
    git config user.email lint@example.test
    git config user.name 'Lint test'
}
source_dir() {
    mkdir -p "$1"
    printf 'class File {}\n' > "$1/File.cs"
}
unity_host() {
    mkdir -p "$1/ProjectSettings" "$1/Packages"
    printf 'm_EditorVersion: 6000.3.8f1\n' > "$1/ProjectSettings/ProjectVersion.txt"
    printf '<Solution />\n' > "$1/${2:-${1##*/}.slnx}"
}
commit() { git add .; git commit -qm baseline; }
reset_case() { git reset --hard -q HEAD; git clean -fdq; }
run() {
    expected=$1
    shift
    : > "$LINT_TEST_LOG"
    : > "$LINT_TEST_EVENTS"
    sh "$scripts/lint.sh" "$@" > "$work/output" 2>&1 || { cat "$work/output" >&2; exit 1; }
    actual=$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')
    [ "$actual" = "$expected" ] || { echo "Expected $expected calls, got $actual: $*" >&2; cat "$work/output" >&2; exit 1; }
}
fail() {
    : > "$LINT_TEST_LOG"
    : > "$LINT_TEST_EVENTS"
    if sh "$scripts/lint.sh" "$@" > "$work/output" 2>&1; then
        echo "Expected failure: $*" >&2; exit 1
    fi
}

init framework
cat > .lint.sh <<'CONFIG'
lint_groups projects/*/
lint_solution Framework.slnx
lint_unity projects/*/*.Client --sln=client.slnx
lint_before_unity "$LINT_TEST_SYNC"
CONFIG
cat > Framework.slnx <<'SLN'
<Solution>
  <Project Path="projects/Economy/Economy.Shared/Economy.Shared.csproj" />
  <Project Path="projects/Economy/Economy.Server/Economy.Server.csproj" />
  <Project Path="projects/Testing/Testing.Server/Testing.Server.csproj" />
</Solution>
SLN
for dir in projects/Economy/Economy.Shared projects/Economy/Economy.Server projects/Testing/Testing.Server; do source_dir "$dir"; done
for name in Core Economy; do
    host="projects/$name/$name.Client"
    unity_host "$host" client.slnx
    pkg="$host/Packages/com.gamesmayer.$name"
    mkdir -p "$pkg"
    printf '{}\n' > "$pkg/package.json"
    source_dir "$pkg/Source"
    source_dir "$pkg/Tests"
    source_dir "$pkg/Plugins"
    source_dir "$pkg/GamesMayer.Shared"
done
printf '{}\n' > global.json
printf '<Project />\n' > projects/Modules.props
commit
run 0 --changed
run 0 --staged
printf 'documentation\n' > projects/Core/README.md
run 0 --changed
printf '// edit\n' >> projects/Core/Core.Client/Packages/com.gamesmayer.Core/Source/File.cs
run 2 --changed
[ "$(sed -n '1p' "$LINT_TEST_EVENTS")" = sync ]
run 0 --staged
git add projects/Core
run 2 --staged
reset_case
printf '// edit\n' >> projects/Economy/Economy.Shared/File.cs
run 4 --changed --fix
# Both .NET projects in the selected module share a single workspace run.
sed -n '1p' "$LINT_TEST_LOG" | grep -Fq '<projects/Economy/Economy.Shared/>'
sed -n '1p' "$LINT_TEST_LOG" | grep -Fq '<projects/Economy/Economy.Server/>'
[ "$(sed -n '3p' "$LINT_TEST_EVENTS")" = sync ]
if grep -q -- '--verify-no-changes' "$LINT_TEST_LOG"; then exit 1; fi
if grep -q 'Plugins\|GamesMayer.Shared' "$LINT_TEST_LOG"; then exit 1; fi
reset_case
printf '// edit\n' >> projects/Testing/Testing.Server/File.cs
run 2 --changed
if grep -q sync "$LINT_TEST_EVENTS"; then exit 1; fi
reset_case
for config in global.json projects/Modules.props Framework.slnx .lint.sh; do
    printf '\n' >> "$config"
    run 6 --changed
    reset_case
done
# Multiple modules still load the .NET solution only once, and synchronize only once.
run 6
[ "$(grep -c '<style> <Framework.slnx>' "$LINT_TEST_LOG")" = 1 ]
[ "$(grep -c '^sync$' "$LINT_TEST_EVENTS")" = 1 ]
[ "$(sed -n '3p' "$LINT_TEST_EVENTS")" = sync ]
run 2 Core
run 4 Economy Testing
run 4 --changed Economy
fail Missing
fail --changed --staged
fail --unknown
export LINT_TEST_LIST_FAIL=1
fail --changed
unset LINT_TEST_LIST_FAIL
export LINT_TEST_FAIL='style Framework.slnx'
fail Economy
# A failing .NET lint is reported even if the Unity phase succeeds.
grep -q '^sync$' "$LINT_TEST_EVENTS"
unset LINT_TEST_FAIL
export LINT_TEST_SYNC_FAIL=1
fail Core
[ ! -s "$LINT_TEST_LOG" ]
unset LINT_TEST_SYNC_FAIL
rm .lint.sh
fail
reset_case

init game
cat > .lint.sh <<'CONFIG'
lint_solution Game.slnx
lint_unity projects/Game.Client
CONFIG
cat > Game.slnx <<'SLN'
<Solution>
  <Project Path="projects/Game.Server/Game.Server.csproj" />
  <Project Path="projects/Game.Server.Tests/Game.Server.Tests.csproj" />
  <Project Path="projects/Game.Shared/Game.Shared.csproj" />
  <Project Path="projects/Game.Shared.Tests/Game.Shared.Tests.csproj" />
</Solution>
SLN
unity_host projects/Game.Client
source_dir projects/Game.Client/Assets/Source
for dir in Game.Server Game.Server.Tests Game.Shared Game.Shared.Tests; do source_dir "projects/$dir"; done
printf '{}\n' > global.json
commit
run 4
for dir in Game.Client/Assets/Source Game.Server Game.Server.Tests Game.Shared Game.Shared.Tests; do
    printf '// edit\n' >> "projects/$dir/File.cs"
    run 2 --changed
    grep -Fq "<projects/$dir/>" "$LINT_TEST_LOG"
    run 0 --staged
    git add projects
    run 2 --staged
    reset_case
done
# Unity inputs outside Assets/Source still invalidate the client scope.
printf '{}\n' > projects/Game.Client/Assembly.asmdef
run 2 --changed
reset_case
# A staged server edit ignores unstaged changes to repository-wide settings.
printf '\n' >> global.json
printf '// edit\n' >> projects/Game.Server/File.cs
git add projects/Game.Server
run 2 --staged
reset_case
printf '{}\n' > settings.json
printf 'lint_watch settings.json\n' >> .lint.sh
commit
printf '\n' >> settings.json
run 4 --changed
reset_case
# Missing generated Unity solutions do not prevent a no-op changed run.
rm projects/Game.Client/Game.Client.slnx
git add projects/Game.Client/Game.Client.slnx
git commit -qm 'simulate a host without its generated solution'
run 0 --changed

# A newly added project is discovered from the solution without editing lint configuration.
reset_case
source_dir projects/Game.New
sed '/<\/Solution>/i\
  <Project Path="projects/Game.New/Game.New.csproj" />\
' Game.slnx > New.slnx
mv New.slnx Game.slnx
run 4 --changed
grep -Fq '<projects/Game.New/>' "$LINT_TEST_LOG"

init nested
cat > .lint.sh <<'CONFIG'
lint_solution src/Game.slnx
CONFIG
mkdir -p src
cat > src/Game.slnx <<'SLN'
<Solution>
  <Project Path="../Shared/Shared.csproj" />
</Solution>
SLN
source_dir Shared
commit
printf '// edit\n' >> Shared/File.cs
run 2 --changed
grep -Fq '<Shared/>' "$LINT_TEST_LOG"

init spaces
cat > .lint.sh <<'CONFIG'
lint_solution 'Game solution.slnx'
lint_unity 'projects/Game Client'
CONFIG
cat > 'Game solution.slnx' <<'SLN'
<Solution>
  <Project Path="projects/Game Shared/Game Shared.csproj" />
</Solution>
SLN
source_dir 'projects/Game Shared'
unity_host 'projects/Game Client'
source_dir 'projects/Game Client/Assets/Source'
commit
printf '// edit\n' >> 'projects/Game Shared/File.cs'
run 2 --changed
grep -Fq '<projects/Game Shared/>' "$LINT_TEST_LOG"
run 4
printf 'Diagnostics runner shell tests passed.\n'
