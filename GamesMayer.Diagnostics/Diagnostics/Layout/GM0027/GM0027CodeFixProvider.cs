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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0027CodeFixProvider))]
    [Shared]
    public sealed class GM0027CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0027Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add line break after opening parenthesis",
                    createChangedDocument: ct => FixOpeningParenthesisAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0027CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixOpeningParenthesisAsync(
            Document document,
            int openParenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var openParenToken = root.FindToken(openParenPosition);
            if (!openParenToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OpenParenToken))
            {
                return document;
            }

            SyntaxToken previousToken;
            SyntaxToken firstItemToken;

            if (openParenToken.Parent is ArgumentListSyntax argumentList && argumentList.Arguments.Count > 0)
            {
                previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
                firstItemToken = argumentList.Arguments[0].GetFirstToken();
            }
            else if (openParenToken.Parent is ParameterListSyntax parameterList && parameterList.Parameters.Count > 0)
            {
                previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
                firstItemToken = parameterList.Parameters[0].GetFirstToken();
            }
            else
            {
                return document;
            }

            if (previousToken == default || firstItemToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;
            var previousTokenLine = syntaxTree.GetLineSpan(previousToken.Span).EndLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, previousTokenLine, indentSize);
            var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, firstItemToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "(\n" + expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}