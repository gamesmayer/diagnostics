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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0039CodeFixProvider))]
    [Shared]
    public sealed class GM0039CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0039Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move dot to next line",
                    createChangedDocument: ct => MoveDotToNextLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0039CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveDotToNextLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // The diagnostic should be at the dot position
            if (diagnosticSpan.Start >= sourceText.Length)
                return document;

            var dotChar = sourceText[diagnosticSpan.Start];
            if (dotChar != '.')
                return document;

            // Find everything from the dot to the first non-whitespace on the next line
            // and rearrange it as: newline + indent + dot

            int pos = diagnosticSpan.Start + 1;  // after the dot

            // Scan for the newline
            while (pos < sourceText.Length && sourceText[pos] != '\n' && sourceText[pos] != '\r')
                pos++;

            if (pos >= sourceText.Length)
                return document;  // No newline found

            int newlineStart = pos;
            int newlineEnd = pos + 1;
            if (sourceText[pos] == '\r' && pos + 1 < sourceText.Length && sourceText[pos + 1] == '\n')
                newlineEnd = pos + 2;

            // Find indent on the next line
            int indentEnd = newlineEnd;
            while (indentEnd < sourceText.Length && (sourceText[indentEnd] == ' ' || sourceText[indentEnd] == '\t'))
                indentEnd++;

            // Extract the parts
            string newline = sourceText.GetSubText(new TextSpan(newlineStart, newlineEnd - newlineStart)).ToString();
            string indent = sourceText.GetSubText(new TextSpan(newlineEnd, indentEnd - newlineEnd)).ToString();

            // Build the replacement: instead of ".\n    " we want "\n    ."
            // Total span to replace: from the dot through the indent
            int spanStart = diagnosticSpan.Start;
            int spanLength = indentEnd - spanStart;
            string replacement = newline + indent + ".";

            var newSourceText = sourceText.Replace(new TextSpan(spanStart, spanLength), replacement);
            return document.WithText(newSourceText);
        }
    }
}
