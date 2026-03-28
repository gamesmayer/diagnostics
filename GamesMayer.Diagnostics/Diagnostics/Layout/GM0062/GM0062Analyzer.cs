using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0062Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0062";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use 'is or' syntax for multiple type checks on the same expression",
            messageFormat: "Use 'is or' syntax instead of '||' for multiple type checks on the same expression",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Type checking expressions that test the same variable against multiple types using '||' should use the 'is or' syntax instead.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeLogicalOr, SyntaxKind.LogicalOrExpression);
        }

        private static void AnalyzeLogicalOr(SyntaxNodeAnalysisContext context)
        {
            var logicalOr = (BinaryExpressionSyntax)context.Node;

            // Only handle root-level || (not nested inside another ||)
            if (logicalOr.Parent is BinaryExpressionSyntax parentBinary &&
                parentBinary.IsKind(SyntaxKind.LogicalOrExpression))
                return;

            var operands = new List<ExpressionSyntax>();
            CollectOrOperands(logicalOr, operands);

            string? commonSubject = null;
            foreach (var operand in operands)
            {
                if (!TryGetIsTypeCheckSubject(operand, context.SemanticModel, out var subject))
                    return;

                var subjectText = subject!.ToString().Trim();
                if (commonSubject == null)
                    commonSubject = subjectText;
                else if (commonSubject != subjectText)
                    return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, logicalOr.GetLocation()));
        }

        internal static void CollectOrOperands(ExpressionSyntax expr, List<ExpressionSyntax> operands)
        {
            if (expr is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.LogicalOrExpression))
            {
                CollectOrOperands(binary.Left, operands);
                CollectOrOperands(binary.Right, operands);
            }
            else
            {
                operands.Add(expr);
            }
        }

        internal static bool TryGetIsTypeCheckSubject(
            ExpressionSyntax expr,
            SemanticModel semanticModel,
            out ExpressionSyntax? subject)
        {
            // n is TypeA  (IsPatternExpressionSyntax with TypePatternSyntax)
            if (expr is IsPatternExpressionSyntax isPattern &&
                isPattern.Pattern is TypePatternSyntax)
            {
                subject = isPattern.Expression;
                return true;
            }

            // n is TypeA  (BinaryExpressionSyntax, when the right looks like a type name)
            if (expr is BinaryExpressionSyntax binary &&
                binary.IsKind(SyntaxKind.IsExpression))
            {
                var symbolInfo = semanticModel.GetSymbolInfo(binary.Right);
                if (symbolInfo.Symbol is ITypeSymbol)
                {
                    subject = binary.Left;
                    return true;
                }
            }

            subject = null;
            return false;
        }
    }
}
