using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0007Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0007";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Class members must be separated by a blank line",
            messageFormat: "Insert a blank line between '{0}' and '{1}'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class members must be separated by a blank line, including between consecutive fields.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var members = classDeclaration.Members;

            for (var i = 1; i < members.Count; i++)
            {
                var previous = members[i - 1];
                var current = members[i];

                var previousEndLine = previous.GetLocation().GetLineSpan().EndLinePosition.Line;
                var currentStartLine = current.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (currentStartLine - previousEndLine <= 1)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor,
                            current.GetFirstToken().GetLocation(),
                            GetMemberName(previous),
                            GetMemberName(current)));
                }
            }
        }

        private static string GetMemberName(MemberDeclarationSyntax member) => member switch
        {
            MethodDeclarationSyntax method => method.Identifier.Text,
            PropertyDeclarationSyntax property => property.Identifier.Text,
            FieldDeclarationSyntax field => field.Declaration.Variables.Count > 0
                ? field.Declaration.Variables[0].Identifier.Text
                : "field",
            EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.Text,
            EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables.Count > 0
                ? eventField.Declaration.Variables[0].Identifier.Text
                : "event",
            ConstructorDeclarationSyntax constructor => constructor.Identifier.Text,
            DestructorDeclarationSyntax destructor => "~" + destructor.Identifier.Text,
            IndexerDeclarationSyntax _ => "this",
            OperatorDeclarationSyntax @operator => "operator " + @operator.OperatorToken.Text,
            ConversionOperatorDeclarationSyntax conversion => "operator " + conversion.Type,
            ClassDeclarationSyntax nestedClass => nestedClass.Identifier.Text,
            StructDeclarationSyntax nestedStruct => nestedStruct.Identifier.Text,
            InterfaceDeclarationSyntax nestedInterface => nestedInterface.Identifier.Text,
            EnumDeclarationSyntax nestedEnum => nestedEnum.Identifier.Text,
            DelegateDeclarationSyntax nestedDelegate => nestedDelegate.Identifier.Text,
            _ => "member"
        };
    }
}
