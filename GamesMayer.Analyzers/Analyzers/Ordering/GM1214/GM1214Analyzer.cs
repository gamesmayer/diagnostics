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
    public sealed class GM1214Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1214";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Readonly elements must appear before non-readonly elements",
            messageFormat: "A readonly field must appear before a non-readonly field",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A readonly field is positioned beneath a non-readonly field of the same type. Readonly fields should be placed above non-readonly fields.");

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

            bool prevWasNonReadonly = false;
            AccessLevel prevAccessLevel = AccessLevel.NotSpecified;
            bool prevWasStatic = false;
            bool prevWasConst = false;

            foreach (var member in members)
            {
                if (!(member is FieldDeclarationSyntax field))
                {
                    prevWasNonReadonly = false;
                    prevAccessLevel = AccessLevel.NotSpecified;
                    prevWasStatic = false;
                    prevWasConst = false;
                    continue;
                }

                AccessLevel currentAccessLevel = OrderingHelpers.GetAccessLevel(field.Modifiers);
                bool isConst = field.Modifiers.Any(SyntaxKind.ConstKeyword);
                bool isReadonly = isConst || field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
                bool isStatic = isConst || field.Modifiers.Any(SyntaxKind.StaticKeyword);

                bool sameGroup = currentAccessLevel == prevAccessLevel
                    && isStatic == prevWasStatic
                    && isConst == prevWasConst;

                if (sameGroup)
                {
                    if (prevWasNonReadonly && isReadonly)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            Descriptor,
                            OrderingHelpers.GetNameOrIdentifierLocation(member)));
                    }
                }
                else
                {
                    prevWasNonReadonly = false;
                }

                if (!isReadonly)
                    prevWasNonReadonly = true;

                prevAccessLevel = currentAccessLevel;
                prevWasStatic = isStatic;
                prevWasConst = isConst;
            }
        }
    }
}
