using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0043Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0043";
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0043.threshold";
        private const int DefaultMinOperands = 3;

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each operand in a multi-operand logical expression must be on its own line",
            messageFormat: "Move this operand to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a logical expression with at least the configured threshold of operands, every operand must be on its own line for clarity.");

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

            int minOperands = GetMinimumOperands(context);
            if (operands.Count < minOperands)
                return;

            var tree = context.Node.SyntaxTree;

            for (int i = 1; i < operands.Count; i++)
            {
                var (operand, operatorToken) = operands[i];
                if (operatorToken == null)
                    continue;

                var prevLastToken = operands[i - 1].Operand.GetLastToken();
                var prevLine = tree.GetLineSpan(prevLastToken.Span).EndLinePosition.Line;
                var operandLine = tree.GetLineSpan(operand.Span).StartLinePosition.Line;

                if (operandLine == prevLine)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, operand.Span)));
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

        private static int GetMinimumOperands(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue(ThresholdOptionKey, out var rawValue)
                && int.TryParse(rawValue, out var parsed)
                && parsed > 1)
            {
                return parsed;
            }

            return DefaultMinOperands;
        }
    }
}
