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
    public sealed class GM1200Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1200";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "A C# using directive is placed inside a namespace element",
            messageFormat: "A using directive should appear outside a namespace declaration",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A using directive is placed inside a namespace element. Using directives should appear outside namespace declarations.");

        private static readonly ImmutableArray<SyntaxKind> NamespaceKinds = ImmutableArray.Create(
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

        private static readonly Action<SyntaxNodeAnalysisContext> NamespaceDeclarationAction = HandleNamespaceDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(NamespaceDeclarationAction, NamespaceKinds);
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDecl = (BaseNamespaceDeclarationSyntax)context.Node;

            foreach (var usingDirective in namespaceDecl.Usings)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingDirective.GetLocation()));
            }
        }
    }
}
