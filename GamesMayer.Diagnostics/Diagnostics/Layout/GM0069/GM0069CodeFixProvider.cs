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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0069CodeFixProvider))]
    [Shared]
    public sealed class GM0069CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0069Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Expand property accessor list across multiple lines",
                    createChangedDocument: ct => ExpandAccessorListAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0069CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> ExpandAccessorListAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var openBrace = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var accessorList = openBrace.Parent as AccessorListSyntax;
            if (accessorList == null)
                return document;

            var closeBrace = accessorList.CloseBraceToken;
            var outerIndent = GetLineIndentation(sourceText, openBrace.SpanStart);
            var innerIndent = outerIndent + "    ";

            var changes = new List<TextChange>();

            var tokenBeforeOpenBrace = openBrace.GetPreviousToken();
            changes.Add(new TextChange(
                TextSpan.FromBounds(tokenBeforeOpenBrace.Span.End, openBrace.SpanStart),
                Environment.NewLine + outerIndent));

            var firstAccessor = accessorList.Accessors.First();
            changes.Add(new TextChange(
                TextSpan.FromBounds(openBrace.Span.End, firstAccessor.GetFirstToken().SpanStart),
                Environment.NewLine + innerIndent));

            for (int i = 1; i < accessorList.Accessors.Count; i++)
            {
                var prev = accessorList.Accessors[i - 1];
                var curr = accessorList.Accessors[i];
                changes.Add(new TextChange(
                    TextSpan.FromBounds(prev.GetLastToken().Span.End, curr.GetFirstToken().SpanStart),
                    Environment.NewLine + innerIndent));
            }

            var lastAccessor = accessorList.Accessors.Last();
            changes.Add(new TextChange(
                TextSpan.FromBounds(lastAccessor.GetLastToken().Span.End, closeBrace.SpanStart),
                Environment.NewLine + outerIndent));

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
