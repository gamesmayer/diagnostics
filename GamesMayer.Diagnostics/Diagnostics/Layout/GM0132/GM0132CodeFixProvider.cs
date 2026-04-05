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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0132CodeFixProvider))]
    [Shared]
    public sealed class GM0132CodeFixProvider : CodeFixProvider
    {
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0132.threshold";
        private const int DefaultThreshold = 2;

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0132Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var title = ShouldEnforceOwnLine(context.Document, diagnostic)
                ? "Move parent type to next line"
                : "Move parent type to declaration line";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => ApplyFixAsync(context.Document, diagnostic.Location.SourceSpan.Start, ct),
                    equivalenceKey: nameof(GM0132CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static bool ShouldEnforceOwnLine(Document document, Diagnostic diagnostic)
        {
            var syntaxTree = diagnostic.Location.SourceTree;
            if (syntaxTree == null)
            {
                return true;
            }

            var root = syntaxTree.GetRoot();
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var baseType = token.Parent?.FirstAncestorOrSelf<BaseTypeSyntax>();
            if (baseType?.Parent is not BaseListSyntax baseList)
            {
                return true;
            }

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            var threshold = DefaultThreshold;
            if (analyzerOptions.TryGetValue(ThresholdOptionKey, out var thresholdValue)
                && int.TryParse(thresholdValue, out var parsedThreshold)
                && parsedThreshold > 0)
            {
                threshold = parsedThreshold;
            }

            return baseList.Types.Count >= threshold;
        }

        private static async Task<Document> ApplyFixAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var token = root.FindToken(position);
            var baseType = token.Parent?.FirstAncestorOrSelf<BaseTypeSyntax>();
            if (baseType?.Parent is not BaseListSyntax baseList)
            {
                return document;
            }

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(root.SyntaxTree);
            var threshold = DefaultThreshold;
            if (analyzerOptions.TryGetValue(ThresholdOptionKey, out var thresholdValue)
                && int.TryParse(thresholdValue, out var parsedThreshold)
                && parsedThreshold > 0)
            {
                threshold = parsedThreshold;
            }

            return baseList.Types.Count >= threshold
                ? await MoveParentTypeToNextLineAsync(document, position, cancellationToken).ConfigureAwait(false)
                : await MoveParentTypeToDeclarationLineAsync(document, position, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<Document> MoveParentTypeToNextLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var token = root.FindToken(position);
            var baseType = token.Parent?.FirstAncestorOrSelf<BaseTypeSyntax>();
            if (baseType?.Parent is not BaseListSyntax baseList)
            {
                return document;
            }

            var baseTypeIndex = baseList.Types.IndexOf(baseType);
            if (baseTypeIndex < 0)
            {
                return document;
            }

            var firstToken = baseType.GetFirstToken();
            if (firstToken == default)
            {
                return document;
            }

            var previousToken = baseTypeIndex == 0
                ? baseList.ColonToken
                : baseList.Types.GetSeparator(baseTypeIndex - 1);

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;
            var declarationLineNumber = syntaxTree.GetLineSpan(baseList.ColonToken.Span).StartLinePosition.Line;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            var indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedIndentSize)
                && parsedIndentSize > 0)
            {
                indentSize = parsedIndentSize;
            }

            var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, declarationLineNumber, indentSize);
            var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, firstToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, "\n" + expectedIndentation));

            return document.WithText(updatedText);
        }

        private static async Task<Document> MoveParentTypeToDeclarationLineAsync(
            Document document,
            int position,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var token = root.FindToken(position);
            var baseType = token.Parent?.FirstAncestorOrSelf<BaseTypeSyntax>();
            if (baseType?.Parent is not BaseListSyntax baseList)
            {
                return document;
            }

            var baseTypeIndex = baseList.Types.IndexOf(baseType);
            if (baseTypeIndex < 0)
            {
                return document;
            }

            var firstToken = baseType.GetFirstToken();
            if (firstToken == default)
            {
                return document;
            }

            var previousToken = baseTypeIndex == 0
                ? baseList.ColonToken
                : baseList.Types.GetSeparator(baseTypeIndex - 1);

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var replacementSpan = TextSpan.FromBounds(previousToken.Span.End, firstToken.SpanStart);
            var updatedText = sourceText.WithChanges(new TextChange(replacementSpan, " "));

            return document.WithText(updatedText);
        }
    }
}