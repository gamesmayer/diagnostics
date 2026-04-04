using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0129CodeFixProvider))]
    [Shared]
    public sealed class GM0129CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0129Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0129Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Use explicit object creation"
                : "Simplify to implicit object creation";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0129CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixAsync(
            Document document,
            Diagnostic diagnostic,
            bool enabled,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            if (enabled)
                return await UseExplicitObjectCreationAsync(document, root, diagnostic, cancellationToken).ConfigureAwait(false);

            return UseImplicitObjectCreation(document, root, diagnostic);
        }

        private static async Task<Document> UseExplicitObjectCreationAsync(
            Document document,
            SyntaxNode root,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var implicitCreation = token.Parent?
                .AncestorsAndSelf()
                .OfType<ImplicitObjectCreationExpressionSyntax>()
                .FirstOrDefault();

            if (implicitCreation == null)
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                return document;

            var typeInfo = semanticModel.GetTypeInfo(implicitCreation, cancellationToken);
            var type = typeInfo.Type ?? typeInfo.ConvertedType;
            if (type == null)
                return document;

            var typeText = type.ToMinimalDisplayString(semanticModel, implicitCreation.SpanStart);
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var insertionPoint = implicitCreation.NewKeyword.Span.End;
            var newText = text.WithChanges(new TextChange(new TextSpan(insertionPoint, 0), " " + typeText));
            return document.WithText(newText);
        }

        private static Document UseImplicitObjectCreation(
            Document document,
            SyntaxNode root,
            Diagnostic diagnostic)
        {
            var explicitType = root.FindNode(diagnostic.Location.SourceSpan) as TypeSyntax;
            if (explicitType?.Parent is not ObjectCreationExpressionSyntax objectCreation)
                return document;

            var implicitCreation = GM0129Analyzer.BuildImplicitObjectCreationExpression(objectCreation)
                .WithTriviaFrom(objectCreation);

            var newRoot = root.ReplaceNode(objectCreation, implicitCreation);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
