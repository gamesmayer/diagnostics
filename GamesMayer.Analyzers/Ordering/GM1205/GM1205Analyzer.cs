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
    public sealed class GM1205Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1205";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Partial element does not have an access modifier defined",
            messageFormat: "Partial element does not have an access modifier defined",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A violation of this rule occurs when a partial element does not have an access modifier defined.");

        private static readonly ImmutableArray<SyntaxKind> TypeDeclarationKinds = ImmutableArray.Create(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);

        private static readonly Action<SyntaxNodeAnalysisContext> TypeDeclarationAction = HandleTypeDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(TypeDeclarationAction, TypeDeclarationKinds);
        }

        private static void HandleTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDeclarationNode = (TypeDeclarationSyntax)context.Node;

            if (typeDeclarationNode.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                if (!typeDeclarationNode.Modifiers.Any(SyntaxKind.PublicKeyword)
                    && !typeDeclarationNode.Modifiers.Any(SyntaxKind.InternalKeyword)
                    && !typeDeclarationNode.Modifiers.Any(SyntaxKind.ProtectedKeyword)
                    && !typeDeclarationNode.Modifiers.Any(SyntaxKind.PrivateKeyword))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, typeDeclarationNode.Identifier.GetLocation()));
                }
            }
        }
    }
}
