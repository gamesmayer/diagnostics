using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0051CodeFixProvider))]
    [Shared]
    public sealed class GM0051CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0051Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move return expression to same line as 'return'",
                    createChangedDocument: ct => FixReturnExpressionAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0051CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixReturnExpressionAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var firstExpressionToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var returnToken = firstExpressionToken.GetPreviousToken();

            if (!returnToken.IsKind(SyntaxKind.ReturnKeyword))
                return document;

            var change = new TextChange(TextSpan.FromBounds(returnToken.Span.End, firstExpressionToken.Span.Start), " ");

            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
