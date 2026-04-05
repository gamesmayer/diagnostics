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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0138CodeFixProvider))]
    [Shared]
    public sealed class GM0138CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0138Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move '=>' to the same line as the lambda parameter list",
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0138CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var lambda = node as LambdaExpressionSyntax
                ?? node?.AncestorsAndSelf().OfType<LambdaExpressionSyntax>().FirstOrDefault();
            if (lambda == null)
                return document;

            var parametersLastTokenEnd = lambda switch
            {
                ParenthesizedLambdaExpressionSyntax p => p.ParameterList.CloseParenToken.Span.End,
                SimpleLambdaExpressionSyntax s => s.Parameter.GetLastToken().Span.End,
                _ => -1
            };

            if (parametersLastTokenEnd < 0)
                return document;

            var arrowStart = lambda.ArrowToken.SpanStart;
            var triviaLength = arrowStart - parametersLastTokenEnd;

            if (triviaLength <= 0)
                return document;

            var updatedText = sourceText.Replace(new TextSpan(parametersLastTokenEnd, triviaLength), " ");
            return document.WithText(updatedText);
        }
    }
}
