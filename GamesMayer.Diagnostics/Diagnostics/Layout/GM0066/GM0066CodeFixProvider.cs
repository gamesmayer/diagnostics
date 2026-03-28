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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0066CodeFixProvider)), Shared]
    public sealed class GM0066CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0066Analyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var statement = token.Parent?.FirstAncestorOrSelf<StatementSyntax>();

            if (statement == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add blank line before block statement",
                    createChangedDocument: ct => AddBlankLineAsync(context.Document, statement, ct),
                    equivalenceKey: nameof(GM0066CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> AddBlankLineAsync(Document document, StatementSyntax statement, CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var line = sourceText.Lines.GetLineFromPosition(statement.SpanStart);
            var insertPosition = line.Start;

            var newSourceText = sourceText.WithChanges(new TextChange(new TextSpan(insertPosition, 0), System.Environment.NewLine));
            return document.WithText(newSourceText);
        }
    }
}
