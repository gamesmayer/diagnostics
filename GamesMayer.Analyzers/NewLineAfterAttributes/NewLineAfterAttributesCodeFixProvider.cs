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

namespace GamesMayer.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NewLineAfterAttributesCodeFixProvider))]
    [Shared]
    public sealed class NewLineAfterAttributesCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(NewLineAfterAttributesAnalyzer.DiagnosticId);

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
                    title: "Move declaration to a new line after attribute",
                    createChangedDocument: ct => AddNewLineAfterAttributeAsync(context.Document, position, ct),
                    equivalenceKey: nameof(NewLineAfterAttributesCodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> AddNewLineAfterAttributeAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var firstMemberToken = root.FindToken(position);
            var member = firstMemberToken.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (member == null || member.AttributeLists.Count == 0)
                return document;

            var lastAttrToken = member.AttributeLists.Last().GetLastToken();

            // Preserve indentation from the first attribute's leading whitespace.
            var firstAttrToken = member.AttributeLists.First().GetFirstToken();
            var indentation = firstAttrToken.LeadingTrivia
                .LastOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));

            // ] trailing trivia: strip whitespace, keep anything else (e.g. comments), then add a newline.
            var newLastAttrTrailing = SyntaxFactory.TriviaList(
                lastAttrToken.TrailingTrivia
                    .Where(t => !t.IsKind(SyntaxKind.WhitespaceTrivia) && !t.IsKind(SyntaxKind.EndOfLineTrivia))
                    .Concat(new[] { SyntaxFactory.EndOfLine("\n") }));

            // First member token leading trivia: just the indentation.
            var newFirstMemberLeading = indentation.IsKind(SyntaxKind.None)
                ? SyntaxTriviaList.Empty
                : SyntaxFactory.TriviaList(indentation);

            var newRoot = root.ReplaceTokens(
                new[] { lastAttrToken, firstMemberToken },
                (original, _) =>
                {
                    if (original == lastAttrToken)
                        return original.WithTrailingTrivia(newLastAttrTrailing);
                    if (original == firstMemberToken)
                        return original.WithLeadingTrivia(newFirstMemberLeading);
                    return original;
                });

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
