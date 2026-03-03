// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace GamesMayer.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1201ElementsMustAppearInTheCorrectOrder : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1201";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Elements must appear in the correct order",
            messageFormat: "A {0} should appear before a {1}",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "An element within a C# code file is out of order in relation to the other elements in the code.");

        // Order of elements at file/namespace level
        private static readonly ImmutableArray<SyntaxKind> OuterOrder = ImmutableArray.Create(
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.ClassDeclaration);

        // Order of elements inside a type
        private static readonly ImmutableArray<SyntaxKind> TypeMemberOrder = ImmutableArray.Create(
            SyntaxKind.FieldDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.IndexerDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.ClassDeclaration);

        private static readonly Dictionary<SyntaxKind, string> MemberNames = new Dictionary<SyntaxKind, string>
        {
            [SyntaxKind.NamespaceDeclaration] = "namespace",
            [SyntaxKind.FileScopedNamespaceDeclaration] = "namespace",
            [SyntaxKind.DelegateDeclaration] = "delegate",
            [SyntaxKind.EnumDeclaration] = "enum",
            [SyntaxKind.InterfaceDeclaration] = "interface",
            [SyntaxKind.StructDeclaration] = "struct",
            [SyntaxKind.RecordStructDeclaration] = "record struct",
            [SyntaxKind.ClassDeclaration] = "class",
            [SyntaxKind.RecordDeclaration] = "record",
            [SyntaxKind.FieldDeclaration] = "field",
            [SyntaxKind.ConstructorDeclaration] = "constructor",
            [SyntaxKind.DestructorDeclaration] = "destructor",
            [SyntaxKind.EventDeclaration] = "event",
            [SyntaxKind.EventFieldDeclaration] = "event",
            [SyntaxKind.PropertyDeclaration] = "property",
            [SyntaxKind.IndexerDeclaration] = "indexer",
            [SyntaxKind.MethodDeclaration] = "method",
            [SyntaxKind.ConversionOperatorDeclaration] = "conversion",
            [SyntaxKind.OperatorDeclaration] = "operator",
        };

        private static readonly ImmutableArray<SyntaxKind> NamespaceKinds = ImmutableArray.Create(
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.FileScopedNamespaceDeclaration);

        private static readonly ImmutableArray<SyntaxKind> TypeDeclarationKinds = ImmutableArray.Create(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration);

        private static readonly Action<SyntaxNodeAnalysisContext> CompilationUnitAction = HandleCompilationUnit;
        private static readonly Action<SyntaxNodeAnalysisContext> NamespaceDeclarationAction = HandleNamespaceDeclaration;
        private static readonly Action<SyntaxNodeAnalysisContext> TypeDeclarationAction = HandleTypeDeclaration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(CompilationUnitAction, SyntaxKind.CompilationUnit);
            context.RegisterSyntaxNodeAction(NamespaceDeclarationAction, NamespaceKinds);
            context.RegisterSyntaxNodeAction(TypeDeclarationAction, TypeDeclarationKinds);
        }

        private static void HandleCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;
            HandleMemberList(context, compilationUnit.Members, OuterOrder);
        }

        private static void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDecl = (BaseNamespaceDeclarationSyntax)context.Node;
            HandleMemberList(context, namespaceDecl.Members, OuterOrder);
        }

        private static void HandleTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            HandleMemberList(context, typeDeclaration.Members, TypeMemberOrder);
        }

        private static void HandleMemberList(SyntaxNodeAnalysisContext context, SyntaxList<MemberDeclarationSyntax> members, ImmutableArray<SyntaxKind> order)
        {
            for (int i = 0; i < members.Count - 1; i++)
            {
                if (members[i].IsKind(SyntaxKind.IncompleteMember) || members[i + 1].IsKind(SyntaxKind.IncompleteMember))
                    continue;

                var currentKind = GetOrderingKind(members[i].Kind());
                var nextKind = GetOrderingKind(members[i + 1].Kind());

                int currentIndex = order.IndexOf(currentKind);
                int nextIndex = order.IndexOf(nextKind);

                if (currentIndex < 0 || nextIndex < 0)
                    continue;

                if (currentIndex > nextIndex)
                {
                    var nextMemberKind = members[i + 1].Kind();
                    var currentMemberKind = members[i].Kind();

                    string nextName = MemberNames.TryGetValue(nextMemberKind, out var n) ? n : "<unknown>";
                    string currentName = MemberNames.TryGetValue(currentMemberKind, out var c) ? c : "<unknown>";

                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor,
                        OrderingHelpers.GetNameOrIdentifierLocation(members[i + 1]),
                        nextName,
                        currentName));
                }
            }
        }

        private static SyntaxKind GetOrderingKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EventFieldDeclaration:
                    return SyntaxKind.EventDeclaration;
                case SyntaxKind.RecordDeclaration:
                    return SyntaxKind.ClassDeclaration;
                case SyntaxKind.RecordStructDeclaration:
                    return SyntaxKind.StructDeclaration;
                default:
                    return kind;
            }
        }
    }
}
