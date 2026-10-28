#!/usr/bin/env sh
set -eu

scripts=$(CDPATH='' cd "$(dirname "$0")/.." && pwd)
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
trap 'exit 1' HUP INT TERM
mkdir -p "$work/bin" "$work/repo"
export LINT_TEST_LOG="$work/calls"
cat > "$work/bin/dotnet" <<'STUB'
#!/usr/bin/env sh
printf '<%s> ' "$@" >> "$LINT_TEST_LOG"
printf '\n' >> "$LINT_TEST_LOG"
if [ "${2:-}" = style ] && [ -n "${LINT_TEST_FIX_FILE:-}" ]; then
    printf '// fixed\n' >> "$LINT_TEST_FIX_FILE"
fi
exit "${LINT_TEST_EXIT:-0}"
STUB
chmod +x "$work/bin/dotnet"
export PATH="$work/bin:$PATH"
cd "$work/repo"
git init -q
git config user.email lint@example.test
git config user.name 'Lint test'
mkdir -p 'Client/Assets/Source' Server 'Other folder'
printf 'class File {}\n' > 'Client/Assets/Source/File with spaces.cs'
printf 'class Server {}\n' > Server/File.cs
printf 'class Other {}\n' > 'Other folder/File.cs'
printf '<Solution />\n' > game.slnx
printf '<Solution />\n' > Client/client.slnx
printf '{}\n' > global.json
printf '{}\n' > settings.json
printf 'root = true\n' > .editorconfig
printf '<Project />\n' > Client/Directory.Build.props
printf '<Project />\n' > 'Client/Client.csproj'
printf 'readme\n' > README.md
git add .
git commit -qm baseline

reset_case() {
    git reset --hard -q HEAD
    git clean -fdq
}

expect() {
    expected=$1
    shift
    : > "$LINT_TEST_LOG"
    sh "$scripts/lint-cs.sh" --dir=Client/Assets/Source --sln=Client/client.slnx "$@" > "$work/output" 2>&1 || {
        cat "$work/output" >&2; exit 1;
    }
    count=$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')
    if [ "$count" != "$expected" ]; then
        echo "Expected $expected dotnet calls, got $count: $*" >&2
        cat "$work/output" >&2
        exit 1
    fi
}

expect_failure() {
    if sh "$scripts/lint-cs.sh" "$@" > "$work/output" 2>&1; then
        echo "Expected failure: $*" >&2; exit 1
    fi
}

expect 2
expect 0 --changed
expect 0 --staged
# No solution discovery or validation when there is nothing to lint.
sh "$scripts/lint-cs.sh" --dir=Client/Assets/Source --changed > /dev/null
printf 'documentation\n' >> README.md
expect 0 --changed
printf '// edit\n' >> 'Client/Assets/Source/File with spaces.cs'
expect 2 --changed
expect 0 --staged
git add Client
expect 2 --staged
export LINT_TEST_FIX_FILE='Client/Assets/Source/File with spaces.cs'
expect 2 --changed --fix
grep -Fq 'Fixed 1 file(s)' "$work/output"
grep -Fq "$LINT_TEST_FIX_FILE" "$work/output"
unset LINT_TEST_FIX_FILE
grep -q -- '--verify-no-changes' "$LINT_TEST_LOG" && exit 1
reset_case

printf 'class New {}\n' > 'Client/Assets/Source/New file.cs'
expect 2 --changed
expect 0 --staged
reset_case
rm 'Client/Assets/Source/File with spaces.cs'
expect 2 --changed
git add -u
expect 2 --staged
reset_case
# Both sides of moves are selected, including moves out of the lint directory.
git mv 'Client/Assets/Source/File with spaces.cs' 'Other folder/Moved.cs'
expect 2 --staged
: > "$LINT_TEST_LOG"
sh "$scripts/lint-cs.sh" '--dir=Other folder' --sln=game.slnx --staged > /dev/null
[ "$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')" = 2 ]
reset_case

printf '// edit\n' >> Server/File.cs
expect 0 --changed
expect 2 --changed --watch=Server
reset_case
printf '{}\n' >> settings.json
expect 0 --changed
expect 2 --changed --watch=settings.json --watch=another.json
reset_case
for config in .editorconfig global.json Client/Directory.Build.props Client/client.slnx; do
    printf '\n' >> "$config"
    expect 2 --changed
    reset_case
done
printf '\n' >> Client/Client.csproj
expect 2 --changed --watch=Client/Client.csproj
reset_case
for name in File.csproj File.props File.targets File.asmdef File.asmref File.slnx .editorconfig; do
    printf '\n' > "Client/Assets/Source/$name"
    expect 2 --changed
    reset_case
done
# Staged selection ignores unrelated working-tree settings changes.
printf '\n' >> global.json
printf '\n' >> Server/File.cs
git add Server
expect 0 --staged
reset_case

printf '// staged\n' >> 'Client/Assets/Source/File with spaces.cs'
git add Client
printf '// unstaged\n' >> 'Client/Assets/Source/File with spaces.cs'
expect 2 --staged
expect 2 --changed
grep -q '<Client/Assets/Source/>' "$LINT_TEST_LOG"
export LINT_TEST_EXIT=1
expect_failure --dir=Client/Assets/Source --sln=Client/client.slnx --changed
unset LINT_TEST_EXIT
expect_failure --changed --staged
expect_failure --unknown
# Multiple directories share one pair of format calls and only changed directories are selected.
reset_case
printf '// changed\n' >> Server/File.cs
: > "$LINT_TEST_LOG"
sh "$scripts/lint-cs.sh" --dir=Client/Assets/Source --dir=Server --sln=game.slnx --changed > /dev/null
[ "$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')" = 2 ]
grep -Fq '<Server/>' "$LINT_TEST_LOG"
if grep -Fq '<Client/Assets/Source/>' "$LINT_TEST_LOG"; then exit 1; fi
# Subdirectory invocation still recognizes root settings and relative directory paths.
reset_case
printf '\n' >> global.json
cd Client
: > "$LINT_TEST_LOG"
sh "$scripts/lint-cs.sh" --dir=Assets/Source --sln=client.slnx --changed > /dev/null
[ "$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')" = 2 ]
cd ..
reset_case
printf 'is_global = true\n' > .globalconfig
cd Client
: > "$LINT_TEST_LOG"
sh "$scripts/lint-cs.sh" --dir=Assets/Source --sln=client.slnx --changed > /dev/null
[ "$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')" = 2 ]
cd ..
reset_case
printf '\n' >> game.slnx
: > "$LINT_TEST_LOG"
sh "$scripts/lint-cs.sh" --dir=Client/Assets/Source --changed > /dev/null
[ "$(wc -l < "$LINT_TEST_LOG" | tr -d ' ')" = 2 ]
cd "$work"
expect_failure --changed --dir=repo/Client/Assets/Source
mkdir unborn
cd unborn
git init -q
expect_failure --changed
printf 'Diagnostics lint shell tests passed.\n'
