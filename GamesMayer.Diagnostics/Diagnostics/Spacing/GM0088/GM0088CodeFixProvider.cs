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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0088CodeFixProvider))]
    [Shared]
    public sealed class GM0088CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0088Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0088Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces around binary operator"
                : "Remove spaces around binary operator";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0088CodeFixProvider)),
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

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (token.Parent?.FirstAncestorOrSelf<BinaryExpressionSyntax>() is not BinaryExpressionSyntax binaryExpression)
                return document;

            if (binaryExpression.OperatorToken.SpanStart != token.SpanStart)
                return document;

            var operatorToken = binaryExpression.OperatorToken;
            var leftSpan = TextSpan.FromBounds(binaryExpression.Left.GetLastToken().Span.End, operatorToken.SpanStart);
            var rightSpan = TextSpan.FromBounds(operatorToken.Span.End, binaryExpression.Right.GetFirstToken().SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new List<TextChange>();

            if (text.ToString(leftSpan) != replacement)
                changes.Add(new TextChange(leftSpan, replacement));

            if (text.ToString(rightSpan) != replacement)
                changes.Add(new TextChange(rightSpan, replacement));

            if (changes.Count == 0)
                return document;

            var newText = text.WithChanges(changes);
            return document.WithText(newText);
        }
    }
}
