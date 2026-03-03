using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NewLineAfterAttributesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0004";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Member declaration must be on a new line after its attributes",
            messageFormat: "'{0}' must be on a new line after its attributes",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Member declarations must start on a new line following their attributes.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.EnumMemberDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.DelegateDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var member = (MemberDeclarationSyntax)context.Node;

            if (member.AttributeLists.Count == 0)
                return;

            var lastAttrToken = member.AttributeLists.Last().GetLastToken();
            var firstMemberToken = lastAttrToken.GetNextToken();

            var attrLine = lastAttrToken.GetLocation().GetLineSpan().EndLinePosition.Line;
            var memberLine = firstMemberToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (attrLine == memberLine)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Descriptor, firstMemberToken.GetLocation(), firstMemberToken.Text));
            }
        }
    }
}
