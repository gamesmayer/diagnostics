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
    public sealed class GM1213Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1213";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "An add accessor appears after a remove accessor within an event",
            messageFormat: "An add accessor must appear before a remove accessor",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "An add accessor appears after a remove accessor within an event. To comply with this rule, the add accessor should appear before the remove accessor.");

        private static readonly Action<SyntaxNodeAnalysisContext> EventDeclarationAction = HandleEventDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(EventDeclarationAction, SyntaxKind.EventDeclaration);
        }

        private static void HandleEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            var eventDeclaration = (EventDeclarationSyntax)context.Node;

            if (eventDeclaration.AccessorList == null)
                return;

            var accessors = eventDeclaration.AccessorList.Accessors;
            if (eventDeclaration.AccessorList.IsMissing || accessors.Count != 2)
                return;

            if (accessors[0].IsKind(SyntaxKind.RemoveAccessorDeclaration)
                && accessors[1].IsKind(SyntaxKind.AddAccessorDeclaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, accessors[0].Keyword.GetLocation()));
            }
        }
    }
}
