using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0025Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0025";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each argument or parameter must be on its own line in a multi-line list",
            messageFormat: "Place this argument or parameter on its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line argument or parameter list, each item must be placed on its own dedicated line.");

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
            if (items.Count < 2)
                return;

            var tree = context.Node.SyntaxTree;

            // Only apply rule when the list spans multiple lines
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;

            if (openParenLine == closeParenLine)
                return;

            for (int i = 1; i < items.Count; i++)
            {
                var currentItem = items[i];
                var previousItem = items[i - 1];

                var currentFirstToken = currentItem.GetFirstToken();
                var previousFirstToken = previousItem.GetFirstToken();

                if (currentFirstToken == default || previousFirstToken == default)
                    continue;

                var currentLine = tree.GetLineSpan(currentFirstToken.Span).StartLinePosition.Line;
                var previousLine = tree.GetLineSpan(previousFirstToken.Span).StartLinePosition.Line;

                if (currentLine != previousLine)
                    continue;

                // Violation: currentItem shares a line with the previous item.
                // Span covers the item and its trailing separator (if not the last item).
                bool isLastItem = i == items.Count - 1;
                Location location;

                if (!isLastItem)
                {
                    var trailingSeparator = items.GetSeparator(i);
                    location = Location.Create(
                        tree,
                        TextSpan.FromBounds(currentItem.Span.Start, trailingSeparator.Span.End));
                }
                else
                {
                    location = currentItem.GetLocation();
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
            }
        }
    }
}
