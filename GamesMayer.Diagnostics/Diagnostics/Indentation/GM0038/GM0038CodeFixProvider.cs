using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0038CodeFixProvider))]
    [Shared]
    public sealed class GM0038CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0038Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix fluent-chain segment indentation",
                    createChangedDocument: ct => FixIndentationAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0038CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentationAsync(
            Document document,
            int dotPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var dotToken = root.FindToken(dotPosition);
            if (dotToken.Parent is not MemberAccessExpressionSyntax memberAccess
                || memberAccess.OperatorToken != dotToken)
            {
                return document;
            }

            // Walk up the chain to find the outermost chain expression.
            ExpressionSyntax chainExpression = memberAccess;
            while (true)
            {
                var parent = chainExpression.Parent;
                if (parent is InvocationExpressionSyntax invocation && invocation.Expression == chainExpression)
                    chainExpression = invocation;
                else if (parent is MemberAccessExpressionSyntax outerMemberAccess && outerMemberAccess.Expression == chainExpression)
                    chainExpression = outerMemberAccess;
                else
                    break;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var chainStartLine = syntaxTree.GetLineSpan(chainExpression.GetFirstToken().Span).StartLinePosition.Line;
            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, chainStartLine);

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var expectedIndentation = GM0038Analyzer.GetExpectedIndentation(baseIndentation, indentSize);

            var dotLine = syntaxTree.GetLineSpan(dotToken.Span).StartLinePosition.Line;
            var dotTextLine = sourceText.Lines[dotLine];
            var lineStr = dotTextLine.ToString();

            int actualLen = 0;
            while (actualLen < lineStr.Length && (lineStr[actualLen] == ' ' || lineStr[actualLen] == '\t'))
                actualLen++;

            var indentationSpan = new TextSpan(dotTextLine.Start, actualLen);
            var updatedText = sourceText.WithChanges(new TextChange(indentationSpan, expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
