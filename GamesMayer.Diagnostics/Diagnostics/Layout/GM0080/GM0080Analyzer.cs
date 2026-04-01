using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0080Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0080";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Break statement must be preceded by a blank line",
            messageFormat: "Add a blank line before the break statement",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A break statement must be preceded by exactly one blank line when it is not the first statement in its block.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.BreakStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var breakStatement = (BreakStatementSyntax)context.Node;

            if (breakStatement.Parent is not BlockSyntax block)
                return;

            var index = block.Statements.IndexOf(breakStatement);

            if (index <= 0)
                return;

            var previousStatement = block.Statements[index - 1];
            var tree = context.Node.SyntaxTree;

            var breakLine = tree.GetLineSpan(breakStatement.GetFirstToken().Span).StartLinePosition.Line;
            var previousStatementEndLine = tree.GetLineSpan(previousStatement.GetLastToken().Span).EndLinePosition.Line;

            var linesBetween = breakLine - previousStatementEndLine;

            if (linesBetween < 2)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, breakStatement.GetLocation()));
        }
    }
}
