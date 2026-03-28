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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0068CodeFixProvider))]
    [Shared]
    public sealed class GM0068CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0068Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move statement to its own line",
                    createChangedDocument: ct => MoveStatementToOwnLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0068CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveStatementToOwnLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var currStatement = token.Parent?.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (currStatement?.Parent is not BlockSyntax block)
                return document;

            var idx = block.Statements.IndexOf(currStatement);
            if (idx <= 0)
                return document;

            var prevStatement = block.Statements[idx - 1];
            var indentation = GetLineIndentation(sourceText, prevStatement.SpanStart);

            var change = new TextChange(
                TextSpan.FromBounds(prevStatement.GetLastToken().Span.End, currStatement.GetFirstToken().SpanStart),
                Environment.NewLine + indentation);

            return document.WithText(sourceText.WithChanges(change));
        }

        private static string GetLineIndentation(SourceText text, int position)
        {
            var line = text.Lines.GetLineFromPosition(position);
            var lineText = text.ToString(line.Span);
            var index = 0;

            while (index < lineText.Length && (lineText[index] == ' ' || lineText[index] == '\t'))
                index++;

            return lineText.Substring(0, index);
        }
    }
}
