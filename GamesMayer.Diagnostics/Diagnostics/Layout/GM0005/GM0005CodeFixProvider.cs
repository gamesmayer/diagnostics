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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0005CodeFixProvider))]
    [Shared]
    public sealed class GM0005CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0005Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var attributeList = root.FindNode(diagnosticSpan) as AttributeListSyntax;
            if (attributeList == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Separate attributes into individual declarations",
                    createChangedDocument: ct => SeparateAttributesAsync(context.Document, attributeList, ct),
                    equivalenceKey: nameof(GM0005CodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> SeparateAttributesAsync(
            Document document,
            AttributeListSyntax attributeList,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            // Create separate attribute lists for each attribute
            var separateAttributeLists = attributeList.Attributes
                .Select((attr, index) =>
                {
                    var newList = SyntaxFactory.AttributeList(
                        attributeList.Target,
                        SyntaxFactory.SingletonSeparatedList(attr));

                    // First attribute gets the leading trivia from the original list
                    if (index == 0)
                    {
                        newList = newList.WithLeadingTrivia(attributeList.GetLeadingTrivia());
                    }
                    else
                    {
                        newList = newList.WithLeadingTrivia(SyntaxFactory.EndOfLine("\n"));
                    }

                    // Last attribute gets the trailing trivia from the original list
                    if (index == attributeList.Attributes.Count - 1)
                    {
                        newList = newList.WithTrailingTrivia(attributeList.GetTrailingTrivia());
                    }
                    
                    return newList;
                })
                .ToList();

            // Get the node that contains the attribute list (e.g., class, method, property)
            var parentNode = attributeList.Parent;
            if (parentNode == null)
                return document;

            // Find all attribute lists on the parent node
            var attributeLists = parentNode.GetType().GetProperty("AttributeLists")?.GetValue(parentNode) as SyntaxList<AttributeListSyntax>?;
            if (!attributeLists.HasValue)
                return document;

            // Find the index of the original attribute list
            var index = attributeLists.Value.IndexOf(attributeList);
            if (index < 0)
                return document;

            // Remove the original attribute list
            var newAttributeLists = attributeLists.Value.RemoveAt(index);

            // Insert all separate attribute lists at the same index
            newAttributeLists = newAttributeLists.InsertRange(index, separateAttributeLists);

            // Create new parent node with updated attribute lists
            SyntaxNode newParent = parentNode switch
            {
                ClassDeclarationSyntax classDecl => classDecl.WithAttributeLists(newAttributeLists),
                StructDeclarationSyntax structDecl => structDecl.WithAttributeLists(newAttributeLists),
                InterfaceDeclarationSyntax interfaceDecl => interfaceDecl.WithAttributeLists(newAttributeLists),
                EnumDeclarationSyntax enumDecl => enumDecl.WithAttributeLists(newAttributeLists),
                DelegateDeclarationSyntax delegateDecl => delegateDecl.WithAttributeLists(newAttributeLists),
                MethodDeclarationSyntax methodDecl => methodDecl.WithAttributeLists(newAttributeLists),
                PropertyDeclarationSyntax propertyDecl => propertyDecl.WithAttributeLists(newAttributeLists),
                FieldDeclarationSyntax fieldDecl => fieldDecl.WithAttributeLists(newAttributeLists),
                EventDeclarationSyntax eventDecl => eventDecl.WithAttributeLists(newAttributeLists),
                EventFieldDeclarationSyntax eventFieldDecl => eventFieldDecl.WithAttributeLists(newAttributeLists),
                ConstructorDeclarationSyntax ctorDecl => ctorDecl.WithAttributeLists(newAttributeLists),
                DestructorDeclarationSyntax dtorDecl => dtorDecl.WithAttributeLists(newAttributeLists),
                OperatorDeclarationSyntax opDecl => opDecl.WithAttributeLists(newAttributeLists),
                ConversionOperatorDeclarationSyntax convOpDecl => convOpDecl.WithAttributeLists(newAttributeLists),
                IndexerDeclarationSyntax indexerDecl => indexerDecl.WithAttributeLists(newAttributeLists),
                ParameterSyntax paramDecl => paramDecl.WithAttributeLists(newAttributeLists),
                TypeParameterSyntax typeParamDecl => typeParamDecl.WithAttributeLists(newAttributeLists),
                RecordDeclarationSyntax recordDecl => recordDecl.WithAttributeLists(newAttributeLists),
                _ => parentNode
            };

            var newRoot = root.ReplaceNode(parentNode, newParent);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
