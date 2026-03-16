using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0034Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0034";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "First item in multi-line list must start below opening parenthesis",
            messageFormat: "Move the first argument or parameter to a new line below the opening parenthesis",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line argument or parameter list, the first item must not be on the same line as the opening parenthesis.");

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

            // The rule only applies to multi-line lists.
            if (openParenLine == closeParenLine)
            {
                return;
            }

            var firstItem = items[0];
            var firstItemToken = firstItem.GetFirstToken();
            if (firstItemToken == default)
            {
                return;
            }

            var firstItemLine = tree.GetLineSpan(firstItemToken.Span).StartLinePosition.Line;
            if (firstItemLine != openParenLine)
            {
                return;
            }

            var diagnosticSpan = GetDiagnosticSpan(items, firstItem);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, diagnosticSpan)));
        }

        private static TextSpan GetDiagnosticSpan<TNode>(SeparatedSyntaxList<TNode> items, TNode firstItem)
            where TNode : SyntaxNode
        {
            var itemIndex = items.IndexOf(firstItem);
            var start = GetItemStart(firstItem);

            if (itemIndex < items.Count - 1)
            {
                var trailingSeparator = items.GetSeparator(itemIndex);
                return TextSpan.FromBounds(start, trailingSeparator.Span.End);
            }

            return new TextSpan(start, firstItem.GetFirstToken().Span.Length);
        }

        private static int GetItemStart(SyntaxNode item)
        {
            if (item is ParameterSyntax parameter)
            {
                if (parameter.Type is { } type)
                {
                    return type.SpanStart;
                }

                return parameter.Identifier.SpanStart;
            }

            if (item is ArgumentSyntax argument)
            {
                if (argument.NameColon is { } nameColon)
                {
                    return nameColon.Name.SpanStart;
                }

                var expressionToken = argument.Expression.GetFirstToken();
                if (expressionToken != default)
                {
                    return expressionToken.SpanStart;
                }
            }

            return item.GetFirstToken().SpanStart;
        }
    }
}
