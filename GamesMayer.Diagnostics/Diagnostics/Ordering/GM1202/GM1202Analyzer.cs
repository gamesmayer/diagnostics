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
    public sealed class GM1202Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1202";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Elements must be ordered by access",
            messageFormat: "An element with '{0}' access must appear before an element with '{1}' access",
            category: "Ordering",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "An element within a C# code file is out of order in relation to the other elements in the code. Adjacent elements of the same kind should be ordered from most accessible to least accessible.");

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
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            var members = typeDeclaration.Members;

            for (int i = 0; i < members.Count - 1; i++)
            {
                if (members[i].IsKind(SyntaxKind.IncompleteMember) || members[i + 1].IsKind(SyntaxKind.IncompleteMember))
                    continue;

                int kind1 = GetLogicalKind(members[i].Kind());
                int kind2 = GetLogicalKind(members[i + 1].Kind());

                if (kind1 < 0 || kind2 < 0 || kind1 != kind2)
                    continue;

                var access1 = OrderingHelpers.GetAccessLevel(members[i].Modifiers);
                var access2 = OrderingHelpers.GetAccessLevel(members[i + 1].Modifiers);

                // Violation: current member is more accessible than previous (should come before)
                if (access2 < access1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor,
                        OrderingHelpers.GetNameOrIdentifierLocation(members[i + 1]),
                        OrderingHelpers.GetAccessLevelName(access2),
                        OrderingHelpers.GetAccessLevelName(access1)));
                }
            }
        }

        private static int GetLogicalKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.FieldDeclaration: return 0;
                case SyntaxKind.ConstructorDeclaration: return 1;
                case SyntaxKind.DestructorDeclaration: return 2;
                case SyntaxKind.DelegateDeclaration: return 3;
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.EventFieldDeclaration: return 4;
                case SyntaxKind.EnumDeclaration: return 5;
                case SyntaxKind.InterfaceDeclaration: return 6;
                case SyntaxKind.PropertyDeclaration: return 7;
                case SyntaxKind.IndexerDeclaration: return 8;
                case SyntaxKind.OperatorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration: return 9;
                case SyntaxKind.MethodDeclaration: return 10;
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.RecordStructDeclaration: return 11;
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.RecordDeclaration: return 12;
                default: return -1;
            }
        }
    }
}
