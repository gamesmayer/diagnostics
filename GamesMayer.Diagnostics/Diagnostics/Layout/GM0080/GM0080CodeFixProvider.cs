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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0080CodeFixProvider))]
    [Shared]
    public sealed class GM0080CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0080Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add a blank line before the break statement",
                    createChangedDocument: ct => FixBlankLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0080CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixBlankLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var breakStatement = node as BreakStatementSyntax ?? node.Parent as BreakStatementSyntax;
            if (breakStatement == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var breakToken = breakStatement.BreakKeyword;
            var leadingTriviaSpan = TextSpan.FromBounds(breakToken.FullSpan.Start, breakToken.SpanStart);
            var leadingTriviaText = sourceText.GetSubText(leadingTriviaSpan).ToString();
            var indentation = GetIndentationAfterLastNewline(leadingTriviaText);

            var change = new TextChange(leadingTriviaSpan, Environment.NewLine + indentation);

            return document.WithText(sourceText.WithChanges(change));
        }

        private static string GetIndentationAfterLastNewline(string triviaText)
        {
            var lastNewline = triviaText.LastIndexOfAny(new[] { '\n', '\r' });

            if (lastNewline < 0)
                return triviaText;

            return triviaText.Substring(lastNewline + 1);
        }
    }
}
