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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0021CodeFixProvider))]
    [Shared]
    public sealed class GM0021CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0021Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move opening brace to declaration line",
                    createChangedDocument: ct => MoveOpeningBraceToDeclarationLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0021CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveOpeningBraceToDeclarationLineAsync(
            Document document,
            int bracePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var openBrace = root.FindToken(bracePosition);
            if (!openBrace.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OpenBraceToken))
            {
                return document;
            }

            var previousToken = openBrace.GetPreviousToken(includeZeroWidth: false);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenTokensSpan = TextSpan.FromBounds(previousToken.Span.End, openBrace.Span.Start);
            var updatedText = sourceText.WithChanges(new TextChange(betweenTokensSpan, " "));

            return document.WithText(updatedText);
        }
    }
}
