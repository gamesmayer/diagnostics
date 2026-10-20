using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0081CodeFixProvider))]
    [Shared]
    public sealed class GM0081CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0081Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            new SequentialFixAllProvider(this);

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

            if (node.FirstAncestorOrSelf<SwitchExpressionArmSyntax>() is { } arm)
                return await FixArmIndentationAsync(document, root, arm, cancellationToken).ConfigureAwait(false);

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

        // Moves every line of the arm by the same amount, so a multi-line arm keeps its own layout. Lines that
        // start inside a multi-line string literal are part of the string and stay as they are.
        private static async Task<Document> FixArmIndentationAsync(
            Document document,
            SyntaxNode root,
            SwitchExpressionArmSyntax arm,
            CancellationToken cancellationToken)
        {
            if (arm.Parent is not SwitchExpressionSyntax switchExpr)
                return document;

            var tree = root.SyntaxTree;
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var openBraceCol = tree.GetLineSpan(switchExpr.OpenBraceToken.Span).StartLinePosition.Character;
            var expectedCol = openBraceCol + GM0081Analyzer.DetectIndentStepForExpression(switchExpr, tree);
            var firstLine = tree.GetLineSpan(arm.GetFirstToken().Span).StartLinePosition.Line;
            var lastLine = tree.GetLineSpan(arm.GetLastToken().Span).EndLinePosition.Line;
            var delta = expectedCol - tree.GetLineSpan(arm.GetFirstToken().Span).StartLinePosition.Character;

            if (delta == 0)
                return document;

            var changes = new List<TextChange>();

            for (var line = firstLine; line <= lastLine; line++)
            {
                var textLine = sourceText.Lines[line];
                var text = textLine.ToString();
                var indent = 0;

                while (indent < text.Length && text[indent] == ' ')
                    indent++;

                if (indent == text.Length)
                    continue;

                var token = root.FindToken(textLine.Start + indent);
                if (line != firstLine && token.SpanStart < textLine.Start + indent)
                    continue;

                var newIndent = System.Math.Max(0, indent + delta);
                changes.Add(new TextChange(new TextSpan(textLine.Start, indent), new string(' ', newIndent)));
            }

            return document.WithText(sourceText.WithChanges(changes));
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
