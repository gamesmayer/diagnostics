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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0046CodeFixProvider))]
    [Shared]
    public sealed class GM0046CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0046Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move operator to end of previous line",
                    createChangedDocument: ct => FixOperatorPositionAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0046CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixOperatorPositionAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            // The diagnostic span is the operator token (&&/||).
            // Find the binary expression that owns this operator.
            var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!(operatorToken.Parent is BinaryExpressionSyntax binary)
                || binary.OperatorToken.Span != operatorToken.Span)
                return document;

            var prevLastToken = binary.Left.GetLastToken();
            var rightFirstToken = binary.Right.GetFirstToken();

            // Insert " &&" / " ||" after the previous operand's last token (before its trailing newline trivia).
            // Remove the operator and its trailing space from the start of the next line,
            // leaving the indent in place as the new indentation for the right operand.
            var changes = new[]
            {
                new TextChange(new TextSpan(prevLastToken.Span.End, 0), " " + operatorToken.Text),
                new TextChange(TextSpan.FromBounds(operatorToken.SpanStart, rightFirstToken.Span.Start), string.Empty),
            };

            return document.WithText(sourceText.WithChanges(changes));
        }
    }
}
