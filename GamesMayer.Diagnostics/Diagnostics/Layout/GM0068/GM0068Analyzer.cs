using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0068Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0068";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Multiple statements on one line",
            messageFormat: "Move each statement to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Multiple statements must not appear on the same line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            if (block.Statements.Count < 2)
                return;

            var tree = block.SyntaxTree;

            for (int i = 1; i < block.Statements.Count; i++)
            {
                var prevStatement = block.Statements[i - 1];
                var currStatement = block.Statements[i];

                var prevEndLine = tree.GetLineSpan(prevStatement.Span).EndLinePosition.Line;
                var currStartLine = tree.GetLineSpan(currStatement.Span).StartLinePosition.Line;

                if (prevEndLine == currStartLine)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, currStatement.GetLocation()));
            }
        }
    }
}
