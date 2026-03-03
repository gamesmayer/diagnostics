# GAMESMAYER Analyzers

Custom Roslyn analyzers for C# code style enforcement across GAMESMAYER projects.

## Installation

Add the local NuGet feed to your `nuget.config`:

```xml
<add key="GamesMayer Local" value="../gamesmayer-analyzers/nupkg" />
```

Then reference the package in `Directory.Build.props`:

```xml
<PackageReference Include="GamesMayer.Analyzers" Version="0.1.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>analyzers</IncludeAssets>
</PackageReference>
```

## Rules

All rules default to `Warning` severity and can be suppressed per project or file via `.editorconfig`:

```ini
dotnet_diagnostic.GM1200.severity = none
```

### Custom Rules

| ID | Description |
| -- | ----------- |
| [GM0001](docs/GM0001.md) | No blank lines between consecutive `using` directives |
| [GM0002](docs/GM0002.md) | Auto-implemented property must be on a single line |
| GM0003 | No blank line between an attribute and the member it decorates |
| GM0004 | Member declaration must be on a new line after its attributes |

### Ordering Rules

Ported from [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) (MIT License).

| ID | Description |
| -- | ----------- |
| GM1200 | Using directives must be placed outside namespace declarations |
| GM1201 | Elements must appear in the correct order |
| GM1202 | Elements must be ordered by access level |
| GM1203 | Constant fields must appear before non-constant fields |
| GM1204 | Static elements must appear before instance elements |
| GM1205 | Partial elements must declare an access modifier |
| GM1206 | Declaration keywords must follow order (access → static → other) |
| GM1207 | The keyword `protected` must come before `internal` |
| GM1208 | System using directives must be placed before other using directives |
| GM1209 | Using alias directives must be placed after other using directives |
| GM1210 | Using directives must be ordered alphabetically by namespace |
| GM1211 | Using alias directives must be ordered alphabetically by alias name |
| GM1212 | A get accessor must appear before a set/init accessor |
| GM1213 | An add accessor must appear before a remove accessor |
| GM1214 | Readonly fields must appear before non-readonly fields |
| GM1216 | Using static directives must be placed at the correct location |
| GM1217 | Using static directives must be ordered alphabetically |

## Local Development

```sh
# Pack to local feed (builds implicitly)
dotnet pack GamesMayer.Analyzers/GamesMayer.Analyzers.csproj -c Release -o nupkg

# Clear NuGet cache (required when replacing a package at the same version)
dotnet nuget locals all --clear

# Force-restore consumer solution
dotnet restore /path/to/consumer.sln --force

# Run tests
dotnet test GamesMayer.Analyzers.Tests/GamesMayer.Analyzers.Tests.csproj
```

## Deploy to NuGet.org

1. Bump `<Version>` in [GamesMayer.Analyzers.csproj](GamesMayer.Analyzers/GamesMayer.Analyzers.csproj).
2. Pack the project:
   ```sh
   dotnet pack GamesMayer.Analyzers/GamesMayer.Analyzers.csproj -c Release -o nupkg
   ```
3. Push to NuGet.org — get your API key from [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys):
   ```sh
   dotnet nuget push nupkg/GamesMayer.Analyzers.<version>.nupkg \
     --api-key <your-api-key> \
     --source https://api.nuget.org/v3/index.json
   ```

See the [NuGet publishing docs](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package) for more details.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
