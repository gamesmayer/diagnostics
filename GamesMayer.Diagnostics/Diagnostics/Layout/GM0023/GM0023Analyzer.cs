using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0023Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0023";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Misaligned parameter or argument in multi-line list",
            messageFormat: "Indent this parameter or argument by one step relative to the containing statement",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When arguments or parameters are each on their own line, they must be indented by one step relative to the containing statement.");

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
            AnalyzeList(context, argumentList.Arguments, argumentList.OpenParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            AnalyzeList(context, parameterList.Parameters, parameterList.OpenParenToken);
        }

        private static void AnalyzeList<TNode>(
            SyntaxNodeAnalysisContext context,
            SeparatedSyntaxList<TNode> items,
            SyntaxToken openParenToken)
            where TNode : SyntaxNode
        {
            if (items.Count == 0)
                return;

            var tree = context.Node.SyntaxTree;
            var openParenLineNumber = tree.GetLineSpan(openParenToken.Span).EndLinePosition.Line;

            // Only apply rule when first item is on a different line from the opening paren
            var firstItemFirstToken = items[0].GetFirstToken();
            if (firstItemFirstToken == default)
                return;

            var firstItemLine = tree.GetLineSpan(firstItemFirstToken.Span).StartLinePosition.Line;
            if (firstItemLine == openParenLineNumber)
                return;

            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);
            var expectedIndentation = GetExpectedIndentation(sourceText, openParenLineNumber, indentSize);

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var firstToken = item.GetFirstToken();
                if (firstToken == default)
                    continue;

                var previousToken = i == 0
                    ? openParenToken
                    : items.GetSeparator(i - 1);

                var itemLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                var previousTokenLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;

                if (itemLine == previousTokenLine)
                    continue;

                var actualIndentation = GetActualLineIndentation(sourceText, itemLine);
                if (actualIndentation != expectedIndentation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, item.GetLocation()));
                }
            }
        }

        internal static string GetExpectedIndentation(SourceText sourceText, int openParenLineNumber, int indentSize)
        {
            var openParenLine = sourceText.Lines[openParenLineNumber].ToString();

            var baseIndentBuilder = new StringBuilder();
            foreach (char c in openParenLine)
            {
                if (c == ' ' || c == '\t') baseIndentBuilder.Append(c);
                else break;
            }
            var baseIndent = baseIndentBuilder.ToString();

            string indentUnit = baseIndent.Length > 0 && baseIndent[0] == '\t'
                ? "\t"
                : new string(' ', indentSize);

            return baseIndent + indentUnit;
        }

        internal static string GetActualLineIndentation(SourceText sourceText, int lineNumber)
        {
            var line = sourceText.Lines[lineNumber].ToString();
            var sb = new StringBuilder();
            foreach (char c in line)
            {
                if (c == ' ' || c == '\t') sb.Append(c);
                else break;
            }
            return sb.ToString();
        }

        private static int GetIndentSize(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue("indent_size", out var value) && int.TryParse(value, out var size) && size > 0)
                return size;
            return 4;
        }
    }
}
