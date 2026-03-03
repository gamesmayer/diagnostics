// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace GamesMayer.Analyzers
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1210UsingDirectivesMustBeOrderedAlphabeticallyByNamespace : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1210";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Using directives must be ordered alphabetically by the namespaces",
            messageFormat: "A using directive for '{0}' must appear before a using directive for '{1}'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The using directives within a C# code file are not sorted alphabetically by namespace.");

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
            ProcessUsings(compilationUnit.Usings, context);
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDecl = (BaseNamespaceDeclarationSyntax)context.Node;
            ProcessUsings(namespaceDecl.Usings, context);
        }

        private static void ProcessUsings(SyntaxList<UsingDirectiveSyntax> usings, SyntaxNodeAnalysisContext context)
        {
            UsingDirectiveSyntax prevSystemUsing = null;
            UsingDirectiveSyntax prevNonSystemUsing = null;

            foreach (var usingDirective in usings)
            {
                // Skip alias and static usings — handled by GM1211 and GM1217
                if (usingDirective.Alias != null || !usingDirective.StaticKeyword.IsKind(SyntaxKind.None))
                    continue;

                if (usingDirective.Name == null)
                    continue;

                if (OrderingHelpers.IsPrecededByPreprocessorDirective(usingDirective))
                {
                    prevSystemUsing = null;
                    prevNonSystemUsing = null;
                    continue;
                }

                bool isSystem = OrderingHelpers.IsSystemUsingDirective(usingDirective)
                    && !OrderingHelpers.HasNamespaceAliasQualifier(usingDirective);

                if (isSystem)
                {
                    if (prevSystemUsing != null
                        && prevSystemUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                    {
                        if (OrderingHelpers.CompareNamespaces(prevSystemUsing.Name, usingDirective.Name) > 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Descriptor,
                                usingDirective.GetLocation(),
                                OrderingHelpers.GetNormalizedNamespace(usingDirective.Name),
                                OrderingHelpers.GetNormalizedNamespace(prevSystemUsing.Name)));
                            return;
                        }
                    }

                    prevSystemUsing = usingDirective;
                }
                else
                {
                    if (prevNonSystemUsing != null
                        && prevNonSystemUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                    {
                        if (OrderingHelpers.CompareNamespaces(prevNonSystemUsing.Name, usingDirective.Name) > 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Descriptor,
                                usingDirective.GetLocation(),
                                OrderingHelpers.GetNormalizedNamespace(usingDirective.Name),
                                OrderingHelpers.GetNormalizedNamespace(prevNonSystemUsing.Name)));
                            return;
                        }
                    }

                    prevNonSystemUsing = usingDirective;
                }
            }
        }
    }
}
