using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics.Utils
{
    public static class ComplexExpressionUtils
    {
        public static bool IsComplexExpression(ExpressionSyntax expr, AnalyzerConfigOptions? options = null)
        {
            if (expr is LambdaExpressionSyntax)
                return true;

            if (expr is AnonymousMethodExpressionSyntax)
                return true;

            if (expr is AnonymousObjectCreationExpressionSyntax)
                return true;

            if (expr is ObjectCreationExpressionSyntax objCreation && objCreation.Initializer != null)
                return true;

            if (expr is ImplicitArrayCreationExpressionSyntax)
                return true;

            if (expr is ArrayCreationExpressionSyntax arrayCreation && arrayCreation.Initializer != null)
                return true;


            if (IsComplexFluentChain(expr, options))
                return true;

            if (expr is InvocationExpressionSyntax invocation
                && invocation.ArgumentList.Arguments.Any(arg => IsComplexExpression(arg.Expression, options)))
                return true;

            if (expr is ObjectCreationExpressionSyntax objCreationWithComplexArg
                && objCreationWithComplexArg.ArgumentList != null
                && objCreationWithComplexArg.ArgumentList.Arguments.Any(arg => IsComplexExpression(arg.Expression, options)))
                return true;

            if (expr is BinaryExpressionSyntax binary
                && (IsComplexExpression(binary.Left, options) || IsComplexExpression(binary.Right, options)))
                return true;

            return false;
        }

        private static bool IsComplexFluentChain(ExpressionSyntax expr, AnalyzerConfigOptions? options)
        {
            int threshold = FluentChainUtils.GetFluentChainThreshold(options);
            return FluentChainUtils.CountChainInvocations(expr) >= threshold;
        }
    }
}
