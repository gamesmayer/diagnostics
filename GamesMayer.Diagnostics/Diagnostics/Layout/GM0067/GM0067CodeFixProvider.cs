using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0067CodeFixProvider))]
    [Shared]
    public sealed class GM0067CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0067Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add a blank line after the block statement",
                    createChangedDocument: ct => AddBlankLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0067CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> AddBlankLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var lastToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

            var blockStatement = lastToken.Parent?
                .AncestorsAndSelf()
                .OfType<StatementSyntax>()
                .FirstOrDefault(s => s.Parent is BlockSyntax);

            if (blockStatement == null)
                return document;

            var enclosingBlock = (BlockSyntax)blockStatement.Parent!;
            var index = enclosingBlock.Statements.IndexOf(blockStatement);

            if (index < 0 || index >= enclosingBlock.Statements.Count - 1)
                return document;

            var nextStatement = enclosingBlock.Statements[index + 1];
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var nextToken = nextStatement.GetFirstToken();
            var leadingTriviaSpan = TextSpan.FromBounds(nextToken.FullSpan.Start, nextToken.SpanStart);
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
