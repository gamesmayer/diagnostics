using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0031Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0031";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Empty parentheses must be on the same line as the declaration",
            messageFormat: "Move the empty parentheses to the same line as the declaration",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Declarations and invocations without arguments or parameters must have the empty parentheses written on the same line as the declaration identifier.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
        }

        private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var argumentList = (ArgumentListSyntax)context.Node;
            if (argumentList.Arguments.Count > 0)
            {
                return;
            }

            AnalyzeList(context, argumentList.OpenParenToken, argumentList.CloseParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            if (parameterList.Parameters.Count > 0)
            {
                return;
            }

            // Lambda and anonymous method parameter lists are not bound to a named declaration,
            // so we only care whether ( and ) are on the same line.
            if (parameterList.Parent is ParenthesizedLambdaExpressionSyntax ||
                parameterList.Parent is AnonymousMethodExpressionSyntax)
            {
                var tree = context.Node.SyntaxTree;
                var openLine = tree.GetLineSpan(parameterList.OpenParenToken.Span).StartLinePosition.Line;
                var closeLine = tree.GetLineSpan(parameterList.CloseParenToken.Span).StartLinePosition.Line;
                if (openLine != closeLine)
                {
                    var location = Location.Create(
                        tree,
                        TextSpan.FromBounds(parameterList.OpenParenToken.Span.Start, parameterList.CloseParenToken.Span.End));
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                }
                return;
            }

            AnalyzeList(context, parameterList.OpenParenToken, parameterList.CloseParenToken);
        }

        private static void AnalyzeList(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openParenToken,
            SyntaxToken closeParenToken)
        {
            var tree = context.Node.SyntaxTree;
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;

            var previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return;
            }

            var declarationLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;

            if (openParenLine != declarationLine || openParenLine != closeParenLine)
            {
                var location = Location.Create(
                    tree,
                    TextSpan.FromBounds(previousToken.Span.Start, closeParenToken.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
            }
        }
    }
}
