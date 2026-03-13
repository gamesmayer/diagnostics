using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0018Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0018";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Switch case clause declaration must be on a single line",
            messageFormat: "Write the switch case clause declaration on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Switch case clause declarations must be written on a single line. No line breaks are allowed within the case clause (from the case keyword to the colon).");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeCaseSwitchLabel, SyntaxKind.CaseSwitchLabel);
            context.RegisterSyntaxNodeAction(AnalyzeCasePatternSwitchLabel, SyntaxKind.CasePatternSwitchLabel);
        }

        private static void AnalyzeCaseSwitchLabel(SyntaxNodeAnalysisContext context)
        {
            var node = (CaseSwitchLabelSyntax)context.Node;
            ReportIfMultiLine(context, node.Keyword, node.ColonToken);
        }

        private static void AnalyzeCasePatternSwitchLabel(SyntaxNodeAnalysisContext context)
        {
            var node = (CasePatternSwitchLabelSyntax)context.Node;
            ReportIfMultiLine(context, node.Keyword, node.ColonToken);
        }

        private static void ReportIfMultiLine(SyntaxNodeAnalysisContext context, SyntaxToken startToken, SyntaxToken endToken)
        {
            var tree = context.Node.SyntaxTree;
            var startLine = tree.GetLineSpan(startToken.Span).StartLinePosition.Line;
            var endLine = tree.GetLineSpan(endToken.Span).EndLinePosition.Line;

            if (startLine != endLine)
            {
                var location = Location.Create(tree, TextSpan.FromBounds(startToken.SpanStart, endToken.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
            }
        }
    }
}