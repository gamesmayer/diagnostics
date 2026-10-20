using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics.Utils
{
    public static class ControlClauseUtils
    {
        /// <summary>
        /// Whether the node sits in a control structure clause declaration (from the keyword to the closing
        /// parenthesis), which GM0017 keeps on a single line. Rules that split code into lines skip it there:
        /// a long condition is extracted into a local variable instead.
        /// </summary>
        public static bool IsInClauseHeader(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                if (ancestor is StatementSyntax || ancestor is CatchClauseSyntax)
                {
                    var header = GetHeaderSpan(ancestor);

                    return header.HasValue && header.Value.Contains(node.Span);
                }
            }

            return false;
        }

        private static TextSpan? GetHeaderSpan(SyntaxNode node)
        {
            switch (node)
            {
                case IfStatementSyntax ifStatement:
                    return TextSpan.FromBounds(ifStatement.IfKeyword.SpanStart, ifStatement.CloseParenToken.Span.End);
                case WhileStatementSyntax whileStatement:
                    return TextSpan.FromBounds(whileStatement.WhileKeyword.SpanStart, whileStatement.CloseParenToken.Span.End);
                case DoStatementSyntax doStatement:
                    return TextSpan.FromBounds(doStatement.WhileKeyword.SpanStart, doStatement.CloseParenToken.Span.End);
                case ForStatementSyntax forStatement:
                    return TextSpan.FromBounds(forStatement.ForKeyword.SpanStart, forStatement.CloseParenToken.Span.End);
                case CommonForEachStatementSyntax forEachStatement:
                    return TextSpan.FromBounds(forEachStatement.ForEachKeyword.SpanStart, forEachStatement.CloseParenToken.Span.End);
                case SwitchStatementSyntax switchStatement when !switchStatement.CloseParenToken.IsMissing:
                    return TextSpan.FromBounds(switchStatement.SwitchKeyword.SpanStart, switchStatement.CloseParenToken.Span.End);
                case LockStatementSyntax lockStatement:
                    return TextSpan.FromBounds(lockStatement.LockKeyword.SpanStart, lockStatement.CloseParenToken.Span.End);
                case UsingStatementSyntax usingStatement:
                    return TextSpan.FromBounds(usingStatement.UsingKeyword.SpanStart, usingStatement.CloseParenToken.Span.End);
                case CatchClauseSyntax catchClause when catchClause.Filter != null:
                    return TextSpan.FromBounds(catchClause.CatchKeyword.SpanStart, catchClause.Filter.CloseParenToken.Span.End);
                case CatchClauseSyntax catchClause when catchClause.Declaration != null:
                    return TextSpan.FromBounds(catchClause.CatchKeyword.SpanStart, catchClause.Declaration.CloseParenToken.Span.End);
                default:
                    return null;
            }
        }
    }
}
