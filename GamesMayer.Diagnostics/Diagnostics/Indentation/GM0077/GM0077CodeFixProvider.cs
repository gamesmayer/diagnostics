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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0077CodeFixProvider))]
    [Shared]
    public sealed class GM0077CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0077Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix content indentation",
                    createChangedDocument: ct => FixIndentAsync(context.Document, diagnostic, ct),
                    equivalenceKey: nameof(GM0077CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixIndentAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var tree = root.SyntaxTree;

            int expectedIndent;
            if (token.Parent is ElseClauseSyntax elseClause &&
                elseClause.Parent is IfStatementSyntax parentIf)
            {
                var ifLine = tree.GetLineSpan(parentIf.IfKeyword.Span).StartLinePosition.Line;
                expectedIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[ifLine].ToString());
            }
            else if (token.Parent is CatchClauseSyntax catchClause &&
                     catchClause.Parent is TryStatementSyntax parentTryForCatch)
            {
                var tryLine = tree.GetLineSpan(parentTryForCatch.TryKeyword.Span).StartLinePosition.Line;
                expectedIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[tryLine].ToString());
            }
            else if (token.Parent is FinallyClauseSyntax finallyClause &&
                     finallyClause.Parent is TryStatementSyntax parentTryForFinally)
            {
                var tryLine = tree.GetLineSpan(parentTryForFinally.TryKeyword.Span).StartLinePosition.Line;
                expectedIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[tryLine].ToString());
            }
            else
            {
                var initializer = token.Parent?.FirstAncestorOrSelf<InitializerExpressionSyntax>();
                if (initializer != null && initializer.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression))
                {
                    var openBraceLine = tree.GetLineSpan(initializer.OpenBraceToken.Span).StartLinePosition.Line;
                    var openBraceIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[openBraceLine].ToString());
                    expectedIndent = openBraceIndent + 4;
                }
                else if (initializer != null &&
                         (initializer.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectInitializerExpression) ||
                          initializer.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CollectionInitializerExpression)))
                {
                    var newKeyword = FindNewKeyword(initializer.Parent);
                    if (newKeyword == default)
                        return document;

                    var declarationLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
                    var declarationIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
                    expectedIndent = declarationIndent + 4;
                }
                else
                {
                    var block = token.Parent?.FirstAncestorOrSelf<BlockSyntax>();
                    if (block != null)
                    {
                        var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
                        var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
                        var declarationIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
                        expectedIndent = declarationIndent + 4;
                    }
                    else
                    {
                        var accessor = token.Parent?.FirstAncestorOrSelf<AccessorDeclarationSyntax>();
                        if (accessor?.Parent is AccessorListSyntax accessorList)
                        {
                            var declarationFirstToken = accessorList.Parent?.GetFirstToken() ?? accessorList.OpenBraceToken;
                            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
                            var declarationIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
                            expectedIndent = declarationIndent + 4;
                        }
                        else
                        {
                            var member = token.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>();
                            var typeDeclaration = member?.Parent as TypeDeclarationSyntax;
                            if (member == null || typeDeclaration == null)
                                return document;

                            var declarationLine = tree.GetLineSpan(typeDeclaration.Identifier.Span).StartLinePosition.Line;
                            var declarationIndent = GM0077Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
                            expectedIndent = declarationIndent + 4;
                        }
                    }
                }
            }

            var statementTextLine = sourceText.Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start);
            int actualIndent = GM0077Analyzer.CountLeadingWhitespace(statementTextLine.ToString());

            var indentSpan = new TextSpan(statementTextLine.Start, actualIndent);
            var updatedText = sourceText.WithChanges(new TextChange(indentSpan, new string(' ', expectedIndent)));
            return document.WithText(updatedText);
        }

        private static SyntaxToken FindNewKeyword(SyntaxNode? node)
        {
            var current = node;
            while (current != null)
            {
                if (current is ObjectCreationExpressionSyntax objectCreation)
                    return objectCreation.NewKeyword;
                if (current is ImplicitObjectCreationExpressionSyntax implicitCreation)
                    return implicitCreation.NewKeyword;
                if (current is AnonymousObjectCreationExpressionSyntax anonymousCreation)
                    return anonymousCreation.NewKeyword;
                current = current.Parent;
            }

            return default;
        }
    }
}
