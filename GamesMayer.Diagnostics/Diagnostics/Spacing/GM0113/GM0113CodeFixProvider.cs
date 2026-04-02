using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0113CodeFixProvider))]
    [Shared]
    public sealed class GM0113CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0113Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0113Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces inside single-line block braces"
                : "Remove spaces inside single-line block braces";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0113CodeFixProvider)),
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

            var openBrace = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!TryGetCloseBrace(openBrace, out var closeBrace))
                return document;

            var firstToken = openBrace.GetNextToken();
            var lastToken = closeBrace.GetPreviousToken();

            if (firstToken == closeBrace || firstToken.SpanStart >= closeBrace.SpanStart)
                return document;

            var replacement = enabled ? " " : string.Empty;
            var afterOpenSpan = TextSpan.FromBounds(openBrace.Span.End, firstToken.SpanStart);
            var beforeCloseSpan = TextSpan.FromBounds(lastToken.Span.End, closeBrace.SpanStart);

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new List<TextChange>();

            if (text.ToString(afterOpenSpan) != replacement)
                changes.Add(new TextChange(afterOpenSpan, replacement));

            if (text.ToString(beforeCloseSpan) != replacement)
                changes.Add(new TextChange(beforeCloseSpan, replacement));

            if (changes.Count == 0)
                return document;

            return document.WithText(text.WithChanges(changes));
        }

        private static bool TryGetCloseBrace(SyntaxToken openBrace, out SyntaxToken closeBrace)
        {
            if (!openBrace.IsKind(SyntaxKind.OpenBraceToken))
            {
                closeBrace = default;
                return false;
            }

            switch (openBrace.Parent)
            {
                case BlockSyntax block:
                    closeBrace = block.CloseBraceToken;
                    return true;
                case AccessorListSyntax accessorList:
                    closeBrace = accessorList.CloseBraceToken;
                    return true;
                default:
                    closeBrace = default;
                    return false;
            }
        }
    }
}