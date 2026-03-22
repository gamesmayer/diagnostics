# Create New Diagnostic

## Requirements

If one of the non-optional requirements is not provided, it will not be possible to execute the process.

|     Requirement     |                              Description                              | Optional |                  Default Value                   |                                  Example                                   |
| :-----------------: | :-------------------------------------------------------------------: | :------: | :----------------------------------------------: | :------------------------------------------------------------------------: |
|        rule         |                       Analyzer diagnostic rule                        |  false   |                        -                         |             "Don't allow blank lines between using directives"             |
|         id          |                         Unique Diagnostic ID                          |   true   | The lowest available code. Starting with GM0001. |                                   GM0001                                   |
|        title        |                     Short title describing issue                      |   true   |                 Infer from rule                  |                   "Blank line between using directives"                    |
|    messageFormat    |                    Short message to fix the issue                     |   true   |                 Infer from rule                  |        "Remove the blank line between 'using {0}' and 'using {1}'"         |
|      category       |                    The category of the diagnostic                     |   true   |                     "Layout"                     |                                  "Layout"                                  |
|   defaultSeverity   |                  Default severity of the diagnostic                   |   true   |            DiagnosticSeverity.Warning            |                         DiagnosticSeverity.Warning                         |
| isEnabledByDefault  |                If the diagnostic is enabled by default                |   true   |                       true                       |                                    true                                    |
|     description     |                 A bit longer description of the issue                 |   true   |                 Infer from rule                  |    "Consecutive using directives must not be separated by blank lines."    |
| withCodeFixProvider |     Whether or not to create a code fix provider for the analyzer     |   true   |                       true                       |                                    true                                    |
|      withTests      | Whether or not to create tests for the analyzer and code fix provider |   true   |                       true                       |                                    true                                    |
|      examples       |          Code samples with issues to clarify how rule works           |   true   |                        []                        | ["using System;\n{\|GM0001:\|}\nusing System.Collections;\nclass Foo { }"] |
|        notes        |       Additional details about the behaviour or implementation        |   true   |                        -                         | "Create a test case for consecutive blank lines between using directives"  |

The requirements will be provided in JSON format. For example:

```json
{
  "rule": "Do not allow blank lines between using directives"
}
```

## Instructions

1. Create the diagnostic folder: GamesMayer.Diagnostics/Diagnostics/{category}/{id}
2. Create the diagnostic analyzer: GamesMayer.Diagnostics/Diagnostics/{category}/{id}/{id}Analyzer.cs
3. Create the diagnostic code fix provider if withCodeFixProvider is true: GamesMayer.Diagnostics/Diagnostics/{category}/{id}/{id}CodeFixProvider.cs
4. Create the diagnostic tests folder if withTests is true: GamesMayer.Diagnostics.Tests/Diagnostics/{category}/{id}
5. Create the diagnostic analyzer tests file if withTests is true: GamesMayer.Diagnostics.Tests/Diagnostics/{category}/{id}/{id}AnalyzerTests.cs
6. Create the diagnostic code fix provider tests file if withTests is true and withCodeFixProvider is true: GamesMayer.Diagnostics.Tests/Diagnostics/{category}/{id}/{id}CodeFixProviderTests.cs
7. Create the diagnostic documentation file: docs/Diagnostics/{category}/{id}/{id}.md
8. Add diagnostic with default severity to docs/.editorconfig.example file
9. Add the new diagnostic to [README.md](../README.md) file
10. Run tests to check that everything is alright
