using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0053CodeFixProvider))]
    [Shared]
    public sealed class GM0053CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0053Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var position = diagnostic.Location.SourceSpan.Start;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move 'where' clause to its own line",
                    createChangedDocument: ct => MoveWhereClauseToOwnLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0053CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveWhereClauseToOwnLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var whereToken = root.FindToken(position);
            if (!whereToken.IsKind(SyntaxKind.WhereKeyword))
                return document;

            var previousToken = whereToken.GetPreviousToken();
            var declaration = whereToken.Parent?.Parent;
            var declarationIndent = GetDeclarationIndentation(declaration);
            var whereIndent = declarationIndent + "    ";

            var newWhereToken = whereToken.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.EndOfLine("\n"),
                    SyntaxFactory.Whitespace(whereIndent)));

            var newPreviousToken = previousToken.WithTrailingTrivia(
                previousToken.TrailingTrivia.Where(t => !t.IsKind(SyntaxKind.WhitespaceTrivia)));

            var newRoot = root.ReplaceTokens(
                new[] { previousToken, whereToken },
                (original, _) =>
                {
                    if (original == previousToken) return newPreviousToken;
                    if (original == whereToken) return newWhereToken;
                    return original;
                });

            return document.WithSyntaxRoot(newRoot);
        }

        private static string GetDeclarationIndentation(SyntaxNode? declaration)
        {
            if (declaration == null)
                return string.Empty;

            var firstToken = declaration.GetFirstToken();
            var leadingTrivia = firstToken.LeadingTrivia;

            for (int i = leadingTrivia.Count - 1; i >= 0; i--)
            {
                var trivia = leadingTrivia[i];
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    return trivia.ToString();
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    break;
            }

            return string.Empty;
        }
    }
}
