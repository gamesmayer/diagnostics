using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0123CodeFixProvider))]
    [Shared]
    public sealed class GM0123CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0123Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Place member on its own line",
                    createChangedDocument: ct => FixMemberLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0123CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixMemberLineAsync(
            Document document,
            int tokenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(tokenPosition);

            SyntaxNode? member;
            SyntaxToken openBrace;
            SyntaxToken newKeyword;
            int itemIndex;
            int changeStart;

            var expression = token.Parent?
                .AncestorsAndSelf()
                .OfType<ExpressionSyntax>()
                .FirstOrDefault(e => e.Parent is InitializerExpressionSyntax);

            if (expression?.Parent is InitializerExpressionSyntax initializer)
            {
                member = expression;
                openBrace = initializer.OpenBraceToken;
                newKeyword = GM0078Analyzer.FindNewKeyword(initializer);
                var expressions = initializer.Expressions;
                itemIndex = expressions.IndexOf(expression);
                if (itemIndex < 0)
                    return document;
                changeStart = itemIndex == 0
                    ? openBrace.Span.End
                    : expressions.GetSeparator(itemIndex - 1).Span.End;
            }
            else
            {
                var memberDeclarator = token.Parent?
                    .AncestorsAndSelf()
                    .OfType<AnonymousObjectMemberDeclaratorSyntax>()
                    .FirstOrDefault();
                if (memberDeclarator?.Parent is not AnonymousObjectCreationExpressionSyntax anonymousCreation)
                    return document;

                member = memberDeclarator;
                openBrace = anonymousCreation.OpenBraceToken;
                newKeyword = GM0078Analyzer.FindNewKeyword(memberDeclarator);
                var initializers = anonymousCreation.Initializers;
                itemIndex = initializers.IndexOf(memberDeclarator);
                if (itemIndex < 0)
                    return document;
                changeStart = itemIndex == 0
                    ? openBrace.Span.End
                    : initializers.GetSeparator(itemIndex - 1).Span.End;
            }

            if (newKeyword == default)
                return document;

            var tree = root.SyntaxTree;
            var newKeywordLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
            int declarationIndent = GM0078Analyzer.CountLeadingWhitespace(sourceText.Lines[newKeywordLine].ToString());

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);
            int indentStep = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentStep = parsedSize;
            }

            var expectedIndentation = new string(' ', declarationIndent + indentStep);
            var currentFirstToken = member.GetFirstToken();
            var changeSpan = TextSpan.FromBounds(changeStart, currentFirstToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(changeSpan, "\n" + expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
