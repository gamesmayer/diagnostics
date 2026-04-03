# GAMESMAYER Diagnostics

<div align="center">
  <img src="./assets/gamesmayer-icon.png" style="max-width: 100%; width: 256px" />
</div>
<p align="center">GAMESMAYER C# code diagnostics.
</p>

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

| ID                                          | Description                                                                        | Fixable | Default |
| ------------------------------------------- | ---------------------------------------------------------------------------------- | ------- | ------- |
| [GM0001](docs/Diagnostics/Layout/GM0001.md) | No blank lines between consecutive `using` directives                              | Yes     | Yes     |
| [GM0002](docs/Diagnostics/Layout/GM0002.md) | Auto-implemented property must be on a single line                                 | Yes     | Yes     |
| [GM0003](docs/Diagnostics/Layout/GM0003.md) | No blank line between an attribute and the member it decorates                     | Yes     | Yes     |
| [GM0004](docs/Diagnostics/Layout/GM0004.md) | Member declaration must be on a new line after its attributes                      | Yes     | Yes     |
| [GM0005](docs/Diagnostics/Layout/GM0005.md) | Attributes must not be separated by commas                                         | Yes     | Yes     |
| [GM0006](docs/Diagnostics/Layout/GM0006.md) | No blank lines between attributes on the same declaration                          | Yes     | Yes     |
| [GM0007](docs/Diagnostics/Layout/GM0007.md) | Class members must be separated by a blank line                                    | Yes     | Yes     |
| [GM0008](docs/Diagnostics/Layout/GM0008.md) | No blank lines within argument or parameter lists                                  | Yes     | Yes     |
| [GM0009](docs/Diagnostics/Layout/GM0009.md) | No line breaks between modifiers, type, and identifier                             | Yes     | Yes     |
| [GM0010](docs/Diagnostics/Layout/GM0010.md) | No two consecutive blank lines                                                     | Yes     | Yes     |
| [GM0011](docs/Diagnostics/Layout/GM0011.md) | No blank line at the beginning of file                                             | Yes     | Yes     |
| [GM0012](docs/Diagnostics/Layout/GM0012.md) | No blank line before opening brace                                                 | Yes     | Yes     |
| [GM0013](docs/Diagnostics/Layout/GM0013.md) | Switch case block must be wrapped in braces                                        | Yes     | Yes     |
| [GM0014](docs/Diagnostics/Layout/GM0014.md) | Namespace identifier must be on a single line                                      | Yes     | Yes     |
| [GM0015](docs/Diagnostics/Layout/GM0015.md) | No blank line between control structure clauses                                    | Yes     | Yes     |
| [GM0016](docs/Diagnostics/Layout/GM0016.md) | Attribute must be on a single line                                                 | Yes     | Yes     |
| [GM0017](docs/Diagnostics/Layout/GM0017.md) | Control structure clause declaration must be on a single line                      | Yes     | Yes     |
| [GM0018](docs/Diagnostics/Layout/GM0018.md) | Switch case clause declaration must be on a single line                            | Yes     | Yes     |
| [GM0019](docs/Diagnostics/Layout/GM0019.md) | Empty braces enclosure must be on a single line                                    | Yes     | Yes     |
| [GM0020](docs/Diagnostics/Layout/GM0020.md) | New line before open brace if braces are not empty                                 | Yes     | Yes     |
| [GM0021](docs/Diagnostics/Layout/GM0021.md) | Empty enclosure opening brace must stay on declaration line                        | Yes     | No      |
| [GM0022](docs/Diagnostics/Layout/GM0022.md) | New line required before opening brace for empty enclosure                         | Yes     | Yes     |
| [GM0023](docs/Diagnostics/Layout/GM0023.md) | Misaligned parameter or argument in multi-line list                                | Yes     | Yes     |
| [GM0025](docs/Diagnostics/Layout/GM0025.md) | Each argument or parameter must be on its own line in a multi-line list            | Yes     | Yes     |
| [GM0026](docs/Diagnostics/Layout/GM0026.md) | Closing parenthesis in multi-line argument/parameter list on own line              | Yes     | Yes     |
| [GM0030](docs/Diagnostics/Layout/GM0030.md) | Prevent blank line between declaration and opening parenthesis                     | Yes     | Yes     |
| [GM0031](docs/Diagnostics/Layout/GM0031.md) | Empty parentheses must be on the same line as the declaration                      | Yes     | Yes     |
| [GM0032](docs/Diagnostics/Layout/GM0032.md) | Single-line parameter or argument list on declaration line                         | Yes     | Yes     |
| [GM0034](docs/Diagnostics/Layout/GM0034.md) | First item in multi-line list must start below opening parenthesis                 | Yes     | Yes     |
| [GM0035](docs/Diagnostics/Layout/GM0035.md) | No blank lines between fluent-chain segments                                       | Yes     | Yes     |
| [GM0036](docs/Diagnostics/Layout/GM0036.md) | Variable declaration header must be on a single line                               | Yes     | Yes     |
| [GM0037](docs/Diagnostics/Layout/GM0037.md) | Parameter declaration header must be on a single line                              | Yes     | Yes     |
| [GM0039](docs/Diagnostics/Layout/GM0039.md) | Dot must be on the same line as the next identifier in fluent chains               | Yes     | Yes     |
| [GM0040](docs/Diagnostics/Layout/GM0040.md) | All fluent-chain segments must be on their own line if any segment is              | Yes     | Yes     |
| [GM0041](docs/Diagnostics/Layout/GM0041.md) | Each segment in a multi-invocation fluent chain must be on its own line            | Yes     | Yes     |
| [GM0042](docs/Diagnostics/Layout/GM0042.md) | Blank line required after last using directive                                     | Yes     | Yes     |
| [GM0043](docs/Diagnostics/Layout/GM0043.md) | Each operand in a multi-operand logical expression must be on its own line         | Yes     | Yes     |
| [GM0044](docs/Diagnostics/Layout/GM0044.md) | No blank line between lambda arrow and body                                        | Yes     | Yes     |
| [GM0046](docs/Diagnostics/Layout/GM0046.md) | Binary operator in multi-line expression must be at the end of the previous line   | Yes     | Yes     |
| [GM0047](docs/Diagnostics/Layout/GM0047.md) | Multi-line assignment value must start on the same line as the assignment operator | Yes     | Yes     |
| [GM0048](docs/Diagnostics/Layout/GM0048.md) | Expression body must start on the same line as '=>'                                | Yes     | Yes     |
| [GM0050](docs/Diagnostics/Layout/GM0050.md) | No blank lines between multi-line operator-expression items                        | Yes     | Yes     |
| [GM0051](docs/Diagnostics/Layout/GM0051.md) | Return expression must start on the same line as 'return'                          | Yes     | Yes     |
| [GM0052](docs/Diagnostics/Layout/GM0052.md) | No blank lines between 'return' and returned expression                            | Yes     | Yes     |
| [GM0053](docs/Diagnostics/Layout/GM0053.md) | Each 'where' constraint clause must be on its own line                             | Yes     | Yes     |
| [GM0055](docs/Diagnostics/Layout/GM0055.md) | No blank lines between 'where' constraint clauses                                  | Yes     | Yes     |
| [GM0056](docs/Diagnostics/Layout/GM0056.md) | 'where' constraint clause must be written on a single line                         | Yes     | Yes     |
| [GM0057](docs/Diagnostics/Layout/GM0057.md) | No blank lines between declaration and first 'where' constraint clause             | Yes     | Yes     |
| [GM0058](docs/Diagnostics/Layout/GM0058.md) | Inheritance list must be on the same line as the type declaration                  | Yes     | Yes     |
| [GM0059](docs/Diagnostics/Layout/GM0059.md) | Colon in named argument must be adjacent to parameter name                         | Yes     | Yes     |
| [GM0060](docs/Diagnostics/Layout/GM0060.md) | Property/field access chain must be on a single line                               | Yes     | Yes     |
| [GM0061](docs/Diagnostics/Layout/GM0061.md) | Use '==' or '!=' instead of 'is' pattern for equality comparison                   | Yes     | Yes     |
| [GM0062](docs/Diagnostics/Layout/GM0062.md) | Use 'is or' syntax for multiple type checks on the same expression                 | Yes     | Yes     |
| [GM0063](docs/Diagnostics/Layout/GM0063.md) | Methods and lambda expressions with braces cannot be single line statements        | Yes     | Yes     |
| [GM0064](docs/Diagnostics/Layout/GM0064.md) | Early return in function                                                           | No      | Yes     |
| [GM0065](docs/Diagnostics/Layout/GM0065.md) | Return statement must be preceded by a blank line                                  | Yes     | Yes     |
| [GM0066](docs/Diagnostics/Layout/GM0066.md) | Block statement must be preceded by blank line                                     | Yes     | Yes     |
| [GM0067](docs/Diagnostics/Layout/GM0067.md) | Block statement must be followed by a blank line                                   | Yes     | Yes     |
| [GM0068](docs/Diagnostics/Layout/GM0068.md) | Multiple statements on one line                                                    | Yes     | Yes     |
| [GM0069](docs/Diagnostics/Layout/GM0069.md) | Properties not autoimplemented cannot be single line statements                    | Yes     | Yes     |
| [GM0070](docs/Diagnostics/Layout/GM0070.md) | Each accessor must be on its own line in non-autoimplemented properties            | Yes     | Yes     |
| [GM0071](docs/Diagnostics/Layout/GM0071.md) | Each accessor in non-autoimplemented properties must be separated by a blank line  | Yes     | Yes     |
| [GM0072](docs/Diagnostics/Layout/GM0072.md) | Blank line required before `#if` directive                                         | Yes     | Yes     |
| [GM0073](docs/Diagnostics/Layout/GM0073.md) | Blank line required after `#endif` directive                                       | Yes     | Yes     |
| [GM0074](docs/Diagnostics/Layout/GM0074.md) | `#if` / `#else` / `#elif` directive must not be followed by blank line             | Yes     | Yes     |
| [GM0075](docs/Diagnostics/Layout/GM0075.md) | `#endif` / `#else` / `#elif` directive must not be preceded by blank line          | Yes     | Yes     |
| [GM0080](docs/Diagnostics/Layout/GM0080.md) | Break statement must be preceded by a blank line                                   | Yes     | Yes     |
| [GM0112](docs/Diagnostics/Layout/GM0112.md) | No blank lines between consecutive variables                                       | Yes     | Yes     |
| [GM0117](docs/Diagnostics/Layout/GM0117.md) | Each item in array initializer must be on its own line                             | Yes     | Yes     |
| [GM0119](docs/Diagnostics/Layout/GM0119.md) | No blank lines between array initializer items                                     | Yes     | Yes     |
| [GM0118](docs/Diagnostics/Layout/GM0118.md) | Comma must be on the same line as the previous token                               | Yes     | Yes     |
| [GM1505](docs/Diagnostics/Layout/GM1505.md) | Opening brace must not be followed by blank line                                   | Yes     | Yes     |
| [GM1508](docs/Diagnostics/Layout/GM1508.md) | Closing brace must not be preceded by blank line                                   | Yes     | Yes     |

