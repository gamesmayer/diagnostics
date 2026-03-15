using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0026Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0026";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Closing parenthesis in multi-line list must be on a dedicated line",
            messageFormat: "Place the closing parenthesis on the line below the last argument or parameter",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In multi-line argument or parameter lists, the closing parenthesis must be on its own line directly below the last item.");

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
            AnalyzeList(context, argumentList.Arguments, argumentList.OpenParenToken, argumentList.CloseParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            AnalyzeList(context, parameterList.Parameters, parameterList.OpenParenToken, parameterList.CloseParenToken);
        }

        private static void AnalyzeList<TNode>(
            SyntaxNodeAnalysisContext context,
            SeparatedSyntaxList<TNode> items,
            SyntaxToken openParenToken,
            SyntaxToken closeParenToken)
            where TNode : SyntaxNode
        {
            if (items.Count == 0)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;

            // The rule applies only to multi-line lists.
            if (openParenLine == closeParenLine)
            {
                return;
            }

            var lastItem = items[items.Count - 1];
            var lastItemLine = tree.GetLineSpan(lastItem.Span).EndLinePosition.Line;
            var expectedCloseParenLine = lastItemLine + 1;

            if (closeParenLine != expectedCloseParenLine)
            {
                // Report only the ')' token as requested.
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, closeParenToken.GetLocation()));
            }
        }
    }
}