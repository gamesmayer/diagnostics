using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0117CodeFixProvider))]
    [Shared]
    public sealed class GM0117CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0117Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            new SequentialFixAllProvider(this);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Place item on its own line",
                    createChangedDocument: ct => FixItemLineAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0117CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixItemLineAsync(
            Document document,
            int tokenPosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(tokenPosition);

            // A call starts with an IdentifierName (or MemberAccess), but the array owns the
            // complete invocation. Select the direct initializer item rather than its first subexpression.
            var expression = token.Parent?.FirstAncestorOrSelf<ExpressionSyntax>(node => node.Parent is InitializerExpressionSyntax);
            if (expression?.Parent is not InitializerExpressionSyntax initializer)
                return document;

            var tree = root.SyntaxTree;
            var openBraceLine = sourceText.Lines[tree.GetLineSpan(initializer.OpenBraceToken.Span).StartLinePosition.Line];
            var openBraceStartsLine = string.IsNullOrWhiteSpace(sourceText.ToString(TextSpan.FromBounds(openBraceLine.Start, initializer.OpenBraceToken.SpanStart)));
            int declarationIndent;

            // Items go one step in from their own '{' when it starts a line, so nested arrays (an argument, a
            // member of an anonymous object, a return value) indent from where they are, not from the statement.
            if (openBraceStartsLine)
            {
                declarationIndent = GM0078Analyzer.CountLeadingWhitespace(openBraceLine.ToString());
            }
            else
            {
                var declarationFirstToken = GM0078Analyzer.FindDeclarationFirstToken(initializer);
                if (declarationFirstToken == default)
                    return document;

                var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
                declarationIndent = GM0078Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
            }

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(tree);
            int indentStep = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentStep = parsedSize;
            }

            var expectedIndentation = new string(' ', declarationIndent + indentStep);

            var expressions = initializer.Expressions;
            int itemIndex = expressions.IndexOf(expression);
            if (itemIndex < 0)
                return document;

            int changeStart;
            if (itemIndex == 0)
            {
                changeStart = initializer.OpenBraceToken.Span.End;
            }
            else
            {
                var separator = expressions.GetSeparator(itemIndex - 1);
                changeStart = separator.Span.End;
            }

            var currentFirstToken = expression.GetFirstToken();
            var changeSpan = TextSpan.FromBounds(changeStart, currentFirstToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(changeSpan, "\n" + expectedIndentation));

            return document.WithText(updatedText);
        }
    }
}
