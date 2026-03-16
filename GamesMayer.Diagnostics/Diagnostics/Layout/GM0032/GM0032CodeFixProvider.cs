using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0032CodeFixProvider))]
    [Shared]
    public sealed class GM0032CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0032Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move single-line list to the declaration line",
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0032CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            int diagnosticStart,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var identifierToken = root.FindToken(diagnosticStart);
            var openParenToken = identifierToken.GetNextToken();
            while (!openParenToken.IsKind(SyntaxKind.OpenParenToken) && openParenToken != default)
            {
                openParenToken = openParenToken.GetNextToken();
            }

            if (!openParenToken.IsKind(SyntaxKind.OpenParenToken))
            {
                return document;
            }

            SyntaxToken closeParenToken;
            if (openParenToken.Parent is ArgumentListSyntax argumentList)
            {
                closeParenToken = argumentList.CloseParenToken;
            }
            else if (openParenToken.Parent is ParameterListSyntax parameterList)
            {
                closeParenToken = parameterList.CloseParenToken;
            }
            else
            {
                return document;
            }

            var previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var listSpan = TextSpan.FromBounds(openParenToken.Span.Start, closeParenToken.Span.End);
            var replacementText = sourceText.ToString(listSpan);
            var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, closeParenToken.Span.End);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, replacementText));

            return document.WithText(updatedText);
        }
    }
}