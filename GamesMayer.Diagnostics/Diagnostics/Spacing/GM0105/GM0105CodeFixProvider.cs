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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0105CodeFixProvider))]
    [Shared]
    public sealed class GM0105CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0105Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0105Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces around ternary operators '?' and ':'"
                : "Remove spaces around ternary operators '?' and ':'";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0105CodeFixProvider)),
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

            var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!operatorToken.IsKind(SyntaxKind.QuestionToken)
                && !operatorToken.IsKind(SyntaxKind.ColonToken))
                return document;

            if (!(operatorToken.Parent is ConditionalExpressionSyntax conditionalExpression))
                return document;

            var questionToken = conditionalExpression.QuestionToken;
            var conditionLastToken = conditionalExpression.Condition.GetLastToken();
            var whenTrueFirstToken = conditionalExpression.WhenTrue.GetFirstToken();
            var whenTrueLastToken = conditionalExpression.WhenTrue.GetLastToken();
            var colonToken = conditionalExpression.ColonToken;
            var whenFalseFirstToken = conditionalExpression.WhenFalse.GetFirstToken();

            var replacement = enabled ? " " : string.Empty;
            var beforeQuestionSpan = TextSpan.FromBounds(conditionLastToken.Span.End, questionToken.SpanStart);
            var afterQuestionSpan = TextSpan.FromBounds(questionToken.Span.End, whenTrueFirstToken.SpanStart);
            var beforeColonSpan = TextSpan.FromBounds(whenTrueLastToken.Span.End, colonToken.SpanStart);
            var afterColonSpan = TextSpan.FromBounds(colonToken.Span.End, whenFalseFirstToken.SpanStart);

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new List<TextChange>();

            if (text.ToString(beforeQuestionSpan) != replacement)
                changes.Add(new TextChange(beforeQuestionSpan, replacement));

            if (text.ToString(afterQuestionSpan) != replacement)
                changes.Add(new TextChange(afterQuestionSpan, replacement));

            if (text.ToString(beforeColonSpan) != replacement)
                changes.Add(new TextChange(beforeColonSpan, replacement));

            if (text.ToString(afterColonSpan) != replacement)
                changes.Add(new TextChange(afterColonSpan, replacement));

            if (changes.Count == 0)
                return document;

            return document.WithText(text.WithChanges(changes));
        }
    }
}
