using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0137Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0137";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cast expression and its operand must be on the same line",
            messageFormat: "Move the operand to the same line as the cast expression",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The cast expression closing parenthesis and its operand must be on the same line with no line breaks between them.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CastExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var castExpr = (CastExpressionSyntax)context.Node;
            var tree = context.Node.SyntaxTree;

            var closeParenLine = tree.GetLineSpan(castExpr.CloseParenToken.Span).EndLinePosition.Line;
            var expressionLine = tree.GetLineSpan(castExpr.Expression.GetFirstToken().Span).StartLinePosition.Line;

            if (closeParenLine != expressionLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, castExpr.GetLocation()));
            }
        }
    }
}
