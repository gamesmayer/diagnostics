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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0072CodeFixProvider)), Shared]
    public sealed class GM0072CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0072Analyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add blank line before #if directive",
                    createChangedDocument: ct => AddBlankLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0072CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> AddBlankLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var line = sourceText.Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start);
            var change = new TextChange(new TextSpan(line.Start, 0), System.Environment.NewLine);
            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
