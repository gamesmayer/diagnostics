using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0030CodeFixProvider))]
    [Shared]
    public sealed class GM0030CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0030Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank line before opening parenthesis",
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0030CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixSpacingAsync(
            Document document,
            int diagnosticPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var tokenAtDiagnostic = root.FindToken(diagnosticPosition, findInsideTrivia: true);
            var openParenToken = tokenAtDiagnostic;
            if (!openParenToken.IsKind(SyntaxKind.OpenParenToken))
            {
                openParenToken = tokenAtDiagnostic.GetNextToken(includeZeroWidth: true);
            }

            if (!openParenToken.IsKind(SyntaxKind.OpenParenToken))
            {
                return document;
            }

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

            if (openParenLine <= declarationLine + 1)
            {
                return document;
            }

            var openParenIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, openParenLine);
            var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, openParenToken.Span.End);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "\n" + openParenIndentation + "("));

            return document.WithText(updatedText);
        }
    }
}
