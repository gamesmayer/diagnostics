using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Text;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0017CodeFixProvider))]
    [Shared]
    public sealed class GM0017CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0017Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            new SequentialFixAllProvider(this);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Write the control structure clause declaration on a single line",
                    createChangedDocument: ct => CollapseClauseDeclarationAsync(context.Document, diagnostic.Location.SourceSpan, ct),
                    equivalenceKey: nameof(GM0017CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> CollapseClauseDeclarationAsync(
            Document document,
            TextSpan span,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            if (root == null)
                return document;

            // Only the line breaks between tokens go: string literals and the spacing on each line stay as they are.
            var changes = new List<TextChange>();
            var tokens = root.DescendantTokens(span).Where(token => span.Contains(token.Span)).ToList();

            for (var i = 1; i < tokens.Count; i++)
            {
                var previous = tokens[i - 1];
                var next = tokens[i];
                var between = TextSpan.FromBounds(previous.Span.End, next.SpanStart);
                var trivia = previous.TrailingTrivia.Concat(next.LeadingTrivia).ToList();

                if (!trivia.Any(t => t.IsKind(SyntaxKind.EndOfLineTrivia)) ||
                    trivia.Any(t => !t.IsKind(SyntaxKind.EndOfLineTrivia) && !t.IsKind(SyntaxKind.WhitespaceTrivia)))
                    continue;

                changes.Add(new TextChange(between, NeedsSpace(previous, next) ? " " : string.Empty));
            }

            return document.WithText(sourceText.WithChanges(changes));
        }

        private static bool NeedsSpace(SyntaxToken previous, SyntaxToken next)
        {
            if (previous.IsKind(SyntaxKind.OpenParenToken) ||
                previous.IsKind(SyntaxKind.DotToken) ||
                previous.IsKind(SyntaxKind.OpenBracketToken))
                return false;

            var noSpaceBefore = next.IsKind(SyntaxKind.OpenParenToken) ||
                next.IsKind(SyntaxKind.CloseParenToken) ||
                next.IsKind(SyntaxKind.DotToken) ||
                next.IsKind(SyntaxKind.CommaToken) ||
                next.IsKind(SyntaxKind.SemicolonToken) ||
                next.IsKind(SyntaxKind.OpenBracketToken) ||
                next.IsKind(SyntaxKind.CloseBracketToken) ||
                (next.IsKind(SyntaxKind.QuestionToken) && next.Parent is ConditionalAccessExpressionSyntax) ||
                (next.IsKind(SyntaxKind.ExclamationToken) && next.Parent is PostfixUnaryExpressionSyntax);

            return !noSpaceBefore;
        }
    }
}