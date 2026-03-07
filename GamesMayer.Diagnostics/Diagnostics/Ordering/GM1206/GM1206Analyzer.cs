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
    public sealed class GM1206Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1206";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Declaration keywords must follow order",
            messageFormat: "The keyword '{0}' must appear before keyword '{1}'",
            category: "Ordering",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The keywords within the declaration of an element do not follow a standard ordering scheme. Within an element declaration, keywords must appear in a defined order: access modifiers first, then 'static', then all other keywords.");

        private static readonly ImmutableArray<SyntaxKind> HandledSyntaxKinds = ImmutableArray.Create(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.LocalFunctionStatement,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);

        private static readonly Action<SyntaxNodeAnalysisContext> DeclarationAction = HandleDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(DeclarationAction, HandledSyntaxKinds);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            var modifiers = GetModifiers(context.Node);
            if (modifiers == null || modifiers.Value.Count == 0)
                return;

            CheckModifiersOrder(context, modifiers.Value);
        }

        private static SyntaxTokenList? GetModifiers(SyntaxNode node)
        {
            switch (node)
            {
                case MemberDeclarationSyntax member: return member.Modifiers;
                case LocalFunctionStatementSyntax localFunction: return localFunction.Modifiers;
                default: return null;
            }
        }

        private static void CheckModifiersOrder(SyntaxNodeAnalysisContext context, SyntaxTokenList modifiers)
        {
            int maxRankSeen = -1;
            SyntaxToken lastHigherRankToken = default;

            foreach (var modifier in modifiers)
            {
                int rank = GetModifierRank(modifier.Kind());

                if (rank < maxRankSeen)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor,
                        modifier.GetLocation(),
                        modifier.ValueText,
                        lastHigherRankToken.ValueText));
                    return;
                }

                if (rank > maxRankSeen)
                {
                    maxRankSeen = rank;
                    lastHigherRankToken = modifier;
                }
            }
        }

        private static int GetModifierRank(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.PrivateKeyword:
                case SyntaxKind.ProtectedKeyword:
                case SyntaxKind.InternalKeyword:
                    return 0;

                case SyntaxKind.StaticKeyword:
                    return 1;

                default:
                    return 2;
            }
        }
    }
}
