using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0144Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0144";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use null-conditional operator",
            messageFormat: "Use '?.' instead of explicit null check",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Simplify null checks by using the null-conditional operator '?.'.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ConditionalExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var conditional = (ConditionalExpressionSyntax)context.Node;

            if (TryGetNullConditionalPattern(conditional, out _, out _))
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, conditional.GetLocation()));
        }

        internal static bool TryGetNullConditionalPattern(
            ConditionalExpressionSyntax conditional,
            out ExpressionSyntax checkedExpression,
            out ExpressionSyntax memberAccess)
        {
            checkedExpression = null!;
            memberAccess = null!;

            if (conditional.Condition is not BinaryExpressionSyntax condition)
                return false;

            bool isNotNull = condition.IsKind(SyntaxKind.NotEqualsExpression);
            bool isNull = condition.IsKind(SyntaxKind.EqualsExpression);

            if (!isNotNull && !isNull)
                return false;

            ExpressionSyntax? nonNullOperand = null;

            if (IsNullLiteral(condition.Right))
                nonNullOperand = condition.Left;
            else if (IsNullLiteral(condition.Left))
                nonNullOperand = condition.Right;

            if (nonNullOperand == null)
                return false;

            // x != null ? x.M : null   OR   x == null ? null : x.M
            ExpressionSyntax memberAccessCandidate = isNotNull ? conditional.WhenTrue : conditional.WhenFalse;
            ExpressionSyntax nullBranch = isNotNull ? conditional.WhenFalse : conditional.WhenTrue;

            if (!IsNullLiteral(nullBranch))
                return false;

            if (!IsMemberAccessOn(memberAccessCandidate, nonNullOperand))
                return false;

            checkedExpression = nonNullOperand;
            memberAccess = memberAccessCandidate;
            return true;
        }

        private static bool IsNullLiteral(ExpressionSyntax expr) =>
            expr.IsKind(SyntaxKind.NullLiteralExpression);

        private static bool IsMemberAccessOn(ExpressionSyntax expr, ExpressionSyntax target)
        {
            ExpressionSyntax? accessedOn = expr switch
            {
                MemberAccessExpressionSyntax ma => ma.Expression,
                InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax m } => m.Expression,
                ElementAccessExpressionSyntax ea => ea.Expression,
                _ => null
            };

            if (accessedOn == null)
                return false;

            return SyntaxFactory.AreEquivalent(accessedOn, target);
        }
    }
}
