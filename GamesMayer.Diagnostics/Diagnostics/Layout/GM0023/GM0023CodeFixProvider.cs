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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0023CodeFixProvider))]
    [Shared]
    public sealed class GM0023CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0023Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix indentation",
                    createChangedDocument: ct => FixIndentationAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0023CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentationAsync(
            Document document,
            int tokenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var token = root.FindToken(tokenPosition);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var node = token.Parent;
            while (node != null && !(node is ArgumentSyntax) && !(node is ParameterSyntax))
                node = node.Parent;

            if (node == null)
                return document;

            var listNode = node.Parent;
            SyntaxToken openParen;
            if (listNode is ArgumentListSyntax argList)
                openParen = argList.OpenParenToken;
            else if (listNode is ParameterListSyntax paramList)
                openParen = paramList.OpenParenToken;
            else
                return document;

            var openParenLineNumber = syntaxTree.GetLineSpan(openParen.Span).EndLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, openParenLineNumber, indentSize);

            var tokenLineNumber = syntaxTree.GetLineSpan(token.Span).StartLinePosition.Line;
            var tokenLine = sourceText.Lines[tokenLineNumber];
            var lineStr = tokenLine.ToString();

            int actualLen = 0;
            while (actualLen < lineStr.Length && (lineStr[actualLen] == ' ' || lineStr[actualLen] == '\t'))
                actualLen++;

            var indentSpan = new TextSpan(tokenLine.Start, actualLen);
            var updatedText = sourceText.WithChanges(new TextChange(indentSpan, expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
