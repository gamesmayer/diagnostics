using System.Linq;
using Microsoft.CodeAnalysis;
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
                && IsComplexArgumentList(invocation.ArgumentList.Arguments, options))
                return true;

            if (expr is ObjectCreationExpressionSyntax objCreationWithComplexArg
                && objCreationWithComplexArg.ArgumentList != null
                && IsComplexArgumentList(objCreationWithComplexArg.ArgumentList.Arguments, options))
                return true;

            if (expr is BinaryExpressionSyntax binary
                && (IsComplexExpression(binary.Left, options) || IsComplexExpression(binary.Right, options)))
                return true;

            return false;
        }

        public static bool IsComplexArgumentList(SeparatedSyntaxList<ArgumentSyntax> arguments, AnalyzerConfigOptions? options = null)
        {
            return arguments.Any(arg => arg.NameColon != null || IsComplexExpression(arg.Expression, options));
        }

        private static bool IsComplexFluentChain(ExpressionSyntax expr, AnalyzerConfigOptions? options)
        {
            int threshold = FluentChainUtils.GetFluentChainThreshold(options);
            return FluentChainUtils.CountChainInvocations(expr) >= threshold;
        }
    }
}
