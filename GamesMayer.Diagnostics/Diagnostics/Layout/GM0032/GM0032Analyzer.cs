using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0032Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0032";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Single-line parameter or argument list must be on the declaration line",
            messageFormat: "Move the single-line parameter or argument list to the declaration line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Declarations and invocations with a non-empty single-line argument or parameter list must have the opening parenthesis written on the same line as the declaration identifier.");

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
            if (argumentList.Arguments.Count == 0)
            {
                return;
            }

            AnalyzeList(
                context,
                argumentList.Arguments[0],
                argumentList.Arguments[argumentList.Arguments.Count - 1],
                argumentList.OpenParenToken,
                argumentList.CloseParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            if (parameterList.Parameters.Count == 0)
            {
                return;
            }

            if (parameterList.Parent is ParenthesizedLambdaExpressionSyntax ||
                parameterList.Parent is AnonymousMethodExpressionSyntax)
            {
                return;
            }

            AnalyzeList(
                context,
                parameterList.Parameters[0],
                parameterList.Parameters[parameterList.Parameters.Count - 1],
                parameterList.OpenParenToken,
                parameterList.CloseParenToken);
        }

        private static void AnalyzeList<TNode>(
            SyntaxNodeAnalysisContext context,
            TNode firstItem,
            TNode lastItem,
            SyntaxToken openParenToken,
            SyntaxToken closeParenToken)
            where TNode : SyntaxNode
        {
            var tree = context.Node.SyntaxTree;
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;
            if (openParenLine != closeParenLine)
            {
                return;
            }

            var firstItemLine = tree.GetLineSpan(firstItem.GetFirstToken().Span).StartLinePosition.Line;
            var lastItemLine = tree.GetLineSpan(lastItem.GetLastToken().Span).EndLinePosition.Line;
            if (firstItemLine != openParenLine || lastItemLine != openParenLine)
            {
                return;
            }

            var previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return;
            }

            var declarationLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
            if (openParenLine == declarationLine)
            {
                return;
            }

            var location = Location.Create(
                tree,
                TextSpan.FromBounds(previousToken.Span.Start, closeParenToken.Span.End));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}