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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0025CodeFixProvider))]
    [Shared]
    public sealed class GM0025CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0025Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Place on its own line",
                    createChangedDocument: ct => FixArgumentLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0025CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixArgumentLineAsync(
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

            SyntaxToken openParen;
            SyntaxToken precedingSeparator;

            var argument = token.Parent?.FirstAncestorOrSelf<ArgumentSyntax>();
            if (argument?.Parent is ArgumentListSyntax argList)
            {
                openParen = argList.OpenParenToken;
                var arguments = argList.Arguments;
                int itemIndex = arguments.IndexOf(argument);
                if (itemIndex <= 0)
                    return document;
                precedingSeparator = arguments.GetSeparator(itemIndex - 1);
            }
            else
            {
                var parameter = token.Parent?.FirstAncestorOrSelf<ParameterSyntax>();
                if (parameter?.Parent is not ParameterListSyntax paramList)
                    return document;

                openParen = paramList.OpenParenToken;
                var parameters = paramList.Parameters;
                int itemIndex = parameters.IndexOf(parameter);
                if (itemIndex <= 0)
                    return document;

                precedingSeparator = parameters.GetSeparator(itemIndex - 1);
            }

            var openParenLineNumber = syntaxTree.GetLineSpan(openParen.Span).StartLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, openParenLineNumber, indentSize);

            var currentFirstToken = argument != null
                ? argument.GetFirstToken()
                : token.Parent!.FirstAncestorOrSelf<ParameterSyntax>()!.GetFirstToken();
            var changeSpan = TextSpan.FromBounds(precedingSeparator.Span.End, currentFirstToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(changeSpan, "\n" + expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
