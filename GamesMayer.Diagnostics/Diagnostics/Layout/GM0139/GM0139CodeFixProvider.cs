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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0139CodeFixProvider))]
    [Shared]
    public sealed class GM0139CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0139Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Collapse type expression to a single line",
                    createChangedDocument: ct => CollapseToSingleLineAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0139CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> CollapseToSingleLineAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);

            if (node is TupleTypeSyntax tupleType)
                return CollapseTupleType(document, sourceText, tupleType);

            if (node is QualifiedNameSyntax qualifiedName)
                return CollapseQualifiedName(document, sourceText, qualifiedName);

            // GenericName, ArrayType, NullableType, PointerType, AliasQualifiedName:
            // remove all whitespace between the last token of the left part and the first token of the right part
            var jointToken = GetJointToken(node);
            if (jointToken == null)
                return document;

            var previousToken = jointToken.Value.GetPreviousToken();
            var change = new TextChange(TextSpan.FromBounds(previousToken.Span.End, jointToken.Value.Span.Start), "");
            return document.WithText(sourceText.WithChanges(change));
        }

        private static SyntaxToken? GetJointToken(SyntaxNode node)
        {
            switch (node)
            {
                case GenericNameSyntax genericName:
                    return genericName.TypeArgumentList.LessThanToken;
                case ArrayTypeSyntax arrayType:
                    return arrayType.RankSpecifiers[0].OpenBracketToken;
                case NullableTypeSyntax nullableType:
                    return nullableType.QuestionToken;
                case PointerTypeSyntax pointerType:
                    return pointerType.AsteriskToken;
                case AliasQualifiedNameSyntax aliasQualifiedName:
                    return aliasQualifiedName.ColonColonToken;
                default:
                    return null;
            }
        }

        private static Document CollapseQualifiedName(Document document, SourceText sourceText, QualifiedNameSyntax qualifiedName)
        {
            var leftEnd = qualifiedName.Left.GetLastToken().Span.End;
            var rightStart = qualifiedName.Right.GetFirstToken().Span.Start;
            var change = new TextChange(TextSpan.FromBounds(leftEnd, rightStart), ".");
            return document.WithText(sourceText.WithChanges(change));
        }

        private static Document CollapseTupleType(Document document, SourceText sourceText, TupleTypeSyntax tupleType)
        {
            var elements = string.Join(", ", tupleType.Elements.Select(e => sourceText.ToString(e.Span)));
            var singleLine = $"({elements})";
            var updatedText = sourceText.Replace(new TextSpan(tupleType.SpanStart, tupleType.Span.Length), singleLine);
            return document.WithText(updatedText);
        }
    }
}
