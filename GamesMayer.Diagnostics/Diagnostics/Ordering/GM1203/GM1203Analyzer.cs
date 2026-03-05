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
    public sealed class GM1203Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1203";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Constants must appear before fields",
            messageFormat: "A constant field must appear before a non-constant field",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A constant field is placed beneath a non-constant field. Constants should be placed above fields to indicate that the two are fundamentally different types of elements.");

        private static readonly ImmutableArray<SyntaxKind> TypeDeclarationKinds = ImmutableArray.Create(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
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

            bool prevWasNonConst = false;
            AccessLevel prevAccessLevel = AccessLevel.NotSpecified;

            foreach (var member in members)
            {
                if (!(member is FieldDeclarationSyntax field))
                {
                    prevWasNonConst = false;
                    prevAccessLevel = AccessLevel.NotSpecified;
                    continue;
                }

                AccessLevel currentAccessLevel = OrderingHelpers.GetAccessLevel(field.Modifiers);
                bool isConst = field.Modifiers.Any(SyntaxKind.ConstKeyword);

                bool sameGroup = currentAccessLevel == prevAccessLevel;

                if (sameGroup)
                {
                    if (prevWasNonConst && isConst)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            OrderingHelpers.GetNameOrIdentifierLocation(member)));
                    }
                }
                else
                {
                    // Different group: reset tracking
                    prevWasNonConst = false;
                }

                if (!isConst)
                    prevWasNonConst = true;

                prevAccessLevel = currentAccessLevel;
            }
        }
    }
}
