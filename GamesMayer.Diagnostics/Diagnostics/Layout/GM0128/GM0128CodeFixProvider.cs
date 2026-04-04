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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0128CodeFixProvider))]
    [Shared]
    public sealed class GM0128CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0128Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move type to the same line as 'new'",
                    createChangedDocument: ct => FixObjectCreationAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0128CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixObjectCreationAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var typeFirstToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var newKeyword = typeFirstToken.GetPreviousToken();

            var change = new TextChange(TextSpan.FromBounds(newKeyword.Span.End, typeFirstToken.Span.Start), " ");

            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
