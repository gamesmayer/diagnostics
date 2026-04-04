using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0126CodeFixProvider))]
    [Shared]
    public sealed class GM0126CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0126Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move all parameters to their own lines",
                    createChangedDocument: ct => SplitParametersToOwnLinesAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0126CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> SplitParametersToOwnLinesAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var diagnosticToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var paramList = diagnosticToken.Parent?.AncestorsAndSelf().OfType<ParameterListSyntax>().FirstOrDefault();
            if (paramList == null || paramList.Parameters.Count < 2)
                return document;

            string newline = DetectNewline(sourceText);
            string indent = FindBaseIndent(sourceText, paramList.SpanStart) + "    ";

            var replacements = new List<(int Start, int Length, string Text)>();

            var openParen = paramList.OpenParenToken;
            var firstParam = paramList.Parameters[0];
            var openParenLine = sourceText.Lines.GetLineFromPosition(openParen.Span.End).LineNumber;
            var firstParamLine = sourceText.Lines.GetLineFromPosition(firstParam.SpanStart).LineNumber;

            if (openParenLine == firstParamLine)
            {
                int afterOpen = openParen.Span.End;
                int beforeFirst = firstParam.SpanStart;
                replacements.Add((afterOpen, beforeFirst - afterOpen, newline + indent));
            }

            var separators = paramList.Parameters.GetSeparators().ToList();
            for (int i = 0; i < separators.Count; i++)
            {
                var separator = separators[i];
                var nextParam = paramList.Parameters[i + 1];

                int sepEnd = separator.Span.End;
                int nextStart = nextParam.SpanStart;

                var sepLine = sourceText.Lines.GetLineFromPosition(sepEnd).LineNumber;
                var nextLine = sourceText.Lines.GetLineFromPosition(nextStart).LineNumber;

                if (sepLine == nextLine)
                    replacements.Add((sepEnd, nextStart - sepEnd, newline + indent));
            }

            if (replacements.Count == 0)
                return document;

            replacements.Sort((l, r) => r.Start.CompareTo(l.Start));

            var updatedText = sourceText;
            foreach (var r in replacements)
                updatedText = updatedText.Replace(new TextSpan(r.Start, r.Length), r.Text);

            return document.WithText(updatedText);
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
