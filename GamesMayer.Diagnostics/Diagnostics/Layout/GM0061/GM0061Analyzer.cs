using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0061Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0061";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Use '==' or '!=' instead of 'is' pattern for equality comparison",
            messageFormat: "Use '{0}' instead of '{1}'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Equality comparisons against null or constant values should use '==' or '!=' instead of the 'is' pattern.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Handles: x is null, x is not null, x is not Phase.Began, x is not MyConst
            context.RegisterSyntaxNodeAction(AnalyzeIsPattern, SyntaxKind.IsPatternExpression);

            // Handles: x is Phase.Began, x is MyConst
            // These are parsed as BinaryExpressionSyntax(IsExpression) because the right-hand
            // side looks syntactically like a type name, not a pattern expression.
            context.RegisterSyntaxNodeAction(AnalyzeBinaryIsExpression, SyntaxKind.IsExpression);
        }

        private static void AnalyzeIsPattern(SyntaxNodeAnalysisContext context)
        {
            var isPatternExpr = (IsPatternExpressionSyntax)context.Node;

            // x is A or B or C → x == A || x == B || x == C
            if (isPatternExpr.Pattern is BinaryPatternSyntax orPattern &&
                orPattern.IsKind(SyntaxKind.OrPattern) &&
                IsAllConstantOrPattern(orPattern, context.SemanticModel))
            {
                var orProperties = ImmutableDictionary<string, string?>.Empty
                    .Add("IsNegated", "false")
                    .Add("IsBinaryIs", "false");

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    isPatternExpr.GetLocation(),
                    orProperties,
                    "==",
                    "is"));
                return;
            }

            bool isNegated;
            PatternSyntax innerPattern;

            if (isPatternExpr.Pattern is UnaryPatternSyntax unary &&
                unary.OperatorToken.IsKind(SyntaxKind.NotKeyword))
            {
                isNegated = true;
                innerPattern = unary.Pattern;
            }
            else
            {
                isNegated = false;
                innerPattern = isPatternExpr.Pattern;
            }

            if (innerPattern is not ConstantPatternSyntax)
                return;

            var properties = ImmutableDictionary<string, string?>.Empty
                .Add("IsNegated", isNegated ? "true" : "false")
                .Add("IsBinaryIs", "false");

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                isPatternExpr.GetLocation(),
                properties,
                isNegated ? "!=" : "==",
                isNegated ? "is not" : "is"));
        }

        private static bool IsAllConstantOrPattern(PatternSyntax pattern, SemanticModel semanticModel)
        {
            if (pattern is BinaryPatternSyntax bp && bp.IsKind(SyntaxKind.OrPattern))
                return IsAllConstantOrPattern(bp.Left, semanticModel) && IsAllConstantOrPattern(bp.Right, semanticModel);

            if (pattern is ConstantPatternSyntax cp)
            {
                if (cp.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                    return true;

                var symbol = semanticModel.GetSymbolInfo(cp.Expression).Symbol;
                return symbol is IFieldSymbol field &&
                       (field.IsConst || field.ContainingType?.TypeKind == TypeKind.Enum);
            }

            return false;
        }

        private static void AnalyzeBinaryIsExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;

            var symbol = context.SemanticModel.GetSymbolInfo(binary.Right).Symbol;
            if (symbol is not IFieldSymbol field)
                return;

            if (!field.IsConst && field.ContainingType?.TypeKind != TypeKind.Enum)
                return;

            var properties = ImmutableDictionary<string, string?>.Empty
                .Add("IsNegated", "false")
                .Add("IsBinaryIs", "true");

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                binary.GetLocation(),
                properties,
                "==",
                "is"));
        }
    }
}
