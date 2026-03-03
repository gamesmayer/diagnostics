# GAMESMAYER Analyzers

Custom Roslyn analyzers for C# code style enforcement across GAMESMAYER projects.

## Installation

Add the local NuGet feed to your `nuget.config`:

```xml
<add key="GamesMayer Local" value="../gamesmayer-analyzers/nupkg" />
```

Then reference the package in `Directory.Build.props`:

```xml
<PackageReference Include="GamesMayer.Analyzers" Version="1.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>analyzers</IncludeAssets>
</PackageReference>
```

## Rules

| ID                       | Category | Severity | Description                                           |
| ------------------------ | -------- | -------- | ----------------------------------------------------- |
| [GM0001](docs/GM0001.md) | Style    | Warning  | No blank lines between consecutive `using` directives |
| [GM0002](docs/GM0002.md) | Style    | Warning  | Auto-implemented property must be on a single line    |

## Local Development

```sh
# Pack to local feed (builds implicitly)
dotnet pack GamesMayer.Analyzers/GamesMayer.Analyzers.csproj -c Release -o nupkg

# Clear NuGet cache (required when replacing a package at the same version)
dotnet nuget locals all --clear

# Force-restore consumer solution
dotnet restore /path/to/consumer.sln --force
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
