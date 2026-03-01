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

## Publishing

```sh
dotnet pack GamesMayer.Analyzers/GamesMayer.Analyzers.csproj -c Release -o nupkg
```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
