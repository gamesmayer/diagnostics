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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0082CodeFixProvider))]
    [Shared]
    public sealed class GM0082CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0082Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0082Analyzer.EnabledProperty, out var val)
                && bool.TryParse(val, out var b) && b;

            var title = enabled ? "Add space after cast" : "Remove space after cast";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0082CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixSpacingAsync(
            Document document,
            Diagnostic diagnostic,
            bool enabled,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var closeParenToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var castExpr = closeParenToken.Parent as CastExpressionSyntax;
            if (castExpr == null)
                return document;

            var expressionFirstToken = castExpr.Expression.GetFirstToken();
            SyntaxNode newRoot;

            if (enabled)
            {
                var newToken = expressionFirstToken.WithLeadingTrivia(
                    expressionFirstToken.LeadingTrivia.Insert(0, SyntaxFactory.Space));
                newRoot = root.ReplaceToken(expressionFirstToken, newToken);
            }
            else
            {
                if (closeParenToken.TrailingTrivia.Any(t => t.IsKind(SyntaxKind.WhitespaceTrivia)))
                {
                    var newCloseParen = closeParenToken.WithTrailingTrivia(
                        SyntaxFactory.TriviaList(closeParenToken.TrailingTrivia.Where(t => !t.IsKind(SyntaxKind.WhitespaceTrivia))));
                    newRoot = root.ReplaceToken(closeParenToken, newCloseParen);
                }
                else
                {
                    var newToken = expressionFirstToken.WithLeadingTrivia(
                        SyntaxFactory.TriviaList(expressionFirstToken.LeadingTrivia.Where(t => !t.IsKind(SyntaxKind.WhitespaceTrivia))));
                    newRoot = root.ReplaceToken(expressionFirstToken, newToken);
                }
            }

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
