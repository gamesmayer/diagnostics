using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0048Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0048";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Expression body must start on the same line as '=>'",
            messageFormat: "Move the expression body to start on the same line as '=>'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The body of a lambda expression or expression-bodied member must begin on the same line as the '=>' token.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.ParenthesizedLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeArrowExpressionClause, SyntaxKind.ArrowExpressionClause);
        }

        private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;

            if (lambda.Body is BlockSyntax)
                return;

            var tree = lambda.SyntaxTree;

            var arrowToken = lambda.ArrowToken;
            var bodyFirstToken = lambda.Body.GetFirstToken();

            if (bodyFirstToken == default)
                return;

            var arrowLine = tree.GetLineSpan(arrowToken.Span).EndLinePosition.Line;
            var bodyLine = tree.GetLineSpan(bodyFirstToken.Span).StartLinePosition.Line;

            if (bodyLine != arrowLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    lambda.Body.GetLocation()));
            }
        }

        private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            var arrowClause = (ArrowExpressionClauseSyntax)context.Node;
            var tree = arrowClause.SyntaxTree;

            var arrowToken = arrowClause.ArrowToken;
            var bodyFirstToken = arrowClause.Expression.GetFirstToken();

            if (bodyFirstToken == default)
                return;

            var arrowLine = tree.GetLineSpan(arrowToken.Span).EndLinePosition.Line;
            var bodyLine = tree.GetLineSpan(bodyFirstToken.Span).StartLinePosition.Line;

            if (bodyLine != arrowLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    arrowClause.Expression.GetLocation()));
            }
        }
    }
}
