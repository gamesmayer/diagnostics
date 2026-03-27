using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0058CodeFixProvider))]
    [Shared]
    public sealed class GM0058CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0058Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Write inheritance list on the same line as the type declaration",
                    createChangedDocument: ct => FixSingleLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0058CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixSingleLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            if (node is not BaseListSyntax baseList)
                return document;

            // Replace from the end of the token preceding ':' (identifier or '>') through the
            // last base type, so that any whitespace/newlines before ':' are also collapsed.
            var previousToken = baseList.ColonToken.GetPreviousToken();
            var lastToken = baseList.GetLastToken();

            var spanStart = previousToken.Span.End;
            var spanEnd = lastToken.Span.End;
            var nodeText = sourceText.GetSubText(TextSpan.FromBounds(spanStart, spanEnd)).ToString();

            var parts = nodeText.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var normalized = " " + string.Join(" ", parts);

            var updatedText = sourceText.WithChanges(new TextChange(TextSpan.FromBounds(spanStart, spanEnd), normalized));
            return document.WithText(updatedText);
        }
    }
}
