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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0084CodeFixProvider))]
    [Shared]
    public sealed class GM0084CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0084Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0084Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces inside method declaration parameter parentheses"
                : "Remove spaces inside method declaration parameter parentheses";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0084CodeFixProvider)),
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
            var parameterList = token.Parent?.FirstAncestorOrSelf<ParameterListSyntax>();
            if (parameterList == null || parameterList.Parameters.Count == 0)
                return document;

            var firstParameterToken = parameterList.Parameters[0].GetFirstToken();
            var lastParameterToken = parameterList.Parameters[parameterList.Parameters.Count - 1].GetLastToken();

            var openSpan = TextSpan.FromBounds(parameterList.OpenParenToken.Span.End, firstParameterToken.SpanStart);
            var closeSpan = TextSpan.FromBounds(lastParameterToken.Span.End, parameterList.CloseParenToken.SpanStart);
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
