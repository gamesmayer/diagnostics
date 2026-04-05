using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0044Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0044";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank line between lambda arrow and body",
            messageFormat: "Remove the blank line between '=>' and the lambda body",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Lambda expressions must not contain blank lines between the => token and the body.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
        {
            var lambda = (LambdaExpressionSyntax)context.Node;
            var bodyFirstToken = lambda.Body.GetFirstToken();
            if (bodyFirstToken == default)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            if (!BlankLineDetectionUtils.TryGetFirstBlankLineStart(sourceText, tree, lambda.ArrowToken, bodyFirstToken, out var blankLineStart))
                return;

            var location = Location.Create(tree, new TextSpan(blankLineStart, 0));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
