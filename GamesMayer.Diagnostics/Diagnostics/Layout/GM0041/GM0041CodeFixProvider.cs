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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0041CodeFixProvider))]
    [Shared]
    public sealed class GM0041CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0041Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move all chain segments to their own lines",
                    createChangedDocument: ct => MoveChainToOwnLinesAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0041CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveChainToOwnLinesAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            if (diagnosticSpan.Start >= sourceText.Length || sourceText[diagnosticSpan.Start] != '.')
                return document;

            var dotToken = root.FindToken(diagnosticSpan.Start);
            var chainTop = FindTopMostChainExpression(dotToken.Parent as SyntaxNode);
            if (chainTop == null)
                return document;

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(chainTop, boundaries);
            if (boundaries.Count == 0)
                return document;

            int firstInvocationIndex = -1;
            for (int index = 0; index < boundaries.Count; index++)
            {
                if (boundaries[index].SegmentExpression is InvocationExpressionSyntax)
                {
                    firstInvocationIndex = index;
                    break;
                }
            }

            if (firstInvocationIndex < 0)
                return document;

            string newline = DetectNewline(sourceText);
            string indent = FindBaseIndent(sourceText, boundaries[firstInvocationIndex].DotToken.SpanStart) + "    ";

            var replacements = new List<(int Start, int Length, string Text)>();
            for (int index = firstInvocationIndex; index < boundaries.Count; index++)
            {
                var (_, dotTokenInBoundary, _) = boundaries[index];
                int dotStart = dotTokenInBoundary.SpanStart;
                int replacementStart = dotStart;

                while (replacementStart > 0 && (sourceText[replacementStart - 1] == ' ' || sourceText[replacementStart - 1] == '\t'))
                    replacementStart--;

                replacements.Add((replacementStart, dotStart - replacementStart, newline + indent));
            }

            if (replacements.Count == 0)
                return document;

            replacements.Sort((left, right) => right.Start.CompareTo(left.Start));

            var updatedText = sourceText;
            foreach (var replacement in replacements)
                updatedText = updatedText.Replace(new TextSpan(replacement.Start, replacement.Length), replacement.Text);

            return document.WithText(updatedText);
        }

        private static ExpressionSyntax? FindTopMostChainExpression(SyntaxNode? node)
        {
            SyntaxNode? current = node;
            while (current != null && current is not ExpressionSyntax)
                current = current.Parent;

            if (current is not ExpressionSyntax expression)
                return null;

            while (true)
            {
                if (expression.Parent is InvocationExpressionSyntax invocation && invocation.Expression == expression)
                {
                    expression = invocation;
                    continue;
                }

                if (expression.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression == expression)
                {
                    expression = memberAccess;
                    continue;
                }

                return expression;
            }
        }

        private static void CollectFluentChainBoundaries(
            ExpressionSyntax expression,
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax invocationMemberAccess)
            {
                CollectFluentChainBoundaries(invocationMemberAccess.Expression, boundaries);
                boundaries.Add((invocationMemberAccess.Expression, invocationMemberAccess.OperatorToken, invocation));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                CollectFluentChainBoundaries(memberAccess.Expression, boundaries);
                boundaries.Add((memberAccess.Expression, memberAccess.OperatorToken, memberAccess));
            }
        }

        private static string FindBaseIndent(SourceText sourceText, int position)
        {
            var line = sourceText.Lines.GetLineFromPosition(position);
            var lineText = line.ToString();
            int indentLength = 0;
            while (indentLength < lineText.Length && (lineText[indentLength] == ' ' || lineText[indentLength] == '\t'))
                indentLength++;

            return lineText.Substring(0, indentLength);
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
