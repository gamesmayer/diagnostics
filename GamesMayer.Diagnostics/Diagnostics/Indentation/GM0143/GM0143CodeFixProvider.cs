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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0143CodeFixProvider))]
    [Shared]
    public sealed class GM0143CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0143Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move clause to its own line",
                    createChangedDocument: ct => FixClausePositionAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0143CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixClausePositionAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var tree = root.SyntaxTree;

            string indentation = GetExpectedIndentation(token, sourceText, tree);

            // Replace whitespace between '}' and the keyword with newline + indentation
            int keywordStart = token.SpanStart;
            int wsStart = keywordStart;
            while (wsStart > 0 && (sourceText[wsStart - 1] == ' ' || sourceText[wsStart - 1] == '\t'))
                wsStart--;

            var replacementSpan = TextSpan.FromBounds(wsStart, keywordStart);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "\n" + indentation));
            return document.WithText(updatedText);
        }

        private static string GetExpectedIndentation(SyntaxToken token, SourceText sourceText, SyntaxTree tree)
        {
            int anchorLine = -1;

            if (token.Parent is ElseClauseSyntax elseClause &&
                elseClause.Parent is IfStatementSyntax ifStatement)
            {
                anchorLine = tree.GetLineSpan(ifStatement.IfKeyword.Span).StartLinePosition.Line;
            }
            else if (token.Parent is CatchClauseSyntax catchClause &&
                     catchClause.Parent is TryStatementSyntax tryForCatch)
            {
                anchorLine = tree.GetLineSpan(tryForCatch.TryKeyword.Span).StartLinePosition.Line;
            }
            else if (token.Parent is FinallyClauseSyntax finallyClause &&
                     finallyClause.Parent is TryStatementSyntax tryForFinally)
            {
                anchorLine = tree.GetLineSpan(tryForFinally.TryKeyword.Span).StartLinePosition.Line;
            }

            if (anchorLine < 0)
                return string.Empty;

            var anchorLineText = sourceText.Lines[anchorLine].ToString();
            int indentCount = 0;
            while (indentCount < anchorLineText.Length &&
                   (anchorLineText[indentCount] == ' ' || anchorLineText[indentCount] == '\t'))
                indentCount++;

            return anchorLineText.Substring(0, indentCount);
        }
    }
}
