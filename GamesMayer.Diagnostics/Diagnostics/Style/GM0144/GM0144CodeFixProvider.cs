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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0144CodeFixProvider))]
    [Shared]
    public sealed class GM0144CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0144Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var conditional = node as ConditionalExpressionSyntax
                ?? node.FirstAncestorOrSelf<ConditionalExpressionSyntax>();

            if (conditional == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use null-conditional operator '?.'",
                    createChangedDocument: ct => ApplyFixAsync(context.Document, conditional, ct),
                    equivalenceKey: nameof(GM0144CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> ApplyFixAsync(
            Document document,
            ConditionalExpressionSyntax conditional,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            if (!GM0144Analyzer.TryGetNullConditionalPattern(conditional, out var checkedExpr, out var memberAccessExpr))
                return document;

            var nullConditional = BuildNullConditional(checkedExpr, memberAccessExpr)
                .WithTriviaFrom(conditional);

            var newRoot = root.ReplaceNode(conditional, nullConditional);
            return document.WithSyntaxRoot(newRoot);
        }

        private static ExpressionSyntax BuildNullConditional(ExpressionSyntax target, ExpressionSyntax memberAccess)
        {
            var cleanTarget = target.WithoutTrivia();

            return memberAccess switch
            {
                MemberAccessExpressionSyntax ma =>
                    SyntaxFactory.ConditionalAccessExpression(
                        cleanTarget,
                        SyntaxFactory.MemberBindingExpression(ma.Name)),

                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax ima } invocation =>
                    SyntaxFactory.ConditionalAccessExpression(
                        cleanTarget,
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberBindingExpression(ima.Name),
                            invocation.ArgumentList)),

                ElementAccessExpressionSyntax ea =>
                    SyntaxFactory.ConditionalAccessExpression(
                        cleanTarget,
                        SyntaxFactory.ElementBindingExpression(ea.ArgumentList)),

                _ => memberAccess
            };
        }
    }
}
