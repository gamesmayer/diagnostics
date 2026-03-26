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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0048CodeFixProvider))]
    [Shared]
    public sealed class GM0048CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0048Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move lambda body to same line as '=>'",
                    createChangedDocument: ct => FixLambdaBodyAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0048CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixLambdaBodyAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var firstBodyToken = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var arrowToken = firstBodyToken.GetPreviousToken();

            if (!arrowToken.IsKind(SyntaxKind.EqualsGreaterThanToken))
                return document;

            var change = new TextChange(TextSpan.FromBounds(arrowToken.Span.End, firstBodyToken.Span.Start), " ");

            return document.WithText(sourceText.WithChanges(change));
        }
    }
}
