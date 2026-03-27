using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0058Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0058";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Inheritance list must be on the same line as the type declaration",
            messageFormat: "Write the inheritance list on the same line as the type declaration",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The inheritance list (the ':' symbol and all base types) must be on the same line as the type declaration identifier.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBaseList, SyntaxKind.BaseList);
        }

        private static void AnalyzeBaseList(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;
            if (baseList.Types.Count == 0)
                return;

            var identifier = GetTypeIdentifier(baseList.Parent);
            if (identifier == null)
                return;

            var tree = baseList.SyntaxTree;
            var identifierLine = tree.GetLineSpan(identifier.Value.Span).StartLinePosition.Line;
            var lastTypeLine = tree.GetLineSpan(baseList.Types.Last().GetLastToken().Span).EndLinePosition.Line;

            if (identifierLine != lastTypeLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, baseList.GetLocation()));
            }
        }

        private static SyntaxToken? GetTypeIdentifier(SyntaxNode? parent)
        {
            return parent switch
            {
                ClassDeclarationSyntax classDecl => classDecl.Identifier,
                StructDeclarationSyntax structDecl => structDecl.Identifier,
                InterfaceDeclarationSyntax interfaceDecl => interfaceDecl.Identifier,
                RecordDeclarationSyntax recordDecl => recordDecl.Identifier,
                EnumDeclarationSyntax enumDecl => enumDecl.Identifier,
                _ => null
            };
        }
    }
}
