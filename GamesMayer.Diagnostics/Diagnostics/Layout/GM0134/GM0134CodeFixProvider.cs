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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0134CodeFixProvider))]
    [Shared]
    public sealed class GM0134CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0134Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank line around ':' in inheritance clause",
                    createChangedDocument: ct => RemoveBlankLineAroundColonAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0134CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> RemoveBlankLineAroundColonAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var baseList = FindBaseList(root, diagnostic.Location.SourceSpan);
            if (baseList == null || baseList.Parent is not ClassDeclarationSyntax and not InterfaceDeclarationSyntax)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;
            var newline = DetectNewline(sourceText);

            var colonToken = baseList.ColonToken;
            var previousToken = colonToken.GetPreviousToken();
            if (previousToken == default)
            {
                return document;
            }

            var firstBaseTypeToken = baseList.Types.Count > 0 ? baseList.Types[0].GetFirstToken() : default;
            if (firstBaseTypeToken == default)
            {
                return document;
            }

            var updatedText = sourceText;
            var side = diagnostic.Properties.TryGetValue(GM0134Analyzer.SidePropertyName, out var rawSide)
                ? rawSide
                : null;
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var isBeforeGap = diagnosticSpan.End <= colonToken.SpanStart;
            var isAfterGap = diagnosticSpan.Start >= colonToken.Span.End;

            if (isBeforeGap || string.Equals(side, "before", System.StringComparison.Ordinal))
            {
                updatedText = NormalizeGap(updatedText, syntaxTree, previousToken, colonToken, newline);
            }
            else if (isAfterGap || string.Equals(side, "after", System.StringComparison.Ordinal))
            {
                updatedText = NormalizeGap(updatedText, syntaxTree, colonToken, firstBaseTypeToken, newline);
            }
            else
            {
                updatedText = NormalizeGap(updatedText, syntaxTree, previousToken, colonToken, newline);
            }

            return document.WithText(updatedText);
        }

        private static BaseListSyntax? FindBaseList(SyntaxNode root, TextSpan diagnosticSpan)
        {
            var startToken = root.FindToken(diagnosticSpan.Start);
            var byStartToken = TryGetBaseListFromToken(startToken);
            if (byStartToken != null)
            {
                return byStartToken;
            }

            var probeEnd = diagnosticSpan.End > diagnosticSpan.Start
                ? diagnosticSpan.End - 1
                : diagnosticSpan.Start;
            var endToken = root.FindToken(probeEnd);
            var byEndToken = TryGetBaseListFromToken(endToken);
            if (byEndToken != null)
            {
                return byEndToken;
            }

            return null;
        }

        private static BaseListSyntax? TryGetBaseListFromToken(SyntaxToken token)
        {
            var tokenBaseList = token.Parent?.FirstAncestorOrSelf<BaseListSyntax>();
            if (tokenBaseList != null)
            {
                return tokenBaseList;
            }

            var declaration = token.Parent?.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>();
            return declaration?.BaseList;
        }

        private static SourceText NormalizeGap(
            SourceText sourceText,
            SyntaxTree syntaxTree,
            SyntaxToken leftToken,
            SyntaxToken rightToken,
            string newline)
        {
            var leftLine = syntaxTree.GetLineSpan(leftToken.Span).EndLinePosition.Line;
            var rightLine = syntaxTree.GetLineSpan(rightToken.Span).StartLinePosition.Line;
            if (rightLine - leftLine <= 1)
            {
                return sourceText;
            }

            var replacement = " ";
            if (rightLine > leftLine)
            {
                var rightIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, rightLine);
                replacement = newline + rightIndentation;
            }

            var betweenSpan = TextSpan.FromBounds(leftToken.Span.End, rightToken.SpanStart);
            return sourceText.WithChanges(new TextChange(betweenSpan, replacement));
        }

        private static string DetectNewline(SourceText sourceText)
        {
            for (int i = 0; i < sourceText.Length - 1; i++)
            {
                if (sourceText[i] == '\r' && sourceText[i + 1] == '\n')
                {
                    return "\r\n";
                }

                if (sourceText[i] == '\n')
                {
                    return "\n";
                }
            }

            return "\n";
        }
    }
}