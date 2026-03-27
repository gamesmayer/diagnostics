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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0059CodeFixProvider))]
    [Shared]
    public sealed class GM0059CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0059Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove whitespace before ':' in named argument",
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0059CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            if (node is not NameColonSyntax nameColon)
                return document;

            var nameToken = nameColon.Name.Identifier;
            var colonToken = nameColon.ColonToken;

            if (nameToken == default || colonToken == default || nameToken.Span.End >= colonToken.Span.Start)
                return document;

            var whitespaceSpan = TextSpan.FromBounds(nameToken.Span.End, colonToken.Span.Start);
            var updatedText = sourceText.WithChanges(new TextChange(whitespaceSpan, string.Empty));
            return document.WithText(updatedText);
        }
    }
}