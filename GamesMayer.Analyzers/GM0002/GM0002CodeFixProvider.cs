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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0002CodeFixProvider))]
    [Shared]
    public sealed class GM0002CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0002Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            if (node is not PropertyDeclarationSyntax property)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Convert to single-line auto-implemented property",
                    createChangedDocument: ct => CollapseToSingleLineAsync(context.Document, property, ct),
                    equivalenceKey: nameof(GM0002CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> CollapseToSingleLineAsync(
            Document document,
            PropertyDeclarationSyntax property,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var newProperty = MakeSingleLine(property);
            var newRoot = root.ReplaceNode(property, newProperty);
            return document.WithSyntaxRoot(newRoot);
        }

        private static PropertyDeclarationSyntax MakeSingleLine(PropertyDeclarationSyntax property)
        {
            var originalAccessorList = property.AccessorList!;

            var newAccessors = SyntaxFactory.List(
                originalAccessorList.Accessors.Select(MakeInlineAccessor));

            var newAccessorList = SyntaxFactory.AccessorList(
                SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                    .WithLeadingTrivia(SyntaxFactory.Space)
                    .WithTrailingTrivia(SyntaxTriviaList.Empty),
                newAccessors,
                SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                    .WithLeadingTrivia(SyntaxFactory.Space)
                    .WithTrailingTrivia(originalAccessorList.CloseBraceToken.TrailingTrivia));

            return property
                .WithIdentifier(property.Identifier.WithTrailingTrivia(SyntaxTriviaList.Empty))
                .WithAccessorList(newAccessorList);
        }

        private static AccessorDeclarationSyntax MakeInlineAccessor(AccessorDeclarationSyntax accessor)
        {
            AccessorDeclarationSyntax result;

            if (accessor.Modifiers.Count > 0)
            {
                // e.g. "private set" — put space before the first modifier, space after each modifier
                var newModifiers = SyntaxFactory.TokenList(
                    accessor.Modifiers.Select((mod, i) =>
                        mod.WithLeadingTrivia(i == 0
                                ? SyntaxFactory.TriviaList(SyntaxFactory.Space)
                                : SyntaxTriviaList.Empty)
                           .WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Space))));

                result = accessor
                    .WithModifiers(newModifiers)
                    .WithKeyword(accessor.Keyword
                        .WithLeadingTrivia(SyntaxTriviaList.Empty)
                        .WithTrailingTrivia(SyntaxTriviaList.Empty));
            }
            else
            {
                // e.g. "get" or "set" — just put a space before the keyword
                result = accessor
                    .WithKeyword(accessor.Keyword
                        .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.Space))
                        .WithTrailingTrivia(SyntaxTriviaList.Empty));
            }

            return result.WithSemicolonToken(accessor.SemicolonToken
                .WithLeadingTrivia(SyntaxTriviaList.Empty)
                .WithTrailingTrivia(SyntaxTriviaList.Empty));
        }
    }
}
