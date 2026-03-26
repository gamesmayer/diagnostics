using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0043CodeFixProvider))]
    [Shared]
    public sealed class GM0043CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0043Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move all operands to their own lines",
                    createChangedDocument: ct => MoveOperandsToOwnLinesAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0043CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveOperandsToOwnLinesAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var diagnosticToken = root.FindToken(diagnosticSpan.Start);
            var topMost = FindTopMostSameOperatorExpression(diagnosticToken.Parent as SyntaxNode);
            if (topMost == null)
                return document;

            var operands = new List<(ExpressionSyntax Operand, SyntaxToken? OperatorBefore)>();
            CollectOperands(topMost, topMost.Kind(), operands);
            if (operands.Count == 0)
                return document;

            string newline = DetectNewline(sourceText);
            string indent = FindBaseIndent(sourceText, topMost.SpanStart) + "    ";

            var replacements = new List<(int Start, int Length, string Text)>();
            for (int i = 1; i < operands.Count; i++)
            {
                var operatorToken = operands[i].OperatorBefore;
                if (operatorToken == null)
                    continue;

                int opEnd = operatorToken.Value.Span.End;
                int whitespaceEnd = opEnd;

                while (whitespaceEnd < sourceText.Length && (sourceText[whitespaceEnd] == ' ' || sourceText[whitespaceEnd] == '\t'))
                    whitespaceEnd++;

                replacements.Add((opEnd, whitespaceEnd - opEnd, newline + indent));
            }

            if (replacements.Count == 0)
                return document;

            replacements.Sort((left, right) => right.Start.CompareTo(left.Start));

            var updatedText = sourceText;
            foreach (var replacement in replacements)
                updatedText = updatedText.Replace(new TextSpan(replacement.Start, replacement.Length), replacement.Text);

            return document.WithText(updatedText);
        }

        private static BinaryExpressionSyntax? FindTopMostSameOperatorExpression(SyntaxNode? node)
        {
            SyntaxNode? current = node;
            while (current != null)
            {
                if (current is BinaryExpressionSyntax candidate
                    && (candidate.Kind() == SyntaxKind.LogicalOrExpression
                        || candidate.Kind() == SyntaxKind.LogicalAndExpression))
                    break;

                current = current.Parent;
            }

            if (current is not BinaryExpressionSyntax binary)
                return null;

            var kind = binary.Kind();
            while (binary.Parent is BinaryExpressionSyntax parent && parent.Kind() == kind)
                binary = parent;

            return binary;
        }

        private static void CollectOperands(
            ExpressionSyntax expression,
            SyntaxKind operatorKind,
            List<(ExpressionSyntax Operand, SyntaxToken? OperatorBefore)> operands)
        {
            if (expression is BinaryExpressionSyntax binary && binary.Kind() == operatorKind)
            {
                CollectOperands(binary.Left, operatorKind, operands);
                operands.Add((binary.Right, binary.OperatorToken));
            }
            else
            {
                operands.Add((expression, null));
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