### Indentation Diagnostics

| ID                                               | Description                                                                           | Fixable | Default |
| ------------------------------------------------ | ------------------------------------------------------------------------------------- | ------- | ------- |
| [GM0024](docs/Diagnostics/Indentation/GM0024.md) | Anonymous function incorrectly indented                                               | Yes     | Yes     |
| [GM0027](docs/Diagnostics/Indentation/GM0027.md) | Opening parenthesis placement in multi-line argument/parameter list                   | Yes     | Yes     |
| [GM0028](docs/Diagnostics/Indentation/GM0028.md) | Closing parenthesis indentation in multi-line argument/parameter list                 | Yes     | Yes     |
| [GM0029](docs/Diagnostics/Indentation/GM0029.md) | Opening parenthesis must be on a new line in multi-line list                          | Yes     | Yes     |
| [GM0033](docs/Diagnostics/Indentation/GM0033.md) | Opening parenthesis on separate line must match declaration indentation               | Yes     | Yes     |
| [GM0038](docs/Diagnostics/Indentation/GM0038.md) | Fluent-chain segment indentation                                                      | Yes     | Yes     |
| [GM0045](docs/Diagnostics/Indentation/GM0045.md) | Lambda expression body must be indented one step from the arrow line                  | Yes     | Yes     |
| [GM0049](docs/Diagnostics/Indentation/GM0049.md) | Wrapped operator-expression item must be indented one step from expression start line | Yes     | Yes     |
| [GM0054](docs/Diagnostics/Indentation/GM0054.md) | 'where' constraint clause must be indented one step from the declaration              | Yes     | Yes     |
| [GM0077](docs/Diagnostics/Indentation/GM0077.md) | Block contents must be indented one step from the block braces                        | Yes     | Yes     |
| [GM0078](docs/Diagnostics/Indentation/GM0078.md) | Block brace must be indented at the declaration level                                 | Yes     | Yes     |
| [GM0081](docs/Diagnostics/Indentation/GM0081.md) | Case labels must be indented one step right from the switch keyword                   | Yes     | Yes     |
| [GM0115](docs/Diagnostics/Indentation/GM0115.md) | Array initializer braces must be indented at the declaration level                    | Yes     | Yes     |
| [GM0116](docs/Diagnostics/Indentation/GM0116.md) | Array initializer items must be indented one step from the declaration                | Yes     | Yes     |

