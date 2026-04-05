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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0127CodeFixProvider))]
    [Shared]
    public sealed class GM0127CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0127Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            if (diagnostic.Properties.TryGetValue(GM0127Analyzer.FixTypeKey, out var fixType)
                && fixType == GM0127Analyzer.CollapseFixType)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Collapse arguments to a single line",
                        createChangedDocument: ct => CollapseToSingleLineAsync(context.Document, diagnostic, ct),
                        equivalenceKey: nameof(GM0127CodeFixProvider) + "_Collapse"),
                    diagnostic);
            }
            else
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Move all arguments to their own lines",
                        createChangedDocument: ct => SplitArgumentsToOwnLinesAsync(context.Document, diagnostic, ct),
                        equivalenceKey: nameof(GM0127CodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private static async Task<Document> CollapseToSingleLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var argList = node as ArgumentListSyntax
                ?? node?.AncestorsAndSelf().OfType<ArgumentListSyntax>().FirstOrDefault();
            if (argList == null)
                return document;

            var argsText = string.Join(", ", argList.Arguments.Select(a => sourceText.ToString(a.Span)));
            var singleLine = $"({argsText})";

            var openParen = argList.OpenParenToken;
            var prevToken = openParen.GetPreviousToken();

            int startPos = argList.SpanStart;
            if (prevToken != default)
            {
                var prevTokenLine = sourceText.Lines.GetLineFromPosition(prevToken.Span.End).LineNumber;
                var openParenLine = sourceText.Lines.GetLineFromPosition(openParen.SpanStart).LineNumber;
                if (prevTokenLine != openParenLine)
                    startPos = prevToken.Span.End;
            }

            int endPos = argList.Span.End;
            var updatedText = sourceText.Replace(new TextSpan(startPos, endPos - startPos), singleLine);
            return document.WithText(updatedText);
        }

        private static async Task<Document> SplitArgumentsToOwnLinesAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var diagnosticToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var argList = diagnosticToken.Parent?.AncestorsAndSelf().OfType<ArgumentListSyntax>().FirstOrDefault();
            if (argList == null || argList.Arguments.Count < 1)
                return document;

            string newline = DetectNewline(sourceText);
            string baseIndent = FindBaseIndent(sourceText, argList.SpanStart);
            string indent = baseIndent + "    ";

            var replacements = new List<(int Start, int Length, string Text)>();

            var openParen = argList.OpenParenToken;
            var firstArg = argList.Arguments[0];

            var prevToken = openParen.GetPreviousToken();
            if (prevToken != default)
            {
                var prevTokenLine = sourceText.Lines.GetLineFromPosition(prevToken.Span.End).LineNumber;
                var openParenStartLine = sourceText.Lines.GetLineFromPosition(openParen.SpanStart).LineNumber;

                if (prevTokenLine == openParenStartLine)
                {
                    replacements.Add((prevToken.Span.End, openParen.SpanStart - prevToken.Span.End, newline + baseIndent));
                }
            }

            var openParenEndLine = sourceText.Lines.GetLineFromPosition(openParen.Span.End).LineNumber;
            var firstArgLine = sourceText.Lines.GetLineFromPosition(firstArg.SpanStart).LineNumber;

            if (openParenEndLine == firstArgLine)
            {
                int afterOpen = openParen.Span.End;
                int beforeFirst = firstArg.SpanStart;
                replacements.Add((afterOpen, beforeFirst - afterOpen, newline + indent));
            }

            var separators = argList.Arguments.GetSeparators().ToList();
            for (int i = 0; i < separators.Count; i++)
            {
                var separator = separators[i];
                var nextArg = argList.Arguments[i + 1];

                int sepEnd = separator.Span.End;
                int nextStart = nextArg.SpanStart;

                var sepLine = sourceText.Lines.GetLineFromPosition(sepEnd).LineNumber;
                var nextLine = sourceText.Lines.GetLineFromPosition(nextStart).LineNumber;

                if (sepLine == nextLine)
                    replacements.Add((sepEnd, nextStart - sepEnd, newline + indent));
            }

            var lastArg = argList.Arguments[argList.Arguments.Count - 1];
            var closeParen = argList.CloseParenToken;
            var closeParenLine = sourceText.Lines.GetLineFromPosition(closeParen.SpanStart).LineNumber;
            var lastArgLine = sourceText.Lines.GetLineFromPosition(lastArg.Span.End).LineNumber;

            if (lastArgLine == closeParenLine)
            {
                int beforeClose = closeParen.SpanStart;
                int afterLastArg = lastArg.Span.End;
                replacements.Add((afterLastArg, beforeClose - afterLastArg, newline + baseIndent));
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
