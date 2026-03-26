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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0047CodeFixProvider))]
    [Shared]
    public sealed class GM0047CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0047Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move assignment value to same line as '='",
                    createChangedDocument: ct => FixAssignmentAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0047CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAssignmentAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var firstValueToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var equalsToken = firstValueToken.GetPreviousToken();

            if (!equalsToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsToken))
                return document;

            var change = new TextChange(TextSpan.FromBounds(equalsToken.Span.End, firstValueToken.Span.Start), " ");

            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
