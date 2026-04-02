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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0024CodeFixProvider))]
    [Shared]
    public sealed class GM0024CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0024Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix indentation",
                    createChangedDocument: ct => FixIndentationAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0024CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentationAsync(
            Document document,
            int tokenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var token = root.FindToken(tokenPosition);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            // Walk up to find the containing anonymous function
            BlockSyntax? block = null;
            SyntaxToken anchorToken = default;
            var node = token.Parent;

            while (node != null)
            {
                switch (node)
                {
                    case SimpleLambdaExpressionSyntax simpleLambda when simpleLambda.Body is BlockSyntax b:
                        block = b;
                        anchorToken = simpleLambda.ArrowToken;
                        break;
                    case ParenthesizedLambdaExpressionSyntax lambda when lambda.Body is BlockSyntax b:
                        block = b;
                        anchorToken = lambda.ArrowToken;
                        break;
                    case AnonymousMethodExpressionSyntax anonMethod when anonMethod.Block != null:
                        block = anonMethod.Block;
                        anchorToken = anonMethod.DelegateKeyword;
                        break;
                }

                if (block != null)
                    break;

                node = node.Parent;
            }

            if (block == null || anchorToken == default)
                return document;

            var anchorLine = syntaxTree.GetLineSpan(anchorToken.Span).StartLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var expectedBraceIndent = GM0024Analyzer.GetLineIndentation(sourceText, anchorLine);
            var indentUnit = GM0024Analyzer.GetIndentUnit(expectedBraceIndent, indentSize);
            var expectedStatementIndent = expectedBraceIndent + indentUnit;

            string expectedIndent = token == block.OpenBraceToken || token == block.CloseBraceToken
                ? expectedBraceIndent
                : expectedStatementIndent;

            var tokenLineNumber = syntaxTree.GetLineSpan(token.Span).StartLinePosition.Line;
            var tokenLine = sourceText.Lines[tokenLineNumber];
            var lineStr = tokenLine.ToString();

            int actualLen = 0;
            while (actualLen < lineStr.Length && (lineStr[actualLen] == ' ' || lineStr[actualLen] == '\t'))
                actualLen++;

            var indentSpan = new TextSpan(tokenLine.Start, actualLen);
            return document.WithText(sourceText.WithChanges(new TextChange(indentSpan, expectedIndent)));
        }
    }
}
