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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0086CodeFixProvider))]
    [Shared]
    public sealed class GM0086CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0086Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0086Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space before ':' in inheritance clause"
                : "Remove space before ':' in inheritance clause";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0086CodeFixProvider)),
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

            SyntaxToken colonToken;
            if (token.Parent?.FirstAncestorOrSelf<BaseListSyntax>() is BaseListSyntax baseList)
                colonToken = baseList.ColonToken;
            else if (token.Parent?.FirstAncestorOrSelf<ConstructorInitializerSyntax>() is ConstructorInitializerSyntax ctorInit)
                colonToken = ctorInit.ColonToken;
            else if (token.Parent?.FirstAncestorOrSelf<TypeParameterConstraintClauseSyntax>() is TypeParameterConstraintClauseSyntax constraintClause)
                colonToken = constraintClause.ColonToken;
            else
                return document;

            var previousToken = colonToken.GetPreviousToken();
            var betweenSpan = TextSpan.FromBounds(previousToken.Span.End, colonToken.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (text.ToString(betweenSpan) == replacement)
                return document;

            var newText = text.Replace(betweenSpan, replacement);
            return document.WithText(newText);
        }
    }
}
