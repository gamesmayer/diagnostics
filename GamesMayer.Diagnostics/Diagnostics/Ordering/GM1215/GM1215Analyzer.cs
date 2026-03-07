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
    public sealed class GM1215Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1215";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Using static directives must be ordered alphabetically",
            messageFormat: "A 'using static' directive for '{0}' must appear before a 'using static' directive for '{1}'",
            category: "Ordering",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The using static directives within a C# code file are not sorted alphabetically by namespace.");

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
            UsingDirectiveSyntax lastSystemStaticUsing = null;
            UsingDirectiveSyntax lastNonSystemStaticUsing = null;
            UsingDirectiveSyntax firstNonSystemStatic = null;

            foreach (var usingDirective in usings)
            {
                if (!usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                    continue;

                if (OrderingHelpers.IsPrecededByPreprocessorDirective(usingDirective))
                {
                    lastSystemStaticUsing = null;
                    lastNonSystemStaticUsing = null;
                    firstNonSystemStatic = null;
                    continue;
                }

                bool isSystem = OrderingHelpers.IsSystemUsingDirective(usingDirective)
                    && !OrderingHelpers.HasNamespaceAliasQualifier(usingDirective);

                if (usingDirective.Name == null)
                    continue;

                if (isSystem)
                {
                    // System static using after non-system static using → violation
                    if (firstNonSystemStatic != null
                        && firstNonSystemStatic.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            firstNonSystemStatic.GetLocation(),
                            OrderingHelpers.GetNormalizedNamespace(firstNonSystemStatic.Name),
                            OrderingHelpers.GetNormalizedNamespace(usingDirective.Name)));
                        return;
                    }

                    if (lastSystemStaticUsing != null
                        && lastSystemStaticUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                    {
                        if (OrderingHelpers.CompareNamespaces(lastSystemStaticUsing.Name, usingDirective.Name) > 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Descriptor,
                                lastSystemStaticUsing.GetLocation(),
                                OrderingHelpers.GetNormalizedNamespace(lastSystemStaticUsing.Name),
                                OrderingHelpers.GetNormalizedNamespace(usingDirective.Name)));
                            return;
                        }
                    }

                    lastSystemStaticUsing = usingDirective;
                }
                else
                {
                    if (lastNonSystemStaticUsing != null
                        && lastNonSystemStaticUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) == usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                    {
                        if (OrderingHelpers.CompareNamespaces(lastNonSystemStaticUsing.Name, usingDirective.Name) > 0)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                Descriptor,
                                lastNonSystemStaticUsing.GetLocation(),
                                OrderingHelpers.GetNormalizedNamespace(lastNonSystemStaticUsing.Name),
                                OrderingHelpers.GetNormalizedNamespace(usingDirective.Name)));
                            return;
                        }
                    }

                    lastNonSystemStaticUsing = usingDirective;
                    if (firstNonSystemStatic == null)
                        firstNonSystemStatic = usingDirective;
                }
            }
        }
    }
}
