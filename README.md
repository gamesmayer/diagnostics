# GAMESMAYER Diagnostics

GAMESMAYER C# code diagnostics.

## Installation

Add the local NuGet feed to your `nuget.config`:

```xml
<add key="GamesMayer Local" value="../gamesmayer-diagnostics/nupkg" />
```

Then reference the package in `Directory.Build.props`:

```xml
<PackageReference Include="GamesMayer.Diagnostics" Version="0.1.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>analyzers</IncludeAssets>
</PackageReference>
```

## Diagnostics

All diagnostics default to `Warning` severity and can be suppressed per project or file via `.editorconfig`:

```ini
dotnet_diagnostic.GM0001.severity = none
```

Diagnostics are organized by category.

### Layout Diagnostics

| ID                                          | Description                                                    | Fixable |
| ------------------------------------------- | -------------------------------------------------------------- | ------- |
| [GM0001](docs/Diagnostics/Layout/GM0001.md) | No blank lines between consecutive `using` directives          | Yes     |
| [GM0002](docs/Diagnostics/Layout/GM0002.md) | Auto-implemented property must be on a single line             | Yes     |
| [GM0003](docs/Diagnostics/Layout/GM0003.md) | No blank line between an attribute and the member it decorates | Yes     |
| [GM0004](docs/Diagnostics/Layout/GM0004.md) | Member declaration must be on a new line after its attributes  | Yes     |
| [GM0005](docs/Diagnostics/Layout/GM0005.md) | Attributes must not be separated by commas                     | Yes     |
| [GM0006](docs/Diagnostics/Layout/GM0006.md) | No blank lines between attributes on the same declaration      | Yes     |
| [GM0007](docs/Diagnostics/Layout/GM0007.md) | Class members must be separated by a blank line                | Yes     |
| [GM0008](docs/Diagnostics/Layout/GM0008.md) | No blank lines within argument or parameter lists              | Yes     |
| [GM0009](docs/Diagnostics/Layout/GM0009.md) | No line breaks between modifiers, type, and identifier         | Yes     |
| [GM0010](docs/Diagnostics/Layout/GM0010.md) | No two consecutive blank lines                                 | Yes     |
| [GM0011](docs/Diagnostics/Layout/GM0011.md) | No blank line at the beginning of file                         | Yes     |
| [GM0012](docs/Diagnostics/Layout/GM0012.md) | No blank line before opening brace                             | Yes     |
| [GM0013](docs/Diagnostics/Layout/GM0013.md) | Switch case block must be wrapped in braces                    | Yes     |

### Ordering Diagnostics

Ported from [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).

| ID                                            | Description                                                          | Fixable |
| --------------------------------------------- | -------------------------------------------------------------------- | ------- |
| [GM1200](docs/Diagnostics/Ordering/GM1200.md) | Using directives must be placed outside namespace declarations       | No      |
| [GM1201](docs/Diagnostics/Ordering/GM1201.md) | Elements must appear in the correct order                            | No      |
| [GM1202](docs/Diagnostics/Ordering/GM1202.md) | Elements must be ordered by access level                             | No      |
| [GM1203](docs/Diagnostics/Ordering/GM1203.md) | Constant fields must appear before non-constant fields               | No      |
| [GM1204](docs/Diagnostics/Ordering/GM1204.md) | Static elements must appear before instance elements                 | No      |
| [GM1205](docs/Diagnostics/Ordering/GM1205.md) | Partial elements must declare an access modifier                     | No      |
| [GM1206](docs/Diagnostics/Ordering/GM1206.md) | Declaration keywords must follow order (access → static → other)     | No      |
| [GM1207](docs/Diagnostics/Ordering/GM1207.md) | The keyword `protected` must come before `internal`                  | No      |
| [GM1208](docs/Diagnostics/Ordering/GM1208.md) | System using directives must be placed before other using directives | No      |
| [GM1209](docs/Diagnostics/Ordering/GM1209.md) | Using alias directives must be placed after other using directives   | No      |
| [GM1210](docs/Diagnostics/Ordering/GM1210.md) | Using directives must be ordered alphabetically by namespace         | No      |
| [GM1211](docs/Diagnostics/Ordering/GM1211.md) | Using alias directives must be ordered alphabetically by alias name  | No      |
| [GM1212](docs/Diagnostics/Ordering/GM1212.md) | A get accessor must appear before a set/init accessor                | No      |
| [GM1213](docs/Diagnostics/Ordering/GM1213.md) | An add accessor must appear before a remove accessor                 | No      |
| [GM1214](docs/Diagnostics/Ordering/GM1214.md) | Readonly fields must appear before non-readonly fields               | No      |
| [GM1215](docs/Diagnostics/Ordering/GM1215.md) | Using static directives must be ordered alphabetically               | No      |
| [GM1216](docs/Diagnostics/Ordering/GM1216.md) | Using static directives must be placed at the correct location       | No      |

## Local Development

```sh
# Pack to local feed (builds implicitly)
dotnet pack GamesMayer.Diagnostics/GamesMayer.Diagnostics.csproj -c Release -o nupkg

# Clear NuGet cache (required when replacing a package at the same version)
dotnet nuget locals all --clear

# Force-restore consumer solution
dotnet restore /path/to/consumer.sln --force
```

## Testing

```sh
dotnet test GamesMayer.Diagnostics.Tests/GamesMayer.Diagnostics.Tests.csproj
```

## Deploy to NuGet.org

1. Bump `<Version>` in [GamesMayer.Diagnostics.csproj](GamesMayer.Diagnostics/GamesMayer.Diagnostics.csproj).
2. Pack the project:
   ```sh
   dotnet pack GamesMayer.Diagnostics/GamesMayer.Diagnostics.csproj -c Release -o nupkg
   ```
3. Push to NuGet.org — get your API key from [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys):
   ```sh
   dotnet nuget push nupkg/GamesMayer.Diagnostics.<version>.nupkg \
     --api-key <your-api-key> \
     --source https://api.nuget.org/v3/index.json
   ```

See the [NuGet publishing docs](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package) for more details.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
