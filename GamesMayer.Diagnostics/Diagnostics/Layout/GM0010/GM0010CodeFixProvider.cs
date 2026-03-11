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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0010CodeFixProvider))]
    [Shared]
    public sealed class GM0010CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0010Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var position = diagnostic.Location.SourceSpan.Start;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove consecutive blank line",
                    createChangedDocument: ct => RemoveBlankLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0010CodeFixProvider)),
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

            // Remove the line break that ends the previous line — this is what creates the
            // current blank line. Using prevLine's break (rather than the current line's
            // SpanIncludingLineBreak) ensures non-overlapping spans across all diagnostics,
            // including consecutive blanks at EOF where the last line has no line break of its own.
            var prevLine = sourceText.Lines[line.LineNumber - 1];
            var spanToRemove = TextSpan.FromBounds(prevLine.Span.End, prevLine.SpanIncludingLineBreak.End);

            return document.WithText(sourceText.WithChanges(new TextChange(spanToRemove, string.Empty)));
        }
    }
}
