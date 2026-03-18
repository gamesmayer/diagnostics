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

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0040CodeFixProvider))]
    [Shared]
    public sealed class GM0040CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0040Analyzer.DiagnosticId);

        // Returning null because a single fix already normalizes the entire chain,
        // so batch-fixing multiple diagnostics in the same chain would produce
        // conflicting changes.
        public override FixAllProvider? GetFixAllProvider() => null;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move segment to its own line",
                    createChangedDocument: ct => MoveSegmentsToOwnLinesAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0040CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> MoveSegmentsToOwnLinesAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var tree = root.SyntaxTree;
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (diagnosticSpan.Start >= sourceText.Length || sourceText[diagnosticSpan.Start] != '.')
                return document;

            // Walk up from the diagnostic dot to the top of the fluent chain
            var chainTop = FindChainTop(root, diagnosticSpan.Start);
            if (chainTop == null)
                return document;

            // Collect all boundaries and filter to invocations only (same logic as analyzer)
            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(chainTop, boundaries);

            var invocationBoundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            foreach (var b in boundaries)
            {
                if (b.SegmentExpression is InvocationExpressionSyntax)
                    invocationBoundaries.Add(b);
            }

            if (invocationBoundaries.Count == 0)
                return document;

            // Compute the correct indentation from the chain root's line
            string correctIndent = ComputeCorrectIndent(sourceText, invocationBoundaries[0].LeftExpression);
            string newlineStr = DetectNewline(sourceText);

            var changes = new List<TextChange>();

            for (int i = 0; i < invocationBoundaries.Count; i++)
            {
                var (leftExpression, dotToken, _, _) = invocationBoundaries[i];

                SyntaxToken referenceToken = i == 0
                    ? leftExpression.GetLastToken()
                    : invocationBoundaries[i - 1].SegmentExpression.GetLastToken();

                var referenceLine = tree.GetLineSpan(referenceToken.Span).EndLinePosition.Line;
                var dotLineNum = tree.GetLineSpan(dotToken.Span).StartLinePosition.Line;
                bool isOnOwnLine = dotLineNum > referenceLine;

                int dotPos = dotToken.SpanStart;

                if (!isOnOwnLine)
                {
                    // Insert newline + correct indent before the dot, removing any
                    // horizontal whitespace that precedes it on the same line.
                    int removeStart = dotPos;
                    while (removeStart > 0 && (sourceText[removeStart - 1] == ' ' || sourceText[removeStart - 1] == '\t'))
                        removeStart--;
                    changes.Add(new TextChange(new TextSpan(removeStart, dotPos - removeStart), newlineStr + correctIndent));
                }
                else
                {
                    // Segment is already on its own line — fix its indentation if it differs
                    // from the correct indentation so the whole chain is consistent.
                    int lineStart = dotPos;
                    while (lineStart > 0 && sourceText[lineStart - 1] != '\n' && sourceText[lineStart - 1] != '\r')
                        lineStart--;

                    int currentIndentLength = dotPos - lineStart;
                    string currentIndent = sourceText.GetSubText(new TextSpan(lineStart, currentIndentLength)).ToString();
                    if (currentIndent != correctIndent)
                        changes.Add(new TextChange(new TextSpan(lineStart, currentIndentLength), correctIndent));
                }
            }

            if (changes.Count == 0)
                return document;

            var newSourceText = sourceText.WithChanges(changes);
            return document.WithText(newSourceText);
        }

        // Walk up from the diagnostic dot to the outermost expression of the chain.
        private static ExpressionSyntax FindChainTop(SyntaxNode root, int dotPos)
        {
            var token = root.FindToken(dotPos);
            var current = token.Parent as ExpressionSyntax;
            while (current != null)
            {
                var parent = current.Parent;
                if (parent is InvocationExpressionSyntax inv && inv.Expression == current)
                    current = inv;
                else if (parent is MemberAccessExpressionSyntax ma && ma.Expression == current)
                    current = ma;
                else
                    break;
            }
            return current;
        }

        private static void CollectFluentChainBoundaries(
            ExpressionSyntax expression,
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax invMemberAccess)
            {
                CollectFluentChainBoundaries(invMemberAccess.Expression, boundaries);
                boundaries.Add((invMemberAccess.Expression, invMemberAccess.OperatorToken, invMemberAccess.Name.GetFirstToken(), invocation));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                CollectFluentChainBoundaries(memberAccess.Expression, boundaries);
                boundaries.Add((memberAccess.Expression, memberAccess.OperatorToken, memberAccess.Name.GetFirstToken(), memberAccess));
            }
        }

        // The correct indentation for chain segments is the leading whitespace of the
        // line where the root expression starts, plus one standard indent level.
        private static string ComputeCorrectIndent(SourceText sourceText, ExpressionSyntax rootExpression)
        {
            var rootFirstToken = rootExpression.GetFirstToken();
            var rootLine = sourceText.Lines.GetLineFromPosition(rootFirstToken.SpanStart);
            var rootLineText = rootLine.ToString();

            int baseEnd = 0;
            while (baseEnd < rootLineText.Length && (rootLineText[baseEnd] == ' ' || rootLineText[baseEnd] == '\t'))
                baseEnd++;

            string baseIndent = rootLineText.Substring(0, baseEnd);
            string extraIndent = baseIndent.IndexOf('\t') >= 0 ? "\t" : "    ";
            return baseIndent + extraIndent;
        }

        private static string DetectNewline(SourceText sourceText)
        {
            for (int i = 0; i < sourceText.Length - 1; i++)
            {
                if (sourceText[i] == '\r' && sourceText[i + 1] == '\n') return "\r\n";
                if (sourceText[i] == '\n') return "\n";
            }
            return "\n";
        }
    }
}
