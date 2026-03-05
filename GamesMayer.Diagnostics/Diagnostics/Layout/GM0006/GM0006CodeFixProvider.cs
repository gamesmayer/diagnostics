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

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0006CodeFixProvider))]
    [Shared]
    public sealed class GM0006CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0006Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var blankLinePosition = diagnostic.Location.SourceSpan.Start;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove blank lines between attributes",
                    createChangedDocument: ct => RemoveBlankLinesBetweenAttributesAsync(context.Document, blankLinePosition, ct),
                    equivalenceKey: nameof(GM0006CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> RemoveBlankLinesBetweenAttributesAsync(
            Document document,
            int blankLinePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var attributeList = root.DescendantNodes()
                .OfType<AttributeListSyntax>()
                .FirstOrDefault(list => list.GetLeadingTrivia()
                    .Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.SpanStart == blankLinePosition));
            if (attributeList == null)
                return document;

            var parent = attributeList.Parent;
            if (parent == null)
                return document;

            if (!TryGetAttributeLists(parent, out var attributeLists))
                return document;

            var index = attributeLists.IndexOf(attributeList);
            if (index <= 0)
                return document;

            var previous = attributeLists[index - 1];
            var previousLastToken = previous.GetLastToken();
            var currentLeadingTrivia = attributeList.GetLeadingTrivia();

            var newPreviousTrailingTrivia = SyntaxFactory.TriviaList(
                previousLastToken.TrailingTrivia
                    .Where(t => !t.IsKind(SyntaxKind.WhitespaceTrivia) && !t.IsKind(SyntaxKind.EndOfLineTrivia))
                    .Concat(new[] { SyntaxFactory.EndOfLine("\n") }));

            var newCurrentLeadingTrivia = SyntaxFactory.TriviaList(
                currentLeadingTrivia.Where(t => !t.IsKind(SyntaxKind.EndOfLineTrivia)));

            var newPreviousToken = previousLastToken.WithTrailingTrivia(newPreviousTrailingTrivia);
            var interimRoot = root.ReplaceToken(previousLastToken, newPreviousToken);

            var currentAttributeList = interimRoot.FindNode(attributeList.Span) as AttributeListSyntax;
            if (currentAttributeList == null)
                return document.WithSyntaxRoot(interimRoot);

            var newCurrentAttributeList = currentAttributeList.WithLeadingTrivia(newCurrentLeadingTrivia);
            var newRoot = interimRoot.ReplaceNode(currentAttributeList, newCurrentAttributeList);

            return document.WithSyntaxRoot(newRoot);
        }

        private static bool TryGetAttributeLists(SyntaxNode node, out SyntaxList<AttributeListSyntax> attributeLists)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDecl:
                    attributeLists = classDecl.AttributeLists;
                    return true;
                case StructDeclarationSyntax structDecl:
                    attributeLists = structDecl.AttributeLists;
                    return true;
                case InterfaceDeclarationSyntax interfaceDecl:
                    attributeLists = interfaceDecl.AttributeLists;
                    return true;
                case EnumDeclarationSyntax enumDecl:
                    attributeLists = enumDecl.AttributeLists;
                    return true;
                case DelegateDeclarationSyntax delegateDecl:
                    attributeLists = delegateDecl.AttributeLists;
                    return true;
                case MethodDeclarationSyntax methodDecl:
                    attributeLists = methodDecl.AttributeLists;
                    return true;
                case PropertyDeclarationSyntax propertyDecl:
                    attributeLists = propertyDecl.AttributeLists;
                    return true;
                case FieldDeclarationSyntax fieldDecl:
                    attributeLists = fieldDecl.AttributeLists;
                    return true;
                case EventDeclarationSyntax eventDecl:
                    attributeLists = eventDecl.AttributeLists;
                    return true;
                case EventFieldDeclarationSyntax eventFieldDecl:
                    attributeLists = eventFieldDecl.AttributeLists;
                    return true;
                case ConstructorDeclarationSyntax ctorDecl:
                    attributeLists = ctorDecl.AttributeLists;
                    return true;
                case DestructorDeclarationSyntax dtorDecl:
                    attributeLists = dtorDecl.AttributeLists;
                    return true;
                case OperatorDeclarationSyntax opDecl:
                    attributeLists = opDecl.AttributeLists;
                    return true;
                case ConversionOperatorDeclarationSyntax convOpDecl:
                    attributeLists = convOpDecl.AttributeLists;
                    return true;
                case IndexerDeclarationSyntax indexerDecl:
                    attributeLists = indexerDecl.AttributeLists;
                    return true;
                case ParameterSyntax parameterDecl:
                    attributeLists = parameterDecl.AttributeLists;
                    return true;
                case TypeParameterSyntax typeParamDecl:
                    attributeLists = typeParamDecl.AttributeLists;
                    return true;
                case RecordDeclarationSyntax recordDecl:
                    attributeLists = recordDecl.AttributeLists;
                    return true;
                default:
                    attributeLists = default;
                    return false;
            }
        }
    }
}
