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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0078CodeFixProvider))]
    [Shared]
    public sealed class GM0078CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0078Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix brace indentation",
                    createChangedDocument: ct => FixIndentAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0078CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var block = token.Parent?.FirstAncestorOrSelf<BlockSyntax>();
            if (block == null)
                return document;

            var tree = root.SyntaxTree;
            var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            int declarationIndent = GM0078Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());

            var braceLine = sourceText.Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start);
            int actualIndent = GM0078Analyzer.CountLeadingWhitespace(braceLine.ToString());

            var indentSpan = new TextSpan(braceLine.Start, actualIndent);
            var updatedText = sourceText.WithChanges(new TextChange(indentSpan, new string(' ', declarationIndent)));
            return document.WithText(updatedText);
        }
    }
}
