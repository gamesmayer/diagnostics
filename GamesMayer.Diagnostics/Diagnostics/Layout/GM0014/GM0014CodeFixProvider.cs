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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0014CodeFixProvider))]
    [Shared]
    public sealed class GM0014CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0014Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Place namespace identifier on a single line",
                    createChangedDocument: ct => CollapseNameAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0014CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> CollapseNameAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var position = diagnostic.Location.SourceSpan.Start;
            var token = root!.FindToken(position);
            var nsDecl = token.Parent?.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>();

            if (nsDecl == null)
                return document;

            var nameNode = nsDecl.Name;
            var cleanName = string.Concat(nameNode.DescendantTokens().Select(t => t.Text));
            var nameSpan = TextSpan.FromBounds(nameNode.GetFirstToken().SpanStart, nameNode.GetLastToken().Span.End);

            return document.WithText(sourceText.WithChanges(new TextChange(nameSpan, cleanName)));
        }
    }
}
