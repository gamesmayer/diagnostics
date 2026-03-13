using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0016CodeFixProvider))]
    [Shared]
    public sealed class GM0016CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0016Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Write the attribute on a single line",
                    createChangedDocument: ct => CollapseAttributeListAsync(context.Document, diagnostic.Location.SourceSpan, ct),
                    equivalenceKey: nameof(GM0016CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> CollapseAttributeListAsync(
            Document document,
            TextSpan span,
            CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var original = sourceText.ToString(span);

            var collapsed = Regex.Replace(original, @"\s+", " ");
            collapsed = collapsed.Replace("[ ", "[").Replace(" ]", "]");

            return document.WithText(sourceText.WithChanges(new TextChange(span, collapsed)));
        }
    }
}
