using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0121CodeFixProvider))]
    [Shared]
    public sealed class GM0121CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0121Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank line between constructor declaration and initializer",
                    createChangedDocument: ct => RemoveBlankLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0121CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> RemoveBlankLineAsync(
            Document document,
            int diagnosticPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            if (!TryFindAffectedSpan(root, diagnosticPosition, sourceText, syntaxTree, out var fromToken, out var toToken))
            {
                return document;
            }

            var toLine = sourceText.Lines.GetLineFromPosition(toToken.SpanStart);
            var toIndentLength = toToken.SpanStart - toLine.Start;
            var toIndentation = sourceText.ToString(new TextSpan(toLine.Start, toIndentLength));

            var replacementSpan = TextSpan.FromBounds(fromToken.Span.End, toToken.SpanStart);
            var replacementText = DetectNewline(sourceText) + toIndentation;
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, replacementText));

            return document.WithText(updatedText);
        }

        private static bool TryFindAffectedSpan(
            SyntaxNode root,
            int diagnosticPosition,
            SourceText sourceText,
            SyntaxTree syntaxTree,
            out SyntaxToken fromToken,
            out SyntaxToken toToken)
        {
            fromToken = default;
            toToken = default;

            var token = root.FindToken(diagnosticPosition, findInsideTrivia: true);
            var node = token.Parent;
            while (node != null)
            {
                if (node is ConstructorInitializerSyntax initializer
                    && initializer.Parent is ConstructorDeclarationSyntax constructorDeclaration)
                {
                    var closeParen = constructorDeclaration.ParameterList.CloseParenToken;
                    var colon = initializer.ColonToken;
                    var keyword = initializer.ThisOrBaseKeyword;

                    if (closeParen != default && colon != default
                        && closeParen.Span.End <= diagnosticPosition
                        && diagnosticPosition <= colon.SpanStart
                        && BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, syntaxTree, closeParen, colon, out _))
                    {
                        fromToken = closeParen;
                        toToken = colon;
                        return true;
                    }

                    if (colon != default && keyword != default
                        && colon.Span.End <= diagnosticPosition
                        && diagnosticPosition <= keyword.SpanStart
                        && BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, syntaxTree, colon, keyword, out _))
                    {
                        fromToken = colon;
                        toToken = keyword;
                        return true;
                    }
                }

                node = node.Parent;
            }

            return false;
        }

        private static string DetectNewline(SourceText sourceText)
        {
            for (int i = 0; i < sourceText.Length - 1; i++)
            {
                if (sourceText[i] == '\r' && sourceText[i + 1] == '\n')
                    return "\r\n";
                if (sourceText[i] == '\n')
                    return "\n";
            }

            return "\n";
        }
    }
}
