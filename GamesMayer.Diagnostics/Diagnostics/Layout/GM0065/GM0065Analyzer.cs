using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0065Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0065";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Return statement must be preceded by a blank line",
            messageFormat: "Add a blank line before the return statement",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A return statement must be preceded by exactly one blank line when it is not the first (and only) statement in its block.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ReturnStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;

            if (returnStatement.Parent is not BlockSyntax block)
                return;

            var index = block.Statements.IndexOf(returnStatement);

            if (index <= 0)
                return;

            var previousStatement = block.Statements[index - 1];
            var tree = context.Node.SyntaxTree;

            var returnLine = tree.GetLineSpan(returnStatement.GetFirstToken().Span).StartLinePosition.Line;
            var previousStatementEndLine = tree.GetLineSpan(previousStatement.GetLastToken().Span).EndLinePosition.Line;

            var linesBetween = returnLine - previousStatementEndLine;

            if (linesBetween < 2)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, returnStatement.GetLocation()));
        }
    }
}
