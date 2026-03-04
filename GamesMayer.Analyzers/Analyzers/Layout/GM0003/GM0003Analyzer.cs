using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0003Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0003";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Attribute must be on the line immediately before the member",
            messageFormat: "Attribute on '{0}' must be on the line immediately before the member with no blank lines",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Attributes must appear on the line directly preceding the member they annotate, with no blank lines in between.");

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

            var firstMemberToken = member.AttributeLists.Last().GetLastToken().GetNextToken();

            foreach (var trivia in firstMemberToken.LeadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(Descriptor, trivia.GetLocation(), GetMemberName(member)));
                    return;
                }
            }
        }

        private static string GetMemberName(MemberDeclarationSyntax member) => member switch
        {
            PropertyDeclarationSyntax p => p.Identifier.Text,
            MethodDeclarationSyntax m => m.Identifier.Text,
            FieldDeclarationSyntax f => f.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "field",
            EnumMemberDeclarationSyntax em => em.Identifier.Text,
            BaseTypeDeclarationSyntax t => t.Identifier.Text,
            ConstructorDeclarationSyntax c => c.Identifier.Text,
            DestructorDeclarationSyntax d => "~" + d.Identifier.Text,
            EventDeclarationSyntax e => e.Identifier.Text,
            EventFieldDeclarationSyntax ef => ef.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "event",
            IndexerDeclarationSyntax _ => "this",
            DelegateDeclarationSyntax d => d.Identifier.Text,
            OperatorDeclarationSyntax op => "operator " + op.OperatorToken.Text,
            _ => "member",
        };
    }
}
