using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0046Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0046";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Logical operator in multi-line expression must be at the end of the previous line",
            messageFormat: "Move the '{0}' operator to the end of the previous line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line logical expression, operators (&&, ||) must appear at the end of the previous operand's line, not at the beginning of the next line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.LogicalOrExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.LogicalAndExpression);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;

            if (IsNestedInSameOperator(binary))
                return;

            var operands = new List<(ExpressionSyntax Operand, SyntaxToken? OperatorBefore)>();
            CollectOperands(binary, binary.Kind(), operands);

            var tree = context.Node.SyntaxTree;

            for (int i = 1; i < operands.Count; i++)
            {
                var (_, operatorToken) = operands[i];
                if (operatorToken == null)
                    continue;

                var opToken = operatorToken.Value;
                var prevLastToken = operands[i - 1].Operand.GetLastToken();
                var prevLastTokenLine = tree.GetLineSpan(prevLastToken.Span).EndLinePosition.Line;
                var operatorLine = tree.GetLineSpan(opToken.Span).StartLinePosition.Line;

                if (operatorLine != prevLastTokenLine)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptor,
                        Location.Create(tree, opToken.Span),
                        opToken.Text));
                }
            }
        }

        private static bool IsNestedInSameOperator(BinaryExpressionSyntax binary)
        {
            return binary.Parent is BinaryExpressionSyntax parent && parent.Kind() == binary.Kind();
        }

        private static void CollectOperands(
            ExpressionSyntax expression,
            SyntaxKind operatorKind,
            List<(ExpressionSyntax Operand, SyntaxToken? OperatorBefore)> operands)
        {
            if (expression is BinaryExpressionSyntax binary && binary.Kind() == operatorKind)
            {
                CollectOperands(binary.Left, operatorKind, operands);
                operands.Add((binary.Right, binary.OperatorToken));
            }
            else
            {
                operands.Add((expression, null));
            }
        }
    }
}
