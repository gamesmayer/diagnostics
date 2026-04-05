using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using GamesMayer.Diagnostics.Utils;
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
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0041.threshold";
        private const int DefaultMinInvocations = 2;

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0041Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var title = ShouldEnforceOwnLine(context.Document, diagnostic)
                ? "Move all chain segments to their own lines"
                : "Move all chain segments to the same line";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => ApplyFixAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0041CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static bool ShouldEnforceOwnLine(Document document, Diagnostic diagnostic)
        {
            var syntaxTree = diagnostic.Location.SourceTree;
            if (syntaxTree == null)
                return true;

            var root = syntaxTree.GetRoot();
            var dotToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var chainTop = FindTopMostChainExpression(dotToken.Parent as SyntaxNode);
            if (chainTop == null)
                return true;

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)>();
            FluentChainUtils.CollectFluentChainBoundaries(chainTop, boundaries);

            int invocationCount = 0;
            foreach (var boundary in boundaries)
            {
                if (boundary.SegmentExpression is InvocationExpressionSyntax)
                    invocationCount++;
            }

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            var threshold = DefaultMinInvocations;
            if (analyzerOptions.TryGetValue(ThresholdOptionKey, out var thresholdValue)
                && int.TryParse(thresholdValue, out var parsedThreshold)
                && parsedThreshold > 1)
            {
                threshold = parsedThreshold;
            }

            return invocationCount >= threshold;
        }

        private static Task<Document> ApplyFixAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            return ShouldEnforceOwnLine(document, diagnostic)
                ? MoveChainToOwnLinesAsync(document, diagnostic, cancellationToken)
                : CollapseChainToSameLineAsync(document, diagnostic, cancellationToken);
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
            FluentChainUtils.CollectFluentChainBoundaries(chainTop, boundaries);
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

        private static async Task<Document> CollapseChainToSameLineAsync(
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
            FluentChainUtils.CollectFluentChainBoundaries(chainTop, boundaries);
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

            var replacements = new List<(int Start, int Length, string Text)>();
            for (int index = firstInvocationIndex; index < boundaries.Count; index++)
            {
                var (_, dotTokenInBoundary, _) = boundaries[index];
                int dotStart = dotTokenInBoundary.SpanStart;

                // Only remove the newline when the dot is on its own line (only whitespace before it).
                var dotLine = sourceText.Lines.GetLineFromPosition(dotStart);
                bool isOnOwnLine = true;
                for (int pos = dotLine.Start; pos < dotStart; pos++)
                {
                    if (sourceText[pos] != ' ' && sourceText[pos] != '\t')
                    {
                        isOnOwnLine = false;
                        break;
                    }
                }

                if (!isOnOwnLine)
                    continue;

                int replacementStart = dotStart;
                while (replacementStart > 0 && (sourceText[replacementStart - 1] == ' ' || sourceText[replacementStart - 1] == '\t'))
                    replacementStart--;

                if (replacementStart > 0 && sourceText[replacementStart - 1] == '\n')
                {
                    replacementStart--;
                    if (replacementStart > 0 && sourceText[replacementStart - 1] == '\r')
                        replacementStart--;
                }

                if (replacementStart < dotStart)
                    replacements.Add((replacementStart, dotStart - replacementStart, ""));
            }

            if (replacements.Count == 0)
                return document;

            replacements.Sort((left, right) => right.Start.CompareTo(left.Start));

            var updatedText = sourceText;
            foreach (var replacement in replacements)
                updatedText = updatedText.Replace(new TextSpan(replacement.Start, replacement.Length), replacement.Text);

            return document.WithText(updatedText);
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
