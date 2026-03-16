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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0034CodeFixProvider))]
    [Shared]
    public sealed class GM0034CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0034Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move first item to next line",
                    createChangedDocument: ct => FixFirstItemPlacementAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0034CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixFirstItemPlacementAsync(
            Document document,
            int tokenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var token = root.FindToken(tokenPosition);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            SyntaxToken openParenToken;
            SyntaxToken firstItemToken;

            if (token.Parent?.FirstAncestorOrSelf<ArgumentSyntax>() is { Parent: ArgumentListSyntax argumentList } argument)
            {
                openParenToken = argumentList.OpenParenToken;
                firstItemToken = argumentList.Arguments[0].GetFirstToken();
            }
            else if (token.Parent?.FirstAncestorOrSelf<ParameterSyntax>() is { Parent: ParameterListSyntax parameterList } parameter)
            {
                openParenToken = parameterList.OpenParenToken;
                firstItemToken = parameterList.Parameters[0].GetFirstToken();
            }
            else
            {
                return document;
            }

            if (firstItemToken == default)
            {
                return document;
            }

            var openParenLineNumber = syntaxTree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            var indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, openParenLineNumber, indentSize);
            var replacementSpan = TextSpan.FromBounds(openParenToken.Span.End, firstItemToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "\n" + expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
