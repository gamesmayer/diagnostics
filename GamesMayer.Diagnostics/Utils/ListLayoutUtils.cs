using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GamesMayer.Diagnostics.Utils
{
    public static class ListLayoutUtils
    {
        /// <summary>
        /// Whether an argument or parameter list is written inline: no line break between the opening parenthesis
        /// and the first item, between items, or between the last item and the closing parenthesis. The list can
        /// still span lines when an item wraps itself (Assert.True(a ||\n    b)); the parenthesis rules leave it
        /// alone, because GM0127 decides when a list has to be split. A list with complex arguments is never inline.
        /// </summary>
        public static bool IsInlineList<TNode>(SeparatedSyntaxList<TNode> items, SyntaxToken openParenToken, SyntaxToken closeParenToken)
            where TNode : SyntaxNode
        {
            if (items.Count == 0)
                return false;

            // Lambdas, initializers and other complex arguments always split the list (GM0127).
            if (items is SeparatedSyntaxList<ArgumentSyntax> arguments && ComplexExpressionUtils.IsComplexArgumentList(arguments))
                return false;

            var tree = openParenToken.SyntaxTree;

            if (tree == null)
                return false;

            int Line(int position) => tree.GetLineSpan(new Microsoft.CodeAnalysis.Text.TextSpan(position, 0)).StartLinePosition.Line;

            if (Line(openParenToken.Span.End) != Line(items[0].SpanStart))
                return false;

            for (var i = 1; i < items.Count; i++)
            {
                if (Line(items[i - 1].Span.End) != Line(items[i].SpanStart))
                    return false;
            }

            return Line(items[items.Count - 1].Span.End) == Line(closeParenToken.SpanStart);
        }
    }
}
