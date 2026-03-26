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

        private static readonly SyntaxKind[] BinaryExpressionKinds =
        {
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.CoalesceExpression,
        };

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Binary operator in multi-line expression must be at the end of the previous line",
            messageFormat: "Move the '{0}' operator to the end of the previous line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line expression, binary operators must appear at the end of the previous operand's line, not at the beginning of the next line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, BinaryExpressionKinds);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            var tree = binary.SyntaxTree;

            var prevLastToken = binary.Left.GetLastToken();
            var operatorToken = binary.OperatorToken;

            var prevLastTokenLine = tree.GetLineSpan(prevLastToken.Span).EndLinePosition.Line;
            var operatorLine = tree.GetLineSpan(operatorToken.Span).StartLinePosition.Line;

            if (operatorLine != prevLastTokenLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    Location.Create(tree, operatorToken.Span),
                    operatorToken.Text));
            }
        }
    }
}
