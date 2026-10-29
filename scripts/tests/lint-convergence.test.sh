#!/usr/bin/env sh
# Exercise the shipped --fix command, not only code fixes repeated in an AdhocWorkspace.
set -eu
scripts=$(CDPATH='' cd "$(dirname "$0")/.." && pwd)
root=$(CDPATH='' cd "$scripts/.." && pwd)
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
trap 'exit 1' HUP INT TERM

dotnet build "$root/GamesMayer.Diagnostics/GamesMayer.Diagnostics.csproj" -p:UseSharedCompilation=false --verbosity quiet > "$work/build" 2>&1 || { cat "$work/build" >&2; exit 1; }
cd "$work"
major=$(dotnet --version | cut -d. -f1)
mkdir Cases
cat > Test.csproj <<PROJECT
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net$major.0</TargetFramework>
    <AssemblyName>Sample</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Analyzer Include="$root/GamesMayer.Diagnostics/bin/Debug/netstandard2.0/GamesMayer.Diagnostics.dll" />
  </ItemGroup>
</Project>
PROJECT
printf '<Solution><Project Path="Test.csproj" /></Solution>\n' > Test.slnx
{ printf 'root = true\n'; cat "$root/templates/.editorconfig/.editorconfig.source"; } > .editorconfig
cp "$root/GamesMayer.Diagnostics.Tests/Convergence/Fixtures/CombinedLayout.cs.txt" Cases/CombinedLayout.cs
sh "$scripts/lint-cs.sh" --dir=Cases --sln=Test.slnx --fix > "$work/output" 2>&1 || { cat "$work/output" >&2; exit 1; }
sh "$scripts/lint-cs.sh" --dir=Cases --sln=Test.slnx > "$work/output" 2>&1 || { cat "$work/output" >&2; exit 1; }
cp Cases/CombinedLayout.cs "$work/fixed.cs"
sh "$scripts/lint-cs.sh" --dir=Cases --sln=Test.slnx --fix > "$work/output" 2>&1 || { cat "$work/output" >&2; exit 1; }
cmp Cases/CombinedLayout.cs "$work/fixed.cs"
printf 'Diagnostics combined layout convergence test passed.\n'
