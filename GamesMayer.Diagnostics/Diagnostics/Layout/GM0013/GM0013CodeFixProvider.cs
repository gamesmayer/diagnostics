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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0013CodeFixProvider))]
    [Shared]
    public sealed class GM0013CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0013Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Wrap switch case block in braces",
                    createChangedDocument: ct => WrapInBracesAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0013CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> WrapInBracesAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var position = diagnostic.Location.SourceSpan.Start;
            var token = root!.FindToken(position);
            var switchSection = token.Parent?.FirstAncestorOrSelf<SwitchSectionSyntax>();

            if (switchSection == null)
                return document;

            var firstStatement = switchSection.Statements[0];
            var lastStatement = switchSection.Statements.Last();

            // Get indentation of the case label
            var labelLine = sourceText.Lines.GetLineFromPosition(switchSection.Labels[0].SpanStart);
            var labelLineText = sourceText.ToString(labelLine.Span);
            var indent = labelLineText.Substring(0, labelLineText.Length - labelLineText.TrimStart().Length);

            // Get the full span of all statements (includes leading/trailing trivia)
            var statementsSpan = TextSpan.FromBounds(firstStatement.FullSpan.Start, lastStatement.FullSpan.End);
            var statementsText = sourceText.ToString(statementsSpan);

            var newText = $"{indent}{{\n{statementsText}{indent}}}\n";

            return document.WithText(sourceText.WithChanges(new TextChange(statementsSpan, newText)));
        }
    }
}
