using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace GamesMayer.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0003CodeFixProvider))]
    [Shared]
    public sealed class GM0003CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0003Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var position = diagnostic.Location.SourceSpan.Start;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank lines between attribute and member",
                    createChangedDocument: ct => RemoveBlankLinesAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0003CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> RemoveBlankLinesAsync(
            Document document,
            int blankLinePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var token = root.FindToken(blankLinePosition);
            var leadingTrivia = token.LeadingTrivia;

            for (int i = 0; i < leadingTrivia.Count; i++)
            {
                var trivia = leadingTrivia[i];
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.SpanStart == blankLinePosition)
                {
                    var newTrivia = leadingTrivia;
                    int k = i;
                    while (k < newTrivia.Count && newTrivia[k].IsKind(SyntaxKind.EndOfLineTrivia))
                        newTrivia = newTrivia.RemoveAt(k);

                    var newToken = token.WithLeadingTrivia(newTrivia);
                    var newRoot = root.ReplaceToken(token, newToken);
                    return document.WithSyntaxRoot(newRoot);
                }
            }

            return document;
        }
    }
}
