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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0111CodeFixProvider))]
    [Shared]
    public sealed class GM0111CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0111Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move ';' next to previous token",
                    createChangedDocument: ct => RemoveGapBeforeSemicolonAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0111CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> RemoveGapBeforeSemicolonAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var semicolonToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!semicolonToken.IsKind(SyntaxKind.SemicolonToken))
                return document;

            var previousToken = semicolonToken.GetPreviousToken();
            if (previousToken.IsKind(SyntaxKind.None))
                return document;

            var gapSpan = TextSpan.FromBounds(previousToken.Span.End, semicolonToken.SpanStart);
            if (gapSpan.Length == 0)
                return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var newText = text.WithChanges(new TextChange(gapSpan, string.Empty));
            return document.WithText(newText);
        }
    }
}
