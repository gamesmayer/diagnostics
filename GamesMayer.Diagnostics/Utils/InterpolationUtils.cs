using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GamesMayer.Diagnostics.Utils
{
    public static class InterpolationUtils
    {
        /// <summary>
        /// Whether the node sits in an interpolation hole that must stay on one line. Before C# 11 a hole of a regular
        /// <c>$"..."</c> string can't span lines (CS8967), and Unity compiles C# 9; a single-line raw string never can.
        /// Rules that split code into lines skip it there: a long expression is extracted into a local variable instead.
        /// </summary>
        public static bool IsInSingleLineHole(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                if (ancestor is InterpolatedStringExpressionSyntax interpolated)
                {
                    var start = interpolated.StringStartToken;

                    return !start.IsKind(SyntaxKind.InterpolatedVerbatimStringStartToken)
                        && !start.IsKind(SyntaxKind.InterpolatedMultiLineRawStringStartToken);
                }
            }

            return false;
        }
    }
}
