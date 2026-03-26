using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0045Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0045";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Lambda expression body must be indented one step from the arrow line",
            messageFormat: "Indent the lambda body one step from the '=>' line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When a lambda expression body (without braces) is on a different line from the '=>' token, it must be indented exactly one step (4 spaces) more than the leading whitespace of the '=>' line.");

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

            if (lambda.Body is BlockSyntax)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var arrowLine = tree.GetLineSpan(lambda.ArrowToken.Span).EndLinePosition.Line;
            var bodyFirstToken = lambda.Body.GetFirstToken();
            if (bodyFirstToken == default)
                return;

            var bodyLine = tree.GetLineSpan(bodyFirstToken.Span).StartLinePosition.Line;

            if (bodyLine == arrowLine)
                return;

            var arrowLineText = sourceText.Lines[arrowLine].ToString();
            int arrowIndent = CountLeadingWhitespace(arrowLineText);
            int expectedIndent = arrowIndent + 4;

            var bodyLineText = sourceText.Lines[bodyLine].ToString();
            int actualIndent = CountLeadingWhitespace(bodyLineText);

            if (actualIndent != expectedIndent)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, lambda.Body.Span)));
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
        }
    }
}
