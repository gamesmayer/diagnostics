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
    public sealed class GM1216Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1216";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Using static directives must be placed at the correct location",
            messageFormat: "A 'using static' directive must appear after normal using directives and before alias using directives",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A using static directive is placed before a normal using directive or after an alias using directive. Using static directives should appear between normal and alias using directives.");

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
            CheckUsingDeclarations(context, compilationUnit.Usings);
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDecl = (BaseNamespaceDeclarationSyntax)context.Node;
            CheckUsingDeclarations(context, namespaceDecl.Usings);
        }

        private static void CheckUsingDeclarations(SyntaxNodeAnalysisContext context, SyntaxList<UsingDirectiveSyntax> usings)
        {
            UsingDirectiveSyntax lastStaticUsing = null;
            UsingDirectiveSyntax lastAliasUsing = null;

            foreach (var usingDirective in usings)
            {
                if (OrderingHelpers.IsPrecededByPreprocessorDirective(usingDirective))
                {
                    lastStaticUsing = null;
                    lastAliasUsing = null;
                }

                bool sameGlobalAsStatic = lastStaticUsing == null
                    || lastStaticUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);

                bool sameGlobalAsAlias = lastAliasUsing == null
                    || lastAliasUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);

                bool isStatic = usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
                bool isAlias = usingDirective.Alias != null;

                if (isStatic)
                {
                    // static using after alias using → violation
                    if (lastAliasUsing != null && sameGlobalAsAlias)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingDirective.GetLocation()));
                        return;
                    }

                    lastStaticUsing = usingDirective;
                }
                else if (isAlias)
                {
                    lastAliasUsing = usingDirective;
                }
                else
                {
                    // regular using after static using → violation
                    if (lastStaticUsing != null && sameGlobalAsStatic)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingDirective.GetLocation()));
                        return;
                    }
                }
            }
        }
    }
}
