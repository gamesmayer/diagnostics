using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0045CodeFixProvider))]
    [Shared]
    public sealed class GM0045CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0045Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix expression body indentation",
                    createChangedDocument: ct => FixIndentAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0045CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!TryFindArrowAndBody(token.Parent, out var arrowToken, out var body) || body == null)
                return document;

            var tree = root.SyntaxTree;
            var arrowLine = tree.GetLineSpan(arrowToken.Span).EndLinePosition.Line;
            int arrowIndent = GM0045Analyzer.CountLeadingWhitespace(sourceText.Lines[arrowLine].ToString());
            int expectedIndent = arrowIndent + 4;

            var bodyFirstToken = body.GetFirstToken();
            var bodyTextLine = sourceText.Lines.GetLineFromPosition(bodyFirstToken.SpanStart);
            int actualIndent = GM0045Analyzer.CountLeadingWhitespace(bodyTextLine.ToString());

            var indentSpan = new TextSpan(bodyTextLine.Start, actualIndent);
            var updatedText = sourceText.WithChanges(new TextChange(indentSpan, new string(' ', expectedIndent)));
            return document.WithText(updatedText);
        }

        private static bool TryFindArrowAndBody(SyntaxNode? node, out SyntaxToken arrowToken, out SyntaxNode? body)
        {
            while (node != null)
            {
                if (node is LambdaExpressionSyntax lambda && !(lambda.Body is BlockSyntax))
                {
                    arrowToken = lambda.ArrowToken;
                    body = lambda.Body;
                    return true;
                }
                if (node is ArrowExpressionClauseSyntax arrowClause)
                {
                    arrowToken = arrowClause.ArrowToken;
                    body = arrowClause.Expression;
                    return true;
                }
                node = node.Parent;
            }

            arrowToken = default;
            body = null;
            return false;
        }
    }
}
