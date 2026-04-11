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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0142CodeFixProvider))]
    [Shared]
    public sealed class GM0142CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0142Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var invocation = node as InvocationExpressionSyntax
                ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            if (invocation == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with 'new T[0]'",
                    createChangedDocument: ct => ReplaceWithArrayCreationAsync(context.Document, invocation, ct),
                    equivalenceKey: nameof(GM0142CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> ReplaceWithArrayCreationAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return document;

            if (memberAccess.Name is not GenericNameSyntax genericName)
                return document;

            var typeArgument = genericName.TypeArgumentList.Arguments[0];

            var arrayCreation = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(
                    typeArgument.WithoutTrivia(),
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(0)))))))
                .WithTriviaFrom(invocation);

            var newRoot = root.ReplaceNode(invocation, arrayCreation);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