### Spacing Diagnostics

| ID                                           | Description                                                 | Fixable | Default |
| -------------------------------------------- | ----------------------------------------------------------- | ------- | ------- |
| [GM0079](docs/Diagnostics/Spacing/GM0079.md) | No more than one space between tokens                       | Yes     | Yes     |
| [GM0082](docs/Diagnostics/Spacing/GM0082.md) | Cast expression spacing                                     | Yes     | Yes     |
| [GM0083](docs/Diagnostics/Spacing/GM0083.md) | Space after control-flow keywords                           | Yes     | Yes     |
| [GM0084](docs/Diagnostics/Spacing/GM0084.md) | Spaces inside method declaration parameter list parentheses | Yes     | Yes     |
| [GM0085](docs/Diagnostics/Spacing/GM0085.md) | Spaces inside method call argument list parentheses         | Yes     | Yes     |
| [GM0086](docs/Diagnostics/Spacing/GM0086.md) | Space before colon in inheritance clause                    | Yes     | Yes     |
| [GM0087](docs/Diagnostics/Spacing/GM0087.md) | Space after colon in inheritance clause                     | Yes     | Yes     |
| [GM0088](docs/Diagnostics/Spacing/GM0088.md) | Space around binary operators                               | Yes     | Yes     |
| [GM0089](docs/Diagnostics/Spacing/GM0089.md) | Space between parentheses                                   | Yes     | No      |
| [GM0090](docs/Diagnostics/Spacing/GM0090.md) | Space in empty method declaration parameter list            | Yes     | No      |
| [GM0091](docs/Diagnostics/Spacing/GM0091.md) | Space between method declaration name and open parenthesis  | Yes     | No      |
| [GM0092](docs/Diagnostics/Spacing/GM0092.md) | Space in empty method call argument list                    | Yes     | No      |
| [GM0093](docs/Diagnostics/Spacing/GM0093.md) | Space between method call name and opening parenthesis      | Yes     | No      |
| [GM0094](docs/Diagnostics/Spacing/GM0094.md) | Space after comma                                           | Yes     | Yes     |
| [GM0095](docs/Diagnostics/Spacing/GM0095.md) | Space before comma                                          | Yes     | No      |
| [GM0096](docs/Diagnostics/Spacing/GM0096.md) | Space after dot                                             | Yes     | No      |
| [GM0097](docs/Diagnostics/Spacing/GM0097.md) | Space before dot                                            | Yes     | No      |
| [GM0099](docs/Diagnostics/Spacing/GM0099.md) | Space after semicolon in for statement                      | Yes     | Yes     |
| [GM0100](docs/Diagnostics/Spacing/GM0100.md) | Space before semicolon in for statement                     | Yes     | No      |
| [GM0098](docs/Diagnostics/Spacing/GM0098.md) | Space before open square bracket                            | Yes     | No      |
| [GM0101](docs/Diagnostics/Spacing/GM0101.md) | Space between empty square brackets                         | Yes     | No      |
| [GM0102](docs/Diagnostics/Spacing/GM0102.md) | Space between square brackets                               | Yes     | No      |
| [GM0103](docs/Diagnostics/Spacing/GM0103.md) | Space before generic '<' symbol                             | Yes     | No      |
| [GM0104](docs/Diagnostics/Spacing/GM0104.md) | Space between generic operators '<' and '>'                 | Yes     | No      |
| [GM0105](docs/Diagnostics/Spacing/GM0105.md) | Space around ternary operators '?' and ':'                  | Yes     | Yes     |
| [GM0106](docs/Diagnostics/Spacing/GM0106.md) | Space before => symbol                                      | Yes     | No      |
| [GM0107](docs/Diagnostics/Spacing/GM0107.md) | Space after => symbol                                       | Yes     | No      |
| [GM0108](docs/Diagnostics/Spacing/GM0108.md) | Space after type in declarations                            | Yes     | Yes     |
| [GM0109](docs/Diagnostics/Spacing/GM0109.md) | Space before assignment operator '='                        | Yes     | Yes     |
| [GM0110](docs/Diagnostics/Spacing/GM0110.md) | Space after assignment operator '='                         | Yes     | Yes     |
| [GM0111](docs/Diagnostics/Spacing/GM0111.md) | Statement closing semicolon must be adjacent to last token  | Yes     | Yes     |
| [GM0113](docs/Diagnostics/Spacing/GM0113.md) | Space between single-line block braces                      | Yes     | Yes     |
| [GM0114](docs/Diagnostics/Spacing/GM0114.md) | Space between accessors in single-line property             | Yes     | Yes     |

