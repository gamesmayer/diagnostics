using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0138Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0138";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Lambda parameters and '=>' must be on the same line",
            messageFormat: "Move '=>' to the same line as the lambda parameters",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The lambda parameters and the '=>' token must be on the same line with no line breaks between them.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ParenthesizedLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleLambdaExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;
            var tree = context.Node.SyntaxTree;

            var parametersLastToken = lambda switch
            {
                ParenthesizedLambdaExpressionSyntax p => p.ParameterList.CloseParenToken,
                SimpleLambdaExpressionSyntax s => s.Parameter.GetLastToken(),
                _ => default
            };

            if (parametersLastToken == default)
                return;

            var parametersLine = tree.GetLineSpan(parametersLastToken.Span).EndLinePosition.Line;
            var arrowLine = tree.GetLineSpan(lambda.ArrowToken.Span).StartLinePosition.Line;

            if (parametersLine != arrowLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, lambda.GetLocation()));
            }
        }
    }
}
