// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace GamesMayer.Diagnostics
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1207Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1207";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "The keyword 'protected' must come before 'internal'",
            messageFormat: "The keyword '{0}' must come before keyword '{1}'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The keyword 'protected' must come before 'internal' in the modifier list of a C# element. The keywords 'private' and 'protected' must also be ordered correctly.");

        private static readonly ImmutableArray<SyntaxKind> HandledSyntaxKinds = ImmutableArray.Create(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.StructDeclaration);

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
            var childTokens = context.Node?.ChildTokens();
            if (childTokens == null)
                return;

            bool protectedKeywordFound = false;
            bool internalKeywordFound = false;

            foreach (var childToken in childTokens)
            {
                if (childToken.IsKind(SyntaxKind.InternalKeyword))
                {
                    internalKeywordFound = true;
                    continue;
                }
                else if (childToken.IsKind(SyntaxKind.ProtectedKeyword))
                {
                    if (internalKeywordFound)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, childToken.GetLocation(), "protected", "internal"));
                        break;
                    }
                    else
                    {
                        protectedKeywordFound = true;
                        continue;
                    }
                }
                else if (protectedKeywordFound && childToken.IsKind(SyntaxKind.PrivateKeyword))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, childToken.GetLocation(), "private", "protected"));
                    break;
                }
            }
        }
    }
}
