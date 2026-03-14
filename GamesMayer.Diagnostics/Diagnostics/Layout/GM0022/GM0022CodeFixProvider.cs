using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0022CodeFixProvider))]
    [Shared]
    public sealed class GM0022CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0022Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move opening brace to a new line",
                    createChangedDocument: ct => MoveOpeningBraceToNewLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0022CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveOpeningBraceToNewLineAsync(
            Document document,
            int bracePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var openBrace = root.FindToken(bracePosition);
            if (!openBrace.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OpenBraceToken))
            {
                return document;
            }

            var previousToken = openBrace.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenTokensSpan = TextSpan.FromBounds(previousToken.Span.End, openBrace.Span.Start);
            var betweenTokensText = sourceText.ToString(betweenTokensSpan);
            var preservedBetweenTokensText = betweenTokensText.TrimEnd(' ', '\t');
            var indent = GetLineIndentation(sourceText, previousToken.SpanStart);

            var replacement = string.Concat(preservedBetweenTokensText, Environment.NewLine, indent);
            var updatedText = sourceText.WithChanges(new TextChange(betweenTokensSpan, replacement));

            return document.WithText(updatedText);
        }

        private static string GetLineIndentation(SourceText text, int position)
        {
            var line = text.Lines.GetLineFromPosition(position);
            var lineText = text.ToString(line.Span);
            var index = 0;

            while (index < lineText.Length && (lineText[index] == ' ' || lineText[index] == '\t'))
            {
                index++;
            }

            return lineText.Substring(0, index);
        }
    }
}
