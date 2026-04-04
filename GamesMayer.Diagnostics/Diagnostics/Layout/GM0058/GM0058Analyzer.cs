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
            title: "Colon must be on the same line as the declaration",
            messageFormat: "Move ':' to the same line as the declaration",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The ':' in inheritance clauses, 'where' constraint clauses, and constructor initializers must be on the same line as the declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.BaseList,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer,
                SyntaxKind.TypeParameterConstraintClause);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken colonToken;

            if (context.Node is BaseListSyntax baseList)
            {
                if (baseList.Types.Count == 0)
                    return;
                colonToken = baseList.ColonToken;
            }
            else if (context.Node is ConstructorInitializerSyntax ctorInit)
            {
                colonToken = ctorInit.ColonToken;
            }
            else if (context.Node is TypeParameterConstraintClauseSyntax constraintClause)
            {
                colonToken = constraintClause.ColonToken;
            }
            else
            {
                return;
            }

            var previousToken = colonToken.GetPreviousToken();
            if (previousToken == default)
                return;

            var tree = context.Node.SyntaxTree;
            var previousLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
            var colonLine = tree.GetLineSpan(colonToken.Span).StartLinePosition.Line;

            if (previousLine == colonLine)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, colonToken.GetLocation()));
        }
    }
}
