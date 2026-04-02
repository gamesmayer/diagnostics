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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0077CodeFixProvider))]
    [Shared]
    public sealed class GM0077CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0077Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix block content indentation",
                    createChangedDocument: ct => FixIndentAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0077CodeFixProvider)),
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
            var tree = root.SyntaxTree;

            int expectedIndent;
            var block = token.Parent?.FirstAncestorOrSelf<BlockSyntax>();
            if (block != null)
            {
                var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
                var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
                var declarationIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
                expectedIndent = declarationIndent + 4;
            }
            else
            {
                var member = token.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                var typeDeclaration = member?.Parent as TypeDeclarationSyntax;
                if (member == null || typeDeclaration == null)
                    return document;

                var declarationLine = tree.GetLineSpan(typeDeclaration.Identifier.Span).StartLinePosition.Line;
                var declarationIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
                expectedIndent = declarationIndent + 4;
            }

            var statementTextLine = sourceText.Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start);
            int actualIndent = GM0077Analyzer.CountLeadingWhitespace(statementTextLine.ToString());

            var indentSpan = new TextSpan(statementTextLine.Start, actualIndent);
            var updatedText = sourceText.WithChanges(new TextChange(indentSpan, new string(' ', expectedIndent)));
            return document.WithText(updatedText);
        }
    }
}
