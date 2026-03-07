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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0007CodeFixProvider))]
    [Shared]
    public sealed class GM0007CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0007Analyzer.DiagnosticId);

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
                    title: "Insert blank line between class members",
                    createChangedDocument: ct => InsertBlankLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0007CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> InsertBlankLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var token = root.FindToken(position);
            var currentMember = token.Parent?.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault();
            if (currentMember == null)
                return document;

            if (currentMember.Parent is not ClassDeclarationSyntax classDeclaration)
                return document;

            var memberIndex = classDeclaration.Members.IndexOf(currentMember);
            if (memberIndex <= 0)
                return document;

            var previousMember = classDeclaration.Members[memberIndex - 1];
            var previousLastToken = previousMember.GetLastToken();

            var existingEndOfLine = previousLastToken.TrailingTrivia
                .FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                .ToString();
            if (string.IsNullOrEmpty(existingEndOfLine))
                existingEndOfLine = "\n";

            var preservedTrailingTrivia = previousLastToken.TrailingTrivia
                .Where(trivia => !trivia.IsKind(SyntaxKind.WhitespaceTrivia) && !trivia.IsKind(SyntaxKind.EndOfLineTrivia));

            var newTrailingTrivia = SyntaxFactory.TriviaList(
                preservedTrailingTrivia
                    .Concat(new[]
                    {
                        SyntaxFactory.EndOfLine(existingEndOfLine),
                        SyntaxFactory.EndOfLine(existingEndOfLine),
                    }));

            var newPreviousLastToken = previousLastToken.WithTrailingTrivia(newTrailingTrivia);
            var newRoot = root.ReplaceToken(previousLastToken, newPreviousLastToken);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
