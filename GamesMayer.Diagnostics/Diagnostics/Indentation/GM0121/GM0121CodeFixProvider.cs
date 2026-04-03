using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0121CodeFixProvider))]
    [Shared]
    public sealed class GM0121CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0121Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix object initializer brace indentation",
                    createChangedDocument: ct => FixIndentAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0121CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var newKeyword = GM0121Analyzer.FindNewKeyword(token.Parent!);
            if (newKeyword == default)
                return document;

            var tree = root.SyntaxTree;
            var newKeywordLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
            int expectedIndent = GM0121Analyzer.CountLeadingWhitespace(sourceText.Lines[newKeywordLine].ToString());

            var braceLine = sourceText.Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start);
            int actualIndent = GM0121Analyzer.CountLeadingWhitespace(braceLine.ToString());

            var indentSpan = new TextSpan(braceLine.Start, actualIndent);
            var updatedText = sourceText.WithChanges(new TextChange(indentSpan, new string(' ', expectedIndent)));
            return document.WithText(updatedText);
        }
    }
}
