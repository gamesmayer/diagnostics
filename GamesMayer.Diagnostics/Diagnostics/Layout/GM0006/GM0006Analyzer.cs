using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0006Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0006";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Blank lines between attributes",
            messageFormat: "Remove blank lines between attributes on '{0}'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Attributes that apply to the same declaration must not be separated by blank lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.AttributeList);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var attributeList = (AttributeListSyntax)context.Node;
            var parent = attributeList.Parent;
            if (parent == null)
                return;

            if (!TryGetAttributeLists(parent, out var attributeLists))
                return;

            var index = attributeLists.IndexOf(attributeList);
            if (index <= 0)
                return;

            var previous = attributeLists[index - 1];

            var currentStartLine = attributeList.GetLocation().GetLineSpan().StartLinePosition.Line;
            var previousEndLine = previous.GetLocation().GetLineSpan().EndLinePosition.Line;

            if (currentStartLine - previousEndLine > 1)
            {
                var blankLineTrivia = default(SyntaxTrivia);
                foreach (var trivia in attributeList.GetLeadingTrivia())
                {
                    if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    {
                        blankLineTrivia = trivia;
                        break;
                    }
                }

                if (blankLineTrivia == default)
                    return;

                context.ReportDiagnostic(
                    Diagnostic.Create(Descriptor, blankLineTrivia.GetLocation(), GetDeclarationName(parent)));
            }
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

        private static string GetDeclarationName(SyntaxNode node) => node switch
        {
            BaseTypeDeclarationSyntax typeDecl => typeDecl.Identifier.Text,
            DelegateDeclarationSyntax delegateDecl => delegateDecl.Identifier.Text,
            MethodDeclarationSyntax methodDecl => methodDecl.Identifier.Text,
            PropertyDeclarationSyntax propertyDecl => propertyDecl.Identifier.Text,
            FieldDeclarationSyntax _ => "field",
            EventDeclarationSyntax eventDecl => eventDecl.Identifier.Text,
            EventFieldDeclarationSyntax _ => "event",
            ConstructorDeclarationSyntax ctorDecl => ctorDecl.Identifier.Text,
            DestructorDeclarationSyntax destructorDecl => "~" + destructorDecl.Identifier.Text,
            OperatorDeclarationSyntax operatorDecl => "operator " + operatorDecl.OperatorToken.Text,
            ConversionOperatorDeclarationSyntax conversionDecl => "operator " + conversionDecl.Type,
            IndexerDeclarationSyntax _ => "this",
            ParameterSyntax parameterDecl => parameterDecl.Identifier.Text,
            TypeParameterSyntax typeParamDecl => typeParamDecl.Identifier.Text,
            _ => "declaration"
        };
    }
}
