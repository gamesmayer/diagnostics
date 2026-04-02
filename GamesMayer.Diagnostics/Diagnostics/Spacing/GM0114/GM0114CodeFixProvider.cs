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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0114CodeFixProvider))]
    [Shared]
    public sealed class GM0114CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0114Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0114Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space between accessors in single-line property"
                : "Remove space between accessors in single-line property";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0114CodeFixProvider)),
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

            var semicolonToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (semicolonToken.Parent is not AccessorDeclarationSyntax currentAccessor)
                return document;

            if (currentAccessor.Parent is not AccessorListSyntax accessorList)
                return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var replacement = enabled ? " " : string.Empty;
            var changes = new List<TextChange>();

            var accessors = accessorList.Accessors;
            for (var i = 0; i < accessors.Count - 1; i++)
            {
                var current = accessors[i];
                var next = accessors[i + 1];

                var betweenSpan = TextSpan.FromBounds(current.SemicolonToken.Span.End, next.SpanStart);
                if (text.ToString(betweenSpan) != replacement)
                    changes.Add(new TextChange(betweenSpan, replacement));
            }

            if (changes.Count == 0)
                return document;

            return document.WithText(text.WithChanges(changes));
        }
    }
}
