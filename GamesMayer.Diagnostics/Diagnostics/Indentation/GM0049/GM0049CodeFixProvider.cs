using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0049CodeFixProvider))]
    [Shared]
    public sealed class GM0049CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0049Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix wrapped item indentation",
                    createChangedDocument: ct => FixIndentationAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0049CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentationAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var binary = root.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()
                .FirstOrDefault(candidate =>
                    GM0049Analyzer.IsSupportedBinaryKind(candidate.Kind())
                    && candidate.Right.GetFirstToken().SpanStart == diagnosticSpan.Start);

            if (binary == null)
                return document;

            var rightFirstToken = binary.Right.GetFirstToken();
            var rightLine = syntaxTree.GetLineSpan(rightFirstToken.Span).StartLinePosition.Line;

            var chainRoot = GM0049Analyzer.GetTopMostBinaryExpression(binary);
            var expressionStartLine = syntaxTree.GetLineSpan(chainRoot.GetFirstToken().Span).StartLinePosition.Line;

            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, expressionStartLine);

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeValue)
                && int.TryParse(indentSizeValue, out var parsedIndentSize)
                && parsedIndentSize > 0)
            {
                indentSize = parsedIndentSize;
            }

            var expectedIndentation = GM0049Analyzer.GetExpectedIndentation(baseIndentation, indentSize);
            var textLine = sourceText.Lines[rightLine];
            var lineText = textLine.ToString();
            var actualIndentLength = GM0049Analyzer.GetIndentationLength(lineText);

            var indentationSpan = new TextSpan(textLine.Start, actualIndentLength);
            var updatedText = sourceText.WithChanges(new TextChange(indentationSpan, expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
