# GAMESMAYER Analyzers

Custom Roslyn diagnostics for C# code style enforcement across GAMESMAYER projects.

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

## Diagnostics

All diagnostics default to `Warning` severity and can be suppressed per project or file via `.editorconfig`:

```ini
dotnet_diagnostic.GM0001.severity = none
```

### Layout Diagnostics

| ID                              | Description                                                    | Fixable |
| ------------------------------- | -------------------------------------------------------------- | ------- |
| [GM0001](docs/Layout/GM0001.md) | No blank lines between consecutive `using` directives          | Yes     |
| [GM0002](docs/Layout/GM0002.md) | Auto-implemented property must be on a single line             | Yes     |
| [GM0003](docs/Layout/GM0003.md) | No blank line between an attribute and the member it decorates | Yes     |
| [GM0004](docs/Layout/GM0004.md) | Member declaration must be on a new line after its attributes  | Yes     |

### Ordering Diagnostics

Ported from [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).

| ID                                | Description                                                          | Fixable |
| --------------------------------- | -------------------------------------------------------------------- | ------- |
| [GM1200](docs/Ordering/GM1200.md) | Using directives must be placed outside namespace declarations       | No      |
| [GM1201](docs/Ordering/GM1201.md) | Elements must appear in the correct order                            | No      |
| [GM1202](docs/Ordering/GM1202.md) | Elements must be ordered by access level                             | No      |
| [GM1203](docs/Ordering/GM1203.md) | Constant fields must appear before non-constant fields               | No      |
| [GM1204](docs/Ordering/GM1204.md) | Static elements must appear before instance elements                 | No      |
| [GM1205](docs/Ordering/GM1205.md) | Partial elements must declare an access modifier                     | No      |
| [GM1206](docs/Ordering/GM1206.md) | Declaration keywords must follow order (access → static → other)     | No      |
| [GM1207](docs/Ordering/GM1207.md) | The keyword `protected` must come before `internal`                  | No      |
| [GM1208](docs/Ordering/GM1208.md) | System using directives must be placed before other using directives | No      |
| [GM1209](docs/Ordering/GM1209.md) | Using alias directives must be placed after other using directives   | No      |
| [GM1210](docs/Ordering/GM1210.md) | Using directives must be ordered alphabetically by namespace         | No      |
| [GM1211](docs/Ordering/GM1211.md) | Using alias directives must be ordered alphabetically by alias name  | No      |
| [GM1212](docs/Ordering/GM1212.md) | A get accessor must appear before a set/init accessor                | No      |
| [GM1213](docs/Ordering/GM1213.md) | An add accessor must appear before a remove accessor                 | No      |
| [GM1214](docs/Ordering/GM1214.md) | Readonly fields must appear before non-readonly fields               | No      |
| [GM1216](docs/Ordering/GM1216.md) | Using static directives must be placed at the correct location       | No      |
| [GM1217](docs/Ordering/GM1217.md) | Using static directives must be ordered alphabetically               | No      |

## Local Development

```sh
# Pack to local feed (builds implicitly)
dotnet pack GamesMayer.Analyzers/GamesMayer.Analyzers.csproj -c Release -o nupkg

# Clear NuGet cache (required when replacing a package at the same version)
dotnet nuget locals all --clear

# Force-restore consumer solution
dotnet restore /path/to/consumer.sln --force
```

## Testing

```sh
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
