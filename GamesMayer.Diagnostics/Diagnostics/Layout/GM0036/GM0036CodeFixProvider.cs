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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0036CodeFixProvider))]
    [Shared]
    public sealed class GM0036CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0036Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Write declaration header on a single line",
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic.Location.SourceSpan, ct),
                    equivalenceKey: nameof(GM0036CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            TextSpan diagnosticSpan,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var declaration = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as LocalDeclarationStatementSyntax;
            if (declaration == null)
            {
                declaration = root.FindToken(diagnosticSpan.Start)
                    .Parent?
                    .FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
            }

            if (declaration == null || declaration.Declaration.Variables.Count != 1)
            {
                return document;
            }

            var variable = declaration.Declaration.Variables[0];
            TextSpan spanToNormalize;

            if (variable.Initializer == null)
            {
                spanToNormalize = TextSpan.FromBounds(
                    declaration.Declaration.Type.SpanStart,
                    declaration.SemicolonToken.Span.End);
            }
            else
            {
                spanToNormalize = TextSpan.FromBounds(
                    declaration.Declaration.Type.SpanStart,
                    variable.Initializer.EqualsToken.Span.End);
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var original = sourceText.ToString(spanToNormalize);
            var normalized = Regex.Replace(original, @"\s+", " ");

            return document.WithText(sourceText.WithChanges(new TextChange(spanToNormalize, normalized)));
        }
    }
}
