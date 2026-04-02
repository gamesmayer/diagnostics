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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0033CodeFixProvider))]
    [Shared]
    public sealed class GM0033CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0033Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Align opening parenthesis indentation",
                    createChangedDocument: ct => FixOpeningParenthesisIndentationAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0033CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixOpeningParenthesisIndentationAsync(
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
            if (openParenToken.Parent is not ArgumentListSyntax && openParenToken.Parent is not ParameterListSyntax)
            {
                return document;
            }

            var previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var declarationLine = syntaxTree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
            var openParenLine = syntaxTree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var expectedIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, declarationLine);

            var openParenTextLine = sourceText.Lines[openParenLine];
            var openParenText = openParenTextLine.ToString();

            var actualIndentLength = 0;
            while (actualIndentLength < openParenText.Length && (openParenText[actualIndentLength] == ' ' || openParenText[actualIndentLength] == '\t'))
            {
                actualIndentLength++;
            }

            var indentationSpan = new TextSpan(openParenTextLine.Start, actualIndentLength);
            var updatedText = sourceText.WithChanges(new TextChange(indentationSpan, expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}