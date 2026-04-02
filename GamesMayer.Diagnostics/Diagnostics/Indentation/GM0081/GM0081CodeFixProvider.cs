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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0081CodeFixProvider))]
    [Shared]
    public sealed class GM0081CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0081Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix case label indentation",
                    createChangedDocument: ct => FixIndentationAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0081CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentationAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var label = node as SwitchLabelSyntax ?? node.Parent as SwitchLabelSyntax;
            if (label == null)
                return document;

            var switchStatement = label.FirstAncestorOrSelf<SwitchStatementSyntax>();
            if (switchStatement == null)
                return document;

            var tree = root.SyntaxTree;
            var switchCol = tree.GetLineSpan(switchStatement.SwitchKeyword.Span).StartLinePosition.Character;
            var indentStep = GM0081Analyzer.DetectIndentStep(switchStatement, tree, switchCol);

            var switchIndentation = GetLineIndentation(switchStatement.SwitchKeyword);
            var usesTabs = switchIndentation.Length > 0 && switchIndentation[0] == '\t';
            var oneStep = usesTabs ? "\t" : new string(' ', indentStep);
            var expectedIndentation = switchIndentation + oneStep;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var labelToken = label.GetFirstToken();
            var leadingTriviaSpan = TextSpan.FromBounds(labelToken.FullSpan.Start, labelToken.SpanStart);
            var leadingTriviaText = sourceText.GetSubText(leadingTriviaSpan).ToString();
            var lastNewline = leadingTriviaText.LastIndexOfAny(new[] { '\n', '\r' });
            var indentStart = lastNewline >= 0
                ? labelToken.FullSpan.Start + lastNewline + 1
                : labelToken.FullSpan.Start;
            var indentSpan = TextSpan.FromBounds(indentStart, labelToken.SpanStart);

            var change = new TextChange(indentSpan, expectedIndentation);
            return document.WithText(sourceText.WithChanges(change));
        }

        private static string GetLineIndentation(SyntaxToken token)
        {
            var triviaList = token.LeadingTrivia;

            for (var i = triviaList.Count - 1; i >= 0; i--)
            {
                var trivia = triviaList[i];

                if (trivia.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.WhitespaceTrivia)
                    return trivia.ToString();

                if (trivia.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.EndOfLineTrivia)
                    return string.Empty;
            }

            return string.Empty;
        }
    }
}
