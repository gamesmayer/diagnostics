using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0008CodeFixProvider))]
    [Shared]
    public sealed class GM0008CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0008Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var position = diagnostic.Location.SourceSpan.Start;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank line from argument or parameter list",
                    createChangedDocument: ct => RemoveBlankLineAsync(context.Document, position, ct),
                    equivalenceKey: nameof(GM0008CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> RemoveBlankLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var trivia = root.FindTrivia(position, findInsideTrivia: true);
            var argumentList = trivia.Token.Parent?.FirstAncestorOrSelf<ArgumentListSyntax>();
            var parameterList = trivia.Token.Parent?.FirstAncestorOrSelf<ParameterListSyntax>();

            if (argumentList == null && parameterList == null)
            {
                var token = root.FindToken(position);
                argumentList = token.Parent?.FirstAncestorOrSelf<ArgumentListSyntax>();
                parameterList = token.Parent?.FirstAncestorOrSelf<ParameterListSyntax>();
            }

            var listNode = (SyntaxNode?)argumentList ?? parameterList;
            if (listNode == null)
                return document;

            int itemCount = argumentList != null
                ? argumentList.Arguments.Count
                : parameterList!.Parameters.Count;
            if (itemCount == 0)
                return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var originalListText = text.GetSubText(listNode.FullSpan).ToString();
            var normalizedListText = CollapseBlankLines(originalListText);

            if (normalizedListText == originalListText)
                return document;

            var newText = text.WithChanges(new TextChange(listNode.FullSpan, normalizedListText));
            return document.WithText(newText);
        }

        private static string CollapseBlankLines(string text)
        {
            string current = text;
            while (true)
            {
                string next = Regex.Replace(current, "(\\r?\\n)[ \\t]*(\\r?\\n)([ \\t]*)", "$1$3");
                if (next == current)
                    return next;

                current = next;
            }
        }
    }
}
