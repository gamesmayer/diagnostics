using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0135CodeFixProvider))]
    [Shared]
    public sealed class GM0135CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0135Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add argument name",
                    createChangedDocument: ct => AddArgumentNameAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0135CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> AddArgumentNameAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            if (!diagnostic.Properties.TryGetValue(GM0135Analyzer.ParameterNameKey, out var paramName)
                || paramName == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var argSyntax = node as ArgumentSyntax
                ?? node?.AncestorsAndSelf().OfType<ArgumentSyntax>().FirstOrDefault();
            if (argSyntax == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var newText = sourceText.WithChanges(
                new TextChange(new TextSpan(argSyntax.SpanStart, 0), paramName + ": "));
            return document.WithText(newText);
        }
    }
}
