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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0140CodeFixProvider))]
    [Shared]
    public sealed class GM0140CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0140Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            var usingDirective = node as UsingDirectiveSyntax
                ?? node.AncestorsAndSelf().OfType<UsingDirectiveSyntax>().FirstOrDefault();

            if (usingDirective == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove unused using directive",
                    createChangedDocument: ct => RemoveUsingDirectiveAsync(context.Document, usingDirective, ct),
                    equivalenceKey: nameof(GM0140CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> RemoveUsingDirectiveAsync(
            Document document,
            UsingDirectiveSyntax usingDirective,
            CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var line = text.Lines.GetLineFromPosition(usingDirective.SpanStart);
            var spanToRemove = TextSpan.FromBounds(line.Start, line.EndIncludingLineBreak);
            var newText = text.Replace(spanToRemove, string.Empty);
            return document.WithText(newText);
        }
    }
}
