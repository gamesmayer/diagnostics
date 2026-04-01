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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0083CodeFixProvider))]
    [Shared]
    public sealed class GM0083CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0083Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0083Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space after control-flow keyword"
                : "Remove space after control-flow keyword";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0083CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixSpacingAsync(
            Document document,
            Diagnostic diagnostic,
            bool enabled,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!TryFindKeywordAndOpenParen(token, out var keywordToken, out var openParenToken))
                return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenSpan = TextSpan.FromBounds(keywordToken.Span.End, openParenToken.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            if (text.ToString(betweenSpan) == replacement)
                return document;

            var newText = text.Replace(betweenSpan, replacement);
            return document.WithText(newText);
        }

        private static bool TryFindKeywordAndOpenParen(
            SyntaxToken token,
            out SyntaxToken keywordToken,
            out SyntaxToken openParenToken)
        {
            foreach (var node in token.Parent?.AncestorsAndSelf() ?? System.Linq.Enumerable.Empty<SyntaxNode>())
            {
                if (!GM0083AnalyzerTryGetPair(node, out keywordToken, out openParenToken))
                    continue;

                if (keywordToken.SpanStart == token.SpanStart)
                    return true;
            }

            keywordToken = default;
            openParenToken = default;
            return false;
        }

        private static bool GM0083AnalyzerTryGetPair(
            SyntaxNode node,
            out SyntaxToken keywordToken,
            out SyntaxToken openParenToken)
        {
            switch (node)
            {
                case IfStatementSyntax ifStatement:
                    keywordToken = ifStatement.IfKeyword;
                    openParenToken = ifStatement.OpenParenToken;
                    return true;
                case ForStatementSyntax forStatement:
                    keywordToken = forStatement.ForKeyword;
                    openParenToken = forStatement.OpenParenToken;
                    return true;
                case ForEachStatementSyntax forEachStatement:
                    keywordToken = forEachStatement.ForEachKeyword;
                    openParenToken = forEachStatement.OpenParenToken;
                    return true;
                case ForEachVariableStatementSyntax forEachVariableStatement:
                    keywordToken = forEachVariableStatement.ForEachKeyword;
                    openParenToken = forEachVariableStatement.OpenParenToken;
                    return true;
                case WhileStatementSyntax whileStatement:
                    keywordToken = whileStatement.WhileKeyword;
                    openParenToken = whileStatement.OpenParenToken;
                    return true;
                case DoStatementSyntax doStatement:
                    keywordToken = doStatement.WhileKeyword;
                    openParenToken = doStatement.OpenParenToken;
                    return true;
                case SwitchStatementSyntax switchStatement:
                    keywordToken = switchStatement.SwitchKeyword;
                    openParenToken = switchStatement.OpenParenToken;
                    return true;
                case LockStatementSyntax lockStatement:
                    keywordToken = lockStatement.LockKeyword;
                    openParenToken = lockStatement.OpenParenToken;
                    return true;
                case UsingStatementSyntax usingStatement when !usingStatement.OpenParenToken.IsKind(SyntaxKind.None):
                    keywordToken = usingStatement.UsingKeyword;
                    openParenToken = usingStatement.OpenParenToken;
                    return true;
                case CatchClauseSyntax catchClause when catchClause.Declaration != null:
                    keywordToken = catchClause.CatchKeyword;
                    openParenToken = catchClause.Declaration.OpenParenToken;
                    return true;
                default:
                    keywordToken = default;
                    openParenToken = default;
                    return false;
            }
        }
    }
}