### Architecture Diagnostics

| ID                                                | Description                                  | Fixable | Default |
| ------------------------------------------------- | -------------------------------------------- | ------- | ------- |
| [GM0076](docs/Diagnostics/Architecture/GM0076.md) | Avoid using Unity Debug.Log methods directly | No      | Yes     |

### Naming Diagnostics

Ported from [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).

| ID                                          | Description                                  | Fixable | Default |
| ------------------------------------------- | -------------------------------------------- | ------- | ------- |
| [GM1300](docs/Diagnostics/Naming/GM1300.md) | Element must begin with an upper-case letter | No      | Yes     |

### Ordering Diagnostics

Ported from [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).

| ID                                            | Description                                                          | Fixable | Default |
| --------------------------------------------- | -------------------------------------------------------------------- | ------- | ------- |
| [GM1200](docs/Diagnostics/Ordering/GM1200.md) | Using directives must be placed outside namespace declarations       | No      | Yes     |
| [GM1201](docs/Diagnostics/Ordering/GM1201.md) | Elements must appear in the correct order                            | No      | Yes     |
| [GM1202](docs/Diagnostics/Ordering/GM1202.md) | Elements must be ordered by access level                             | No      | Yes     |
| [GM1203](docs/Diagnostics/Ordering/GM1203.md) | Constant fields must appear before non-constant fields               | No      | Yes     |
| [GM1204](docs/Diagnostics/Ordering/GM1204.md) | Static elements must appear before instance elements                 | No      | Yes     |
| [GM1205](docs/Diagnostics/Ordering/GM1205.md) | Partial elements must declare an access modifier                     | No      | Yes     |
| [GM1206](docs/Diagnostics/Ordering/GM1206.md) | Declaration keywords must follow order (access → static → other)     | No      | Yes     |
| [GM1207](docs/Diagnostics/Ordering/GM1207.md) | The keyword `protected` must come before `internal`                  | No      | Yes     |
| [GM1208](docs/Diagnostics/Ordering/GM1208.md) | System using directives must be placed before other using directives | No      | Yes     |
| [GM1209](docs/Diagnostics/Ordering/GM1209.md) | Using alias directives must be placed after other using directives   | No      | Yes     |
| [GM1210](docs/Diagnostics/Ordering/GM1210.md) | Using directives must be ordered alphabetically by namespace         | No      | Yes     |
| [GM1211](docs/Diagnostics/Ordering/GM1211.md) | Using alias directives must be ordered alphabetically by alias name  | No      | Yes     |
| [GM1212](docs/Diagnostics/Ordering/GM1212.md) | A get accessor must appear before a set/init accessor                | No      | Yes     |
| [GM1213](docs/Diagnostics/Ordering/GM1213.md) | An add accessor must appear before a remove accessor                 | No      | Yes     |
| [GM1214](docs/Diagnostics/Ordering/GM1214.md) | Readonly fields must appear before non-readonly fields               | No      | Yes     |
| [GM1215](docs/Diagnostics/Ordering/GM1215.md) | Using static directives must be ordered alphabetically               | No      | Yes     |
| [GM1216](docs/Diagnostics/Ordering/GM1216.md) | Using static directives must be placed at the correct location       | No      | Yes     |

## Usage

### Using with `dotnet format`

`dotnet format --fix-analyzers` does **not** respect an analyzer's built-in default severity. Even if a diagnostic defaults to `Warning` inside the package, `dotnet format` will ignore it unless the severity is explicitly declared in `.editorconfig`.

To enable all GamesMayer diagnostics for `dotnet format`, add explicit severity entries to your `.editorconfig`:

```ini
[*.cs]
dotnet_analyzer_diagnostic.severity = default

dotnet_diagnostic.GM0001.severity = warning
dotnet_diagnostic.GM0002.severity = warning
# ... and so on for each rule
```

A ready-to-use example covering all diagnostics is available at [`templates/.editorconfig/.editorconfig.source`](templates/.editorconfig/.editorconfig.source). Rename it to `.editorconfig` and place it in the root of your source code.

> **Note:** Architecture, Ordering, and Naming diagnostics (GM0076, GM1200–GM1216, GM1300) do not have code fix providers, so `dotnet format` will report them as violations but cannot auto-fix them.

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
