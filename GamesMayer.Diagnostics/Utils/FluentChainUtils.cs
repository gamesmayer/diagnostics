using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics.Utils
{
    public static class FluentChainUtils
    {
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0041.threshold";
        private const int DefaultMinInvocations = 2;

        public static int GetFluentChainThreshold(AnalyzerConfigOptions? options)
        {
            if (options != null
                && options.TryGetValue(ThresholdOptionKey, out var rawValue)
                && int.TryParse(rawValue, out var parsed)
                && parsed > 1)
            {
                return parsed;
            }

            return DefaultMinInvocations;
        }


        public static ExpressionSyntax GetChainRoot(ExpressionSyntax expression)
        {
            var current = expression;
            while (true)
            {
                switch (current)
                {
                    case ParenthesizedExpressionSyntax p:
                        current = p.Expression;
                        continue;
                    case AwaitExpressionSyntax a:
                        current = a.Expression;
                        continue;
                    case AssignmentExpressionSyntax ae:
                        current = ae.Right;
                        continue;
                    default:
                        return current;
                }
            }
        }

        public static bool IsFluentChainExpression(ExpressionSyntax expression)
        {
            return expression is MemberAccessExpressionSyntax
                || (expression is InvocationExpressionSyntax invocation
                    && invocation.Expression is MemberAccessExpressionSyntax);
        }

        public static bool IsFluentChainStart(ExpressionSyntax expression)
        {
            if (!IsFluentChainExpression(expression))
                return false;

            if (expression.Parent is InvocationExpressionSyntax parentInvocation
                && parentInvocation.Expression == expression)
                return false;

            if (expression.Parent is MemberAccessExpressionSyntax parentMemberAccess
                && parentMemberAccess.Expression == expression)
                return false;

            return true;
        }

        public static void CollectFluentChainBoundaries(
            ExpressionSyntax expression,
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax invMemberAccess)
            {
                CollectFluentChainBoundaries(invMemberAccess.Expression, boundaries);
                boundaries.Add((invMemberAccess.Expression, invMemberAccess.OperatorToken, invocation));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                CollectFluentChainBoundaries(memberAccess.Expression, boundaries);
                boundaries.Add((memberAccess.Expression, memberAccess.OperatorToken, memberAccess));
            }
        }

        public static int CountChainInvocations(ExpressionSyntax expression)
        {
            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(expression, boundaries);

            int count = 0;
            foreach (var boundary in boundaries)
            {
                if (boundary.SegmentExpression is InvocationExpressionSyntax)
                    count++;
            }

            return count;
        }
    }
}
