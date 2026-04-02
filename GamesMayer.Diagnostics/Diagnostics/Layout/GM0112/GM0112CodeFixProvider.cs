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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0112CodeFixProvider))]
    [Shared]
    public sealed class GM0112CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0112Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var position = diagnostic.Location.SourceSpan.Start;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank line between consecutive variables",
                    createChangedDocument: ct => RemoveBlankLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0112CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> RemoveBlankLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var line = sourceText.Lines.GetLineFromPosition(position);

            if (line.LineNumber == 0)
            {
                var firstLineSpan = line.SpanIncludingLineBreak;
                return document.WithText(sourceText.WithChanges(new TextChange(firstLineSpan, string.Empty)));
            }

            var previousLine = sourceText.Lines[line.LineNumber - 1];
            var spanToRemove = TextSpan.FromBounds(previousLine.Span.End, previousLine.SpanIncludingLineBreak.End);

            return document.WithText(sourceText.WithChanges(new TextChange(spanToRemove, string.Empty)));
        }
    }
}