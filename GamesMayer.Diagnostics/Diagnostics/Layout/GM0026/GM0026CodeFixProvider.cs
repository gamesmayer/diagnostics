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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0026CodeFixProvider))]
    [Shared]
    public sealed class GM0026CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0026Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move closing parenthesis to a new line",
                    createChangedDocument: ct => FixClosingParenthesisAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0026CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixClosingParenthesisAsync(
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
            if (!closeParenToken.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CloseParenToken))
            {
                return document;
            }

            SyntaxToken openParenToken;
            SyntaxToken previousToken;

            if (closeParenToken.Parent is ArgumentListSyntax argumentList && argumentList.Arguments.Count > 0)
            {
                openParenToken = argumentList.OpenParenToken;
                previousToken = argumentList.Arguments[argumentList.Arguments.Count - 1].GetLastToken();
            }
            else if (closeParenToken.Parent is ParameterListSyntax parameterList && parameterList.Parameters.Count > 0)
            {
                openParenToken = parameterList.OpenParenToken;
                previousToken = parameterList.Parameters[parameterList.Parameters.Count - 1].GetLastToken();
            }
            else
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;
            var openParenLine = syntaxTree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, openParenLine);

            var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, closeParenToken.Span.Start);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "\n" + closeParenIndentation));

            return document.WithText(updatedText);
        }
    }
}