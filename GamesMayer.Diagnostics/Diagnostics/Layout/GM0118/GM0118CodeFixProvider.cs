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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0118CodeFixProvider))]
    [Shared]
    public sealed class GM0118CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0118Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move comma to end of previous line",
                    createChangedDocument: ct => FixCommaAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0118CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixCommaAsync(
            Document document,
            int commaPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var comma = root.FindToken(commaPosition);
            if (!comma.IsKind(SyntaxKind.CommaToken))
                return document;

            var previousToken = comma.GetPreviousToken();
            if (previousToken == default)
                return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var spanToReplace = TextSpan.FromBounds(previousToken.Span.End, comma.Span.End);

            return document.WithText(text.WithChanges(new TextChange(spanToReplace, ",")));
        }
    }
}
