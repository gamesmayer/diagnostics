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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0087CodeFixProvider))]
    [Shared]
    public sealed class GM0087CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0087Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0087Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space after ':' in inheritance clause"
                : "Remove space after ':' in inheritance clause";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0087CodeFixProvider)),
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
            SyntaxToken nextToken;
            if (token.Parent?.FirstAncestorOrSelf<BaseListSyntax>() is BaseListSyntax baseList)
            {
                if (baseList.Types.Count == 0)
                    return document;
                colonToken = baseList.ColonToken;
                nextToken = baseList.Types[0].GetFirstToken();
            }
            else if (token.Parent?.FirstAncestorOrSelf<ConstructorInitializerSyntax>() is ConstructorInitializerSyntax ctorInit)
            {
                colonToken = ctorInit.ColonToken;
                nextToken = ctorInit.ThisOrBaseKeyword;
            }
            else if (token.Parent?.FirstAncestorOrSelf<TypeParameterConstraintClauseSyntax>() is TypeParameterConstraintClauseSyntax constraintClause)
            {
                if (constraintClause.Constraints.Count == 0)
                    return document;
                colonToken = constraintClause.ColonToken;
                nextToken = constraintClause.Constraints[0].GetFirstToken();
            }
            else
            {
                return document;
            }

            var betweenSpan = TextSpan.FromBounds(colonToken.Span.End, nextToken.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (text.ToString(betweenSpan) == replacement)
                return document;

            var newText = text.Replace(betweenSpan, replacement);
            return document.WithText(newText);
        }
    }
}
