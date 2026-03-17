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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0037CodeFixProvider))]
    [Shared]
    public sealed class GM0037CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0037Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Write parameter declaration header on a single line",
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic.Location.SourceSpan, ct),
                    equivalenceKey: nameof(GM0037CodeFixProvider)),
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

            var parameter = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as ParameterSyntax;
            if (parameter == null)
            {
                parameter = root.FindToken(diagnosticSpan.Start)
                    .Parent?
                    .FirstAncestorOrSelf<ParameterSyntax>();
            }

            if (parameter == null)
            {
                return document;
            }

            var firstHeaderToken = GetFirstNonAttributeToken(parameter.AttributeLists, parameter.GetFirstToken());
            var headerEndToken = parameter.Default?.EqualsToken ?? parameter.Identifier;
            var spanToNormalize = TextSpan.FromBounds(firstHeaderToken.SpanStart, headerEndToken.Span.End);

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var original = sourceText.ToString(spanToNormalize);
            var normalized = Regex.Replace(original, @"\s+", " ");

            return document.WithText(sourceText.WithChanges(new TextChange(spanToNormalize, normalized)));
        }

        private static SyntaxToken GetFirstNonAttributeToken(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken defaultToken)
        {
            return attributeLists.Count > 0
                ? attributeLists[attributeLists.Count - 1].GetLastToken().GetNextToken()
                : defaultToken;
        }
    }
}
