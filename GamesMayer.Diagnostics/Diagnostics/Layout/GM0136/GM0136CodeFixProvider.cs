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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0136CodeFixProvider))]
    [Shared]
    public sealed class GM0136CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0136Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Collapse generic type argument list to a single line",
                    createChangedDocument: ct => CollapseToSingleLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0136CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> CollapseToSingleLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var typeArgList = node as TypeArgumentListSyntax
                ?? node?.AncestorsAndSelf().OfType<TypeArgumentListSyntax>().FirstOrDefault();
            if (typeArgList == null)
                return document;

            var argsText = string.Join(", ", typeArgList.Arguments.Select(a => sourceText.ToString(a.Span)));
            var singleLine = $"<{argsText}>";

            var updatedText = sourceText.Replace(new TextSpan(typeArgList.SpanStart, typeArgList.Span.Length), singleLine);
            return document.WithText(updatedText);
        }
    }
}
