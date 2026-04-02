using System.Collections.Immutable;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0102CodeFixProvider))]
    [Shared]
    public sealed class GM0102CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0102Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0102Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces between square brackets"
                : "Remove spaces between square brackets";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0102CodeFixProvider)),
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

            var openBracket = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!openBracket.IsKind(SyntaxKind.OpenBracketToken))
                return document;

            var closeBracket = openBracket.Parent switch
            {
                BracketedArgumentListSyntax bracketedList => bracketedList.CloseBracketToken,
                AttributeListSyntax attributeList => attributeList.CloseBracketToken,
                _ => default
            };

            if (closeBracket == default)
                return document;
            var firstToken = openBracket.GetNextToken();
            var lastToken = closeBracket.GetPreviousToken();

            var replacement = enabled ? " " : string.Empty;
            var afterOpenSpan = TextSpan.FromBounds(openBracket.Span.End, firstToken.SpanStart);
            var beforeCloseSpan = TextSpan.FromBounds(lastToken.Span.End, closeBracket.SpanStart);

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new System.Collections.Generic.List<TextChange>();

            if (text.ToString(afterOpenSpan) != replacement)
                changes.Add(new TextChange(afterOpenSpan, replacement));

            if (text.ToString(beforeCloseSpan) != replacement)
                changes.Add(new TextChange(beforeCloseSpan, replacement));

            if (changes.Count == 0)
                return document;

            return document.WithText(text.WithChanges(changes));
        }
    }
}
