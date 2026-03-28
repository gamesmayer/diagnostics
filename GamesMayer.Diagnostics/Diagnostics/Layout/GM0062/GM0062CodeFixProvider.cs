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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0062CodeFixProvider))]
    [Shared]
    public sealed class GM0062CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0062Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use 'is or' syntax",
                    createChangedDocument: ct => UseIsOrSyntaxAsync(context.Document, context.Diagnostics[0], ct),
                    equivalenceKey: nameof(GM0062CodeFixProvider)),
                context.Diagnostics[0]);

            return Task.CompletedTask;
        }

        private static async Task<Document> UseIsOrSyntaxAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var logicalOr = node as BinaryExpressionSyntax ?? node.Parent as BinaryExpressionSyntax;
            if (logicalOr == null || !logicalOr.IsKind(SyntaxKind.LogicalOrExpression))
                return document;

            var operands = new List<ExpressionSyntax>();
            GM0062Analyzer.CollectOrOperands(logicalOr, operands);

            string? subjectText = null;
            var typeTexts = new List<string>();

            foreach (var operand in operands)
            {
                if (operand is IsPatternExpressionSyntax isPattern &&
                    isPattern.Pattern is TypePatternSyntax typePattern)
                {
                    subjectText ??= sourceText.ToString(isPattern.Expression.Span);
                    typeTexts.Add(sourceText.ToString(typePattern.Type.Span));
                }
                else if (operand is BinaryExpressionSyntax binary &&
                         binary.IsKind(SyntaxKind.IsExpression))
                {
                    subjectText ??= sourceText.ToString(binary.Left.Span);
                    typeTexts.Add(sourceText.ToString(binary.Right.Span));
                }
                else
                {
                    return document;
                }
            }

            if (subjectText == null || typeTexts.Count < 2)
                return document;

            var replacement = $"{subjectText} is {string.Join(" or ", typeTexts)}";
            var updatedText = sourceText.WithChanges(new TextChange(logicalOr.Span, replacement));
            return document.WithText(updatedText);
        }
    }
}
