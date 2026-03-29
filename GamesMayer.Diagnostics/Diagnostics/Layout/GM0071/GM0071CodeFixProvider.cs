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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0071CodeFixProvider))]
    [Shared]
    public sealed class GM0071CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0071Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Insert blank line before accessor",
                    createChangedDocument: ct => InsertBlankLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0071CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> InsertBlankLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var currAccessor = token.Parent?.AncestorsAndSelf().OfType<AccessorDeclarationSyntax>().FirstOrDefault();
            if (currAccessor?.Parent is not AccessorListSyntax accessorList)
                return document;

            var idx = accessorList.Accessors.IndexOf(currAccessor);
            if (idx <= 0)
                return document;

            var prevAccessor = accessorList.Accessors[idx - 1];

            var change = new TextChange(
                TextSpan.FromBounds(prevAccessor.GetLastToken().Span.End, currAccessor.GetFirstToken().SpanStart),
                Environment.NewLine + Environment.NewLine + GetLineIndentation(sourceText, currAccessor.SpanStart));

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
