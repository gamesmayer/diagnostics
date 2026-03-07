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
    public sealed class GM1212Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1212";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "A get accessor appears after a set accessor within a property or indexer",
            messageFormat: "A get accessor must appear before a set accessor",
            category: "Ordering",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A get accessor appears after a set accessor within a property or indexer. To comply with this rule, the get accessor should appear before the set accessor.");

        private static readonly Action<SyntaxNodeAnalysisContext> PropertyDeclarationAction = HandlePropertyDeclaration;
        private static readonly Action<SyntaxNodeAnalysisContext> IndexerDeclarationAction = HandleIndexerDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(PropertyDeclarationAction, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(IndexerDeclarationAction, SyntaxKind.IndexerDeclaration);
        }

        private static void HandlePropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            AnalyzeProperty(context, (BasePropertyDeclarationSyntax)context.Node);
        }

        private static void HandleIndexerDeclaration(SyntaxNodeAnalysisContext context)
        {
            AnalyzeProperty(context, (BasePropertyDeclarationSyntax)context.Node);
        }

        private static void AnalyzeProperty(SyntaxNodeAnalysisContext context, BasePropertyDeclarationSyntax propertyDeclaration)
        {
            if (propertyDeclaration?.AccessorList == null)
                return;

            var accessors = propertyDeclaration.AccessorList.Accessors;
            if (propertyDeclaration.AccessorList.IsMissing || accessors.Count != 2)
                return;

            if ((accessors[0].IsKind(SyntaxKind.SetAccessorDeclaration) || accessors[0].IsKind(SyntaxKind.InitAccessorDeclaration))
                && accessors[1].IsKind(SyntaxKind.GetAccessorDeclaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, accessors[0].Keyword.GetLocation()));
            }
        }
    }
}
