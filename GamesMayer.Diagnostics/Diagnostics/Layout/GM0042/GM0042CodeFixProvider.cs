using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0042CodeFixProvider))]
    [Shared]
    public sealed class GM0042CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0042Analyzer.DiagnosticId);

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
                    title: "Insert blank line after last using directive",
                    createChangedDocument: ct => InsertBlankLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0042CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> InsertBlankLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is not CompilationUnitSyntax compilationUnit)
                return document;

            if (compilationUnit.Usings.Count == 0)
                return document;

            var lastUsing = compilationUnit.Usings[compilationUnit.Usings.Count - 1];
            var lastUsingToken = lastUsing.GetLastToken();

            var existingEndOfLine = lastUsingToken.TrailingTrivia
                .FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                .ToString();
            if (string.IsNullOrEmpty(existingEndOfLine))
                existingEndOfLine = "\n";

            var preservedTrailingTrivia = lastUsingToken.TrailingTrivia
                .Where(trivia => !trivia.IsKind(SyntaxKind.WhitespaceTrivia) && !trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            var newTrailingTrivia = SyntaxFactory.TriviaList(
                preservedTrailingTrivia
                    .Concat(new[]
                    {
                        SyntaxFactory.EndOfLine(existingEndOfLine),
                        SyntaxFactory.EndOfLine(existingEndOfLine),
                    }));

            var newLastUsingToken = lastUsingToken.WithTrailingTrivia(newTrailingTrivia);
            var newRoot = root.ReplaceToken(lastUsingToken, newLastUsingToken);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
