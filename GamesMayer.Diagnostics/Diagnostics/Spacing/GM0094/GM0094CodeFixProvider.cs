using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0094CodeFixProvider))]
    [Shared]
    public sealed class GM0094CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0094Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0094Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space after comma"
                : "Remove space after comma";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0094CodeFixProvider)),
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

            var commaToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!commaToken.IsKind(SyntaxKind.CommaToken))
                return document;

            var nextToken = commaToken.GetNextToken();
            if (nextToken.IsKind(SyntaxKind.None))
                return document;

            var afterCommaSpan = TextSpan.FromBounds(commaToken.Span.End, nextToken.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (text.ToString(afterCommaSpan) == replacement)
                return document;

            var newText = text.WithChanges(new TextChange(afterCommaSpan, replacement));
            return document.WithText(newText);
        }
    }
}
