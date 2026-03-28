using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0063CodeFixProvider))]
    [Shared]
    public sealed class GM0063CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0063Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Expand block body across multiple lines",
                    createChangedDocument: ct => ExpandBlockAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0063CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> ExpandBlockAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var openBrace = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var block = openBrace.Parent as BlockSyntax;
            if (block == null)
                return document;

            var closeBrace = block.CloseBraceToken;
            var outerIndent = GetLineIndentation(sourceText, openBrace.SpanStart);
            var innerIndent = outerIndent + "    ";

            var changes = new List<TextChange>
            {
                new TextChange(
                    TextSpan.FromBounds(openBrace.Span.End, block.Statements[0].GetFirstToken().SpanStart),
                    Environment.NewLine + innerIndent),
                new TextChange(
                    TextSpan.FromBounds(block.Statements.Last().GetLastToken().Span.End, closeBrace.SpanStart),
                    Environment.NewLine + outerIndent)
            };

            return document.WithText(sourceText.WithChanges(changes));
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
