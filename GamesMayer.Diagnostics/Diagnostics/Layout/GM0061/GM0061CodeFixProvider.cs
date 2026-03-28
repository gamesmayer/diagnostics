using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0061CodeFixProvider))]
    [Shared]
    public sealed class GM0061CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0061Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            bool isNegated = diagnostic.Properties["IsNegated"] == "true";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: isNegated ? "Use '!=' instead of 'is not'" : "Use '==' instead of 'is'",
                    createChangedDocument: ct => UseEqualityOperatorAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0061CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> UseEqualityOperatorAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            bool isBinaryIs = diagnostic.Properties["IsBinaryIs"] == "true";
            bool isNegated = diagnostic.Properties["IsNegated"] == "true";

            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

            if (isBinaryIs)
            {
                var binary = node as BinaryExpressionSyntax ?? node.Parent as BinaryExpressionSyntax;
                if (binary == null)
                    return document;

                var leftText = sourceText.ToString(binary.Left.Span);
                var rightText = sourceText.ToString(binary.Right.Span);
                var replacement = $"{leftText} == {rightText}";
                var updatedText = sourceText.WithChanges(new TextChange(binary.Span, replacement));
                return document.WithText(updatedText);
            }
            else
            {
                var isPatternExpr = node as IsPatternExpressionSyntax ?? node.Parent as IsPatternExpressionSyntax;
                if (isPatternExpr == null)
                    return document;

                var exprText = sourceText.ToString(isPatternExpr.Expression.Span);

                // x is A or B or C → x == A || x == B || x == C
                if (isPatternExpr.Pattern is BinaryPatternSyntax orPattern &&
                    orPattern.IsKind(SyntaxKind.OrPattern))
                {
                    var constants = new List<string>();
                    CollectOrPatternTexts(orPattern, sourceText, constants);
                    var replacement = string.Join(" || ", constants.Select(t => $"{exprText} == {t}"));
                    var updatedText = sourceText.WithChanges(new TextChange(isPatternExpr.Span, replacement));
                    return document.WithText(updatedText);
                }

                string op;
                string patternText;
                if (isNegated)
                {
                    var unary = isPatternExpr.Pattern as UnaryPatternSyntax;
                    if (unary == null)
                        return document;
                    op = "!=";
                    patternText = GetPatternText(unary.Pattern, sourceText);
                }
                else
                {
                    op = "==";
                    patternText = GetPatternText(isPatternExpr.Pattern, sourceText);
                }

                var replacement2 = $"{exprText} {op} {patternText}";
                var updatedText2 = sourceText.WithChanges(new TextChange(isPatternExpr.Span, replacement2));
                return document.WithText(updatedText2);
            }
        }

        private static void CollectOrPatternTexts(PatternSyntax pattern, SourceText sourceText, List<string> results)
        {
            if (pattern is BinaryPatternSyntax bp && bp.IsKind(SyntaxKind.OrPattern))
            {
                CollectOrPatternTexts(bp.Left, sourceText, results);
                CollectOrPatternTexts(bp.Right, sourceText, results);
            }
            else
            {
                results.Add(GetPatternText(pattern, sourceText));
            }
        }

        private static string GetPatternText(PatternSyntax pattern, SourceText sourceText)
        {
            if (pattern is ConstantPatternSyntax cp)
                return sourceText.ToString(cp.Expression.Span);

            if (pattern is TypePatternSyntax tp)
                return sourceText.ToString(tp.Type.Span);

            return sourceText.ToString(pattern.Span).Trim();
        }
    }
}
