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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0137CodeFixProvider))]
    [Shared]
    public sealed class GM0137CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0137Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move operand to the same line as the cast expression",
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0137CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var castExpr = node as CastExpressionSyntax
                ?? node?.AncestorsAndSelf().OfType<CastExpressionSyntax>().FirstOrDefault();
            if (castExpr == null)
                return document;

            var closeParenEnd = castExpr.CloseParenToken.Span.End;
            var exprStart = castExpr.Expression.GetFirstToken().SpanStart;
            var triviaLength = exprStart - closeParenEnd;

            if (triviaLength <= 0)
                return document;

            var updatedText = sourceText.Replace(new TextSpan(closeParenEnd, triviaLength), "");
            return document.WithText(updatedText);
        }
    }
}
