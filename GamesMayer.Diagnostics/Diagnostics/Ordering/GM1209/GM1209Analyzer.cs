// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace GamesMayer.Diagnostics
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1209Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1209";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Using alias directives must be placed after other using directives",
            messageFormat: "A using alias directive must appear after all other using directives",
            category: "Ordering",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A using-alias directive is positioned before a regular using directive. Using-alias directives should appear after all other using directives.");

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
            for (int i = 0; i < usings.Count; i++)
            {
                var usingDirective = usings[i];
                bool isNotLast = i + 1 < usings.Count;

                if (usingDirective.Alias != null && isNotLast)
                {
                    var nextUsingDirective = usings[i + 1];

                    // Only compare usings with the same 'global' modifier
                    if (nextUsingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) != usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                        continue;

                    if (nextUsingDirective.Alias == null
                        && nextUsingDirective.StaticKeyword.IsKind(SyntaxKind.None)
                        && !OrderingHelpers.IsPrecededByPreprocessorDirective(nextUsingDirective))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingDirective.GetLocation()));
                    }
                }
            }
        }
    }
}
