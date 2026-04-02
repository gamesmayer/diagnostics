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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0093CodeFixProvider))]
    [Shared]
    public sealed class GM0093CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0093Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0093Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space between method call name and opening parenthesis"
                : "Remove space between method call name and opening parenthesis";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0093CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixSpacingAsync(
            Document document,
            Diagnostic diagnostic,
            bool enabled,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var openParen = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!TryGetNameToken(openParen, out var nameToken))
                return document;

            var betweenSpan = TextSpan.FromBounds(nameToken.Span.End, openParen.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (text.ToString(betweenSpan) == replacement)
                return document;

            var newText = text.WithChanges(new TextChange(betweenSpan, replacement));
            return document.WithText(newText);
        }

        private static bool TryGetNameToken(SyntaxToken openParen, out SyntaxToken nameToken)
        {
            switch (openParen.Parent)
            {
                case ArgumentListSyntax argumentList:
                    switch (argumentList.Parent)
                    {
                        case InvocationExpressionSyntax invocation:
                            nameToken = invocation.Expression.GetLastToken();
                            return true;
                        case ObjectCreationExpressionSyntax objectCreation:
                            nameToken = objectCreation.Type.GetLastToken();
                            return true;
                        case ImplicitObjectCreationExpressionSyntax implicitObjectCreation:
                            nameToken = implicitObjectCreation.NewKeyword;
                            return true;
                    }
                    break;
            }

            nameToken = default;
            return false;
        }
    }
}
