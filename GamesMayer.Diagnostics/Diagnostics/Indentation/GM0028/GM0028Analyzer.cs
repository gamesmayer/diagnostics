using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0028Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0028";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Closing parenthesis indentation in multi-line list",
            messageFormat: "Indent closing parenthesis at the same level as the line that opens the list",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In multi-line argument or parameter lists, the closing parenthesis must be indented at the same level as the line containing the opening parenthesis.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
        }

        private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var argumentList = (ArgumentListSyntax)context.Node;
            AnalyzeList(context, argumentList.Arguments, argumentList.OpenParenToken, argumentList.CloseParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            AnalyzeList(context, parameterList.Parameters, parameterList.OpenParenToken, parameterList.CloseParenToken);
        }

        private static void AnalyzeList<TNode>(
            SyntaxNodeAnalysisContext context,
            SeparatedSyntaxList<TNode> items,
            SyntaxToken openParenToken,
            SyntaxToken closeParenToken)
            where TNode : SyntaxNode
        {
            if (items.Count == 0)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;

            if (openParenLine == closeParenLine)
            {
                return;
            }

            var lastItem = items[items.Count - 1];
            var lastItemLine = tree.GetLineSpan(lastItem.Span).EndLinePosition.Line;

            // GM0026 handles cases where ')' is on the same line as the last item.
            if (closeParenLine == lastItemLine)
            {
                return;
            }

            var sourceText = tree.GetText(context.CancellationToken);
            var expectedIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, openParenLine);
            var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, closeParenLine);

            if (actualIndentation != expectedIndentation)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, closeParenToken.GetLocation()));
            }
        }
    }
}
