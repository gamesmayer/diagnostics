# Project Context

## Introduction

Set of C# diagnostics for .editorconfig to force developers follow a specific coding style.

## Project Structure

Only the most relevant files and directories are mentioned in the following diagram.

```
.
├── .ai                                         # Documentation for artificial intelligence agents
├── docs                                        # Documentation files
├── GamesMayer.Diagnostics                        # Source code
    └── Diagnostics
        └── [CategoryName]                      # Layout, Ordering, etc.
            └── GM0001
                ├── GMOOO1Analyzer.cs           # GM0001 analyzer
                └── GM0001CodeFixProvider.cs    # GM0001 code fix provider
├── GamesMayer.Diagnostics.Tests                  # Tests source code
    └── Diagnostics
        └── [CategoryName]
            └── GM0001
                ├── GMOOO1AnalyzerTests.cs      # GM0001 analyzer tests
                └── GM0001CodeFixProvider.cs    # GM0001 code fix provider tests
└── gamesmayer-diagnostics.slnx                   # C# solution
```

## What Is a Diagnostic?

The .NET Compiler Platform SDK provides the tools you need to create custom diagnostics (analyzers), code fixes, code refactoring, and diagnostic suppressors that target C#. An analyzer contains code that recognizes violations of your rule. Your code fix provider contains the code that fixes the violation. The rules you implement can be anything from code structure to coding style to naming conventions and more.

- A diagnostic is the combination of an analyzer and a code fix provider (optional).
- An analyzer is a class that inherits from Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer and whose purpose is to report a certain pattern in our code.
- A code fix provider is a class that inherits from Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider and applies a change to the code to fix the diagnostic reported by the analyzer.
- A diagnostic is identified by a unique ID: GM0001, GM0002, GM0003, etc.
