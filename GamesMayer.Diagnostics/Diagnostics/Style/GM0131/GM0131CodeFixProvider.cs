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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0131CodeFixProvider))]
    [Shared]
    public sealed class GM0131CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0131Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add explicit field name",
                    createChangedDocument: ct => AddExplicitNameAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0131CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> AddExplicitNameAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var expression = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) as ExpressionSyntax;
            if (expression?.Parent is not AnonymousObjectMemberDeclaratorSyntax declarator)
                return document;

            var inferredName = GM0131Analyzer.GetInferredName(expression);
            if (inferredName == null)
                return document;

            var nameIdentifier = SyntaxFactory.IdentifierName(
                SyntaxFactory.Identifier(inferredName)
                    .WithLeadingTrivia(expression.GetLeadingTrivia()));

            var equalsToken = SyntaxFactory.Token(
                SyntaxFactory.TriviaList(SyntaxFactory.Space),
                SyntaxKind.EqualsToken,
                SyntaxFactory.TriviaList(SyntaxFactory.Space));

            var nameEquals = SyntaxFactory.NameEquals(nameIdentifier, equalsToken);

            var newDeclarator = declarator
                .WithNameEquals(nameEquals)
                .WithExpression(expression.WithoutLeadingTrivia());

            var newRoot = root.ReplaceNode(declarator, newDeclarator);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
