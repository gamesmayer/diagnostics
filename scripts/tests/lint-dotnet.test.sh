#!/usr/bin/env sh
# A real formatter check catches false passes caused by invalid --include arguments.
set -eu
scripts=$(CDPATH='' cd "$(dirname "$0")/.." && pwd)
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
trap 'exit 1' HUP INT TERM
cd "$work"
major=$(dotnet --version | cut -d. -f1)
mkdir -p 'Project with spaces'
printf '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net%s.0</TargetFramework><EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild></PropertyGroup></Project>\n' "$major" > 'Project with spaces/Test.csproj'
cat > Test.slnx <<'SLN'
<Solution>
  <Project Path="Project with spaces/Test.csproj" />
</Solution>
SLN
cat > .editorconfig <<'CONFIG'
root = true
[*.cs]
csharp_style_var_for_built_in_types = true:warning
csharp_style_var_when_type_is_apparent = true:warning
csharp_style_var_elsewhere = true:warning
dotnet_diagnostic.IDE0007.severity = warning
CONFIG
cat > 'Project with spaces/File.cs' <<'CS'
public static class Example
{
    public static void Run()
    {
        int value = 1;
        System.Console.WriteLine(value);
    }
}
CS
if sh "$scripts/lint-cs.sh" '--dir=Project with spaces' --sln=Test.slnx > "$work/output" 2>&1; then
    echo 'Expected the real formatter to detect the IDE0007 violation.' >&2
    cat "$work/output" >&2
    exit 1
fi
grep -q IDE0007 "$work/output"
sh "$scripts/lint-cs.sh" '--dir=Project with spaces' --sln=Test.slnx --fix > "$work/output" 2>&1 || { cat "$work/output" >&2; exit 1; }
grep -q 'var value = 1;' 'Project with spaces/File.cs'
sh "$scripts/lint-cs.sh" '--dir=Project with spaces' --sln=Test.slnx > "$work/output" 2>&1 || { cat "$work/output" >&2; exit 1; }
printf 'Diagnostics real dotnet formatter test passed.\n'
