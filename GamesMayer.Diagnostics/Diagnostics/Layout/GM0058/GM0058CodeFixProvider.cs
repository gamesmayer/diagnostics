using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0058CodeFixProvider))]
    [Shared]
    public sealed class GM0058CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0058Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move ':' to the same line as the declaration",
                    createChangedDocument: ct => FixAsync(context.Document, context.Diagnostics[0], ct),
                    equivalenceKey: nameof(GM0058CodeFixProvider)),
                context.Diagnostics[0]);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var colonToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var previousToken = colonToken.GetPreviousToken();
            if (previousToken == default)
                return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenSpan = TextSpan.FromBounds(previousToken.Span.End, colonToken.SpanStart);

            return document.WithText(text.WithChanges(new TextChange(betweenSpan, " ")));
        }
    }
}
