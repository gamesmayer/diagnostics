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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0028CodeFixProvider))]
    [Shared]
    public sealed class GM0028CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0028Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Align closing parenthesis indentation",
                    createChangedDocument: ct => FixClosingParenthesisIndentationAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0028CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixClosingParenthesisIndentationAsync(
            Document document,
            int closeParenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var closeParenToken = root.FindToken(closeParenPosition);
            if (closeParenToken.Parent is not ArgumentListSyntax && closeParenToken.Parent is not ParameterListSyntax)
            {
                return document;
            }

            SyntaxToken openParenToken;
            if (closeParenToken.Parent is ArgumentListSyntax argumentList)
            {
                openParenToken = argumentList.OpenParenToken;
            }
            else if (closeParenToken.Parent is ParameterListSyntax parameterList)
            {
                openParenToken = parameterList.OpenParenToken;
            }
            else
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var openParenLine = syntaxTree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = syntaxTree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;

            var expectedIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, openParenLine);
            var closeParenTextLine = sourceText.Lines[closeParenLine];
            var closeParenText = closeParenTextLine.ToString();

            var actualIndentLength = 0;
            while (actualIndentLength < closeParenText.Length && (closeParenText[actualIndentLength] == ' ' || closeParenText[actualIndentLength] == '\t'))
            {
                actualIndentLength++;
            }

            var indentationSpan = new TextSpan(closeParenTextLine.Start, actualIndentLength);
            var updatedText = sourceText.WithChanges(new TextChange(indentationSpan, expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
