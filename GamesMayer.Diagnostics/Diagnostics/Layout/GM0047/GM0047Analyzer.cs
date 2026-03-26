using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0047Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0047";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Multi-line assignment value must start on the same line as the assignment operator",
            messageFormat: "Move the assignment value to start on the same line as '='",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line assignment, the right-hand side value must begin on the same line as the '=' operator.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
            context.RegisterSyntaxNodeAction(AnalyzeEqualsValueClause, SyntaxKind.EqualsValueClause);
        }

        private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            var tree = assignment.SyntaxTree;

            var equalsToken = assignment.OperatorToken;
            var valueFirstToken = assignment.Right.GetFirstToken();

            if (valueFirstToken.IsKind(SyntaxKind.OpenBraceToken))
                return;

            var equalsLine = tree.GetLineSpan(equalsToken.Span).EndLinePosition.Line;
            var valueLine = tree.GetLineSpan(valueFirstToken.Span).StartLinePosition.Line;

            if (valueLine != equalsLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    assignment.Right.GetLocation()));
            }
        }

        private static void AnalyzeEqualsValueClause(SyntaxNodeAnalysisContext context)
        {
            var equalsValueClause = (EqualsValueClauseSyntax)context.Node;

            if (equalsValueClause.Parent is ParameterSyntax || equalsValueClause.Parent is EnumMemberDeclarationSyntax)
                return;

            var tree = equalsValueClause.SyntaxTree;

            var equalsToken = equalsValueClause.EqualsToken;
            var valueFirstToken = equalsValueClause.Value.GetFirstToken();

            if (valueFirstToken.IsKind(SyntaxKind.OpenBraceToken))
                return;

            var equalsLine = tree.GetLineSpan(equalsToken.Span).EndLinePosition.Line;
            var valueLine = tree.GetLineSpan(valueFirstToken.Span).StartLinePosition.Line;

            if (valueLine != equalsLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    equalsValueClause.Value.GetLocation()));
            }
        }
    }
}
