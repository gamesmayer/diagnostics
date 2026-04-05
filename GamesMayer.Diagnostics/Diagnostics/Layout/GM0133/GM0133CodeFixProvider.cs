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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0133CodeFixProvider))]
    [Shared]
    public sealed class GM0133CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0133Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move constructor initializer to next line",
                    createChangedDocument: ct => MoveInitializerToNextLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0133CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> MoveInitializerToNextLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var token = root.FindToken(position);
            var initializer = token.Parent?.FirstAncestorOrSelf<ConstructorInitializerSyntax>();
            if (initializer == null)
            {
                return document;
            }

            var keyword = initializer.ThisOrBaseKeyword;
            if (keyword == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var constructorDeclaration = initializer.Parent as ConstructorDeclarationSyntax;
            if (constructorDeclaration == null)
            {
                return document;
            }

            var declarationLineNumber = syntaxTree.GetLineSpan(constructorDeclaration.Identifier.Span).StartLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            var indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedIndentSize)
                && parsedIndentSize > 0)
            {
                indentSize = parsedIndentSize;
            }

            var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, declarationLineNumber, indentSize);
            var replacementSpan = TextSpan.FromBounds(initializer.ColonToken.Span.End, keyword.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "\n" + expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
