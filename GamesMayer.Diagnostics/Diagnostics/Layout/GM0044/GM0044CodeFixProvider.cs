using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0044CodeFixProvider))]
    [Shared]
    public sealed class GM0044CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0044Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank line between => and body",
                    createChangedDocument: ct => RemoveBlankLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0044CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> RemoveBlankLineAsync(
            Document document,
            int diagnosticPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var tokenAtDiagnostic = root.FindToken(diagnosticPosition, findInsideTrivia: true);
            var lambda = FindContainingLambda(tokenAtDiagnostic.Parent, diagnosticPosition, sourceText, root.SyntaxTree);
            if (lambda == null)
                return document;

            var bodyFirstToken = lambda.Body.GetFirstToken();
            if (bodyFirstToken == default)
                return document;

            var bodyLine = sourceText.Lines.GetLineFromPosition(bodyFirstToken.SpanStart);
            var bodyIndentLength = bodyFirstToken.SpanStart - bodyLine.Start;
            var bodyIndentation = sourceText.ToString(new TextSpan(bodyLine.Start, bodyIndentLength));

            var replacementSpan = TextSpan.FromBounds(lambda.ArrowToken.Span.End, bodyFirstToken.SpanStart);
            var replacementText = DetectNewline(sourceText) + bodyIndentation;
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, replacementText));

            return document.WithText(updatedText);
        }

        private static LambdaExpressionSyntax? FindContainingLambda(
            SyntaxNode? startNode,
            int diagnosticPosition,
            SourceText sourceText,
            SyntaxTree syntaxTree)
        {
            var node = startNode;
            while (node != null)
            {
                if (node is LambdaExpressionSyntax lambda)
                {
                    var bodyFirstToken = lambda.Body.GetFirstToken();
                    if (bodyFirstToken != default
                        && lambda.ArrowToken.Span.End <= diagnosticPosition
                        && diagnosticPosition <= bodyFirstToken.SpanStart
                        && BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, syntaxTree, lambda.ArrowToken, bodyFirstToken, out _))
                    {
                        return lambda;
                    }
                }

                node = node.Parent;
            }

            return null;
        }

        private static string DetectNewline(SourceText sourceText)
        {
            for (int i = 0; i < sourceText.Length - 1; i++)
            {
                if (sourceText[i] == '\r' && sourceText[i + 1] == '\n')
                    return "\r\n";
                if (sourceText[i] == '\n')
                    return "\n";
            }

            return "\n";
        }
    }
}
