using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0001CodeFixProvider))]
    [Shared]
    public sealed class GM0001CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0001Analyzer.DiagnosticId);

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
                    title: "Remove blank line between using directives",
                    createChangedDocument: ct => RemoveBlankLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0001CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> RemoveBlankLineAsync(
            Document document,
            int blankLinePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is not CompilationUnitSyntax compilationUnit)
                return document;

            for (int i = 1; i < compilationUnit.Usings.Count; i++)
            {
                var usingDirective = compilationUnit.Usings[i];
                var leadingTrivia = usingDirective.GetLeadingTrivia();

                for (int j = 0; j < leadingTrivia.Count; j++)
                {
                    var trivia = leadingTrivia[j];
                    if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.SpanStart == blankLinePosition)
                    {
                        var newUsingDirective = usingDirective.WithLeadingTrivia(leadingTrivia.RemoveAt(j));
                        var newRoot = root.ReplaceNode(usingDirective, newUsingDirective);
                        return document.WithSyntaxRoot(newRoot);
                    }
                }
            }

            return document;
        }
    }
}
