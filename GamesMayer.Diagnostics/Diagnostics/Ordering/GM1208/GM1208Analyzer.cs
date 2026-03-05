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
    public sealed class GM1208Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1208";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "System using directives must be placed before other using directives",
            messageFormat: "A using directive for '{0}' must appear before a using directive for '{1}'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A using directive which declares a member of the System namespace appears after a using directive which declares a member of a different namespace.");

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
            string systemUsingMustBeBeforeThis = null;

            for (int i = 1; i < usings.Count; i++)
            {
                var usingDirective = usings[i];

                if (usingDirective.Alias != null
                    || !usingDirective.StaticKeyword.IsKind(SyntaxKind.None)
                    || OrderingHelpers.IsPrecededByPreprocessorDirective(usingDirective))
                {
                    continue;
                }

                if (OrderingHelpers.IsSystemUsingDirective(usingDirective)
                    && !OrderingHelpers.HasNamespaceAliasQualifier(usingDirective))
                {
                    if (systemUsingMustBeBeforeThis != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            usingDirective.GetLocation(),
                            OrderingHelpers.GetNormalizedNamespace(usingDirective.Name),
                            systemUsingMustBeBeforeThis));
                        continue;
                    }

                    var previousUsing = usings[i - 1];

                    // Only compare usings with the same 'global' modifier
                    if (previousUsing.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) != usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                        continue;

                    if (!OrderingHelpers.IsSystemUsingDirective(previousUsing)
                        || OrderingHelpers.HasNamespaceAliasQualifier(previousUsing)
                        || !previousUsing.StaticKeyword.IsKind(SyntaxKind.None))
                    {
                        systemUsingMustBeBeforeThis = OrderingHelpers.GetNormalizedNamespace(previousUsing.Name);
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            usingDirective.GetLocation(),
                            OrderingHelpers.GetNormalizedNamespace(usingDirective.Name),
                            systemUsingMustBeBeforeThis));
                    }
                }
            }
        }
    }
}
