using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0085CodeFixProvider))]
    [Shared]
    public sealed class GM0085CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0085Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0085Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces inside method call argument parentheses"
                : "Remove spaces inside method call argument parentheses";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0085CodeFixProvider)),
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

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var argumentList = token.Parent?.FirstAncestorOrSelf<ArgumentListSyntax>();
            if (argumentList == null || argumentList.Arguments.Count == 0)
                return document;

            var firstArgumentToken = argumentList.Arguments[0].GetFirstToken();
            var lastArgumentToken = argumentList.Arguments[argumentList.Arguments.Count - 1].GetLastToken();

            var openSpan = TextSpan.FromBounds(argumentList.OpenParenToken.Span.End, firstArgumentToken.SpanStart);
            var closeSpan = TextSpan.FromBounds(lastArgumentToken.Span.End, argumentList.CloseParenToken.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new List<TextChange>();

            if (text.ToString(openSpan) != replacement)
                changes.Add(new TextChange(openSpan, replacement));

            if (text.ToString(closeSpan) != replacement)
                changes.Add(new TextChange(closeSpan, replacement));

            if (changes.Count == 0)
                return document;

            var newText = text.WithChanges(changes);
            return document.WithText(newText);
        }
    }
}
