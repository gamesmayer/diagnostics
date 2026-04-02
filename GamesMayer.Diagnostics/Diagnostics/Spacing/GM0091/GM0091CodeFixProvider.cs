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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0091CodeFixProvider))]
    [Shared]
    public sealed class GM0091CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0091Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0091Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add space between method declaration name and open parenthesis"
                : "Remove space between method declaration name and open parenthesis";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0091CodeFixProvider)),
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

            var openParen = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!TryGetNameToken(openParen, out var nameToken))
                return document;

            var betweenSpan = TextSpan.FromBounds(nameToken.Span.End, openParen.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (text.ToString(betweenSpan) == replacement)
                return document;

            var newText = text.WithChanges(new TextChange(betweenSpan, replacement));
            return document.WithText(newText);
        }

        private static bool TryGetNameToken(SyntaxToken openParen, out SyntaxToken nameToken)
        {
            var parent = openParen.Parent;
            switch (parent)
            {
                case ParameterListSyntax paramList:
                    switch (paramList.Parent)
                    {
                        case MethodDeclarationSyntax method:
                            nameToken = method.Identifier;
                            return true;
                        case ConstructorDeclarationSyntax ctor:
                            nameToken = ctor.Identifier;
                            return true;
                        case DestructorDeclarationSyntax dtor:
                            nameToken = dtor.Identifier;
                            return true;
                        case LocalFunctionStatementSyntax localFunc:
                            nameToken = localFunc.Identifier;
                            return true;
                    }
                    break;
            }

            nameToken = default;
            return false;
        }
    }
}
