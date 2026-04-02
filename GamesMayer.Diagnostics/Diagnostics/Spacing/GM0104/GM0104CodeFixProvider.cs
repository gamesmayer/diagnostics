using System.Collections.Immutable;
using System.Collections.Generic;
using System.Composition;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0104CodeFixProvider))]
    [Shared]
    public sealed class GM0104CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0104Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0104Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces between generic operators '<' and '>'"
                : "Remove spaces between generic operators '<' and '>'";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0104CodeFixProvider)),
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

            var lessThanToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!lessThanToken.IsKind(SyntaxKind.LessThanToken))
                return document;

            var greaterThanToken = lessThanToken.Parent switch
            {
                TypeParameterListSyntax typeParameterList => typeParameterList.GreaterThanToken,
                TypeArgumentListSyntax typeArgumentList => typeArgumentList.GreaterThanToken,
                _ => default
            };

            if (greaterThanToken == default)
                return document;

            var firstToken = lessThanToken.GetNextToken();
            var lastToken = greaterThanToken.GetPreviousToken();

            var replacement = enabled ? " " : string.Empty;
            var afterLessThanSpan = TextSpan.FromBounds(lessThanToken.Span.End, firstToken.SpanStart);
            var beforeGreaterThanSpan = TextSpan.FromBounds(lastToken.Span.End, greaterThanToken.SpanStart);

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new List<TextChange>();

            if (text.ToString(afterLessThanSpan) != replacement)
                changes.Add(new TextChange(afterLessThanSpan, replacement));

            if (text.ToString(beforeGreaterThanSpan) != replacement)
                changes.Add(new TextChange(beforeGreaterThanSpan, replacement));

            if (changes.Count == 0)
                return document;

            return document.WithText(text.WithChanges(changes));
        }
    }
}
