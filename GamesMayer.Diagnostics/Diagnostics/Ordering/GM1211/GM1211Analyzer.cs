// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace GamesMayer.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1211Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1211";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Using alias directives must be ordered alphabetically by alias name",
            messageFormat: "A using alias directive for '{0}' must appear before a using alias directive for '{1}'",
            category: "Ordering",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The using-alias directives within a C# code file are not sorted alphabetically by alias name.");

        private static readonly ImmutableArray<SyntaxKind> NamespaceKinds = ImmutableArray.Create(
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

        private static readonly Action<SyntaxNodeAnalysisContext> CompilationUnitAction = HandleCompilationUnit;
        private static readonly Action<SyntaxNodeAnalysisContext> NamespaceDeclarationAction = HandleNamespaceDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(CompilationUnitAction, SyntaxKind.CompilationUnit);
            context.RegisterSyntaxNodeAction(NamespaceDeclarationAction, NamespaceKinds);
        }

        private static void HandleCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;
            HandleUsingDirectives(context, compilationUnit.Usings);
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDecl = (BaseNamespaceDeclarationSyntax)context.Node;
            HandleUsingDirectives(context, namespaceDecl.Usings);
        }

        private static void HandleUsingDirectives(SyntaxNodeAnalysisContext context, SyntaxList<UsingDirectiveSyntax> usingDirectives)
        {
            if (usingDirectives.Count == 0)
                return;

            var usingAliasNames = new List<string>();
            UsingDirectiveSyntax prevAliasUsing = null;

            foreach (var usingDirective in usingDirectives)
            {
                if (OrderingHelpers.IsPrecededByPreprocessorDirective(usingDirective))
                {
                    usingAliasNames.Clear();
                    prevAliasUsing = null;
                }

                if (prevAliasUsing != null
                    && prevAliasUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) != usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                {
                    usingAliasNames.Clear();
                    prevAliasUsing = null;
                }

                if (usingDirective.Alias?.Name?.IsMissing != false)
                    continue;

                string currentAliasName = usingDirective.Alias.Name.Identifier.ValueText;

                if (prevAliasUsing != null)
                {
                    string currentLower = currentAliasName.ToLowerInvariant();
                    string prevAliasName = prevAliasUsing.Alias.Name.Identifier.ValueText;

                    if (string.CompareOrdinal(prevAliasName.ToLowerInvariant(), currentLower) > 0)
                    {
                        // Find the alias before which the current alias should be placed
                        foreach (string aliasName in usingAliasNames)
                        {
                            if (string.CompareOrdinal(aliasName.ToLowerInvariant(), currentLower) > 0)
                            {
                                prevAliasName = aliasName;
                                break;
                            }
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            usingDirective.GetLocation(),
                            currentAliasName,
                            prevAliasName));
                        return;
                    }
                }

                usingAliasNames.Add(currentAliasName);
                prevAliasUsing = usingDirective;
            }
        }
    }
}
