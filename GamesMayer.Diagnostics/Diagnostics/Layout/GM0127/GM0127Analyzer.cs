using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0127Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0127";
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0127.threshold";
        private const int DefaultMinArguments = 4;

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each argument in a large argument list must be on its own line",
            messageFormat: "Move this argument to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When an argument list has at least the configured threshold of arguments, every argument must be on its own line for clarity.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ArgumentList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var argList = (ArgumentListSyntax)context.Node;
            var arguments = argList.Arguments;

            int minArguments = GetMinimumArguments(context);
            
            // Treat complex expressions (lambdas, object initializers, etc.) as if they met the threshold
            bool hasComplexExpression = arguments.Any(IsComplexExpression);
            
            if (arguments.Count < minArguments && !hasComplexExpression)
                return;

            var tree = context.Node.SyntaxTree;

            var openParenLine = tree.GetLineSpan(argList.OpenParenToken.Span).EndLinePosition.Line;
            var firstArgLine = tree.GetLineSpan(arguments[0].Span).StartLinePosition.Line;

            if (firstArgLine == openParenLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, arguments[0].Span)));
            }

            for (int i = 1; i < arguments.Count; i++)
            {
                var prev = arguments[i - 1];
                var curr = arguments[i];

                var prevLine = tree.GetLineSpan(prev.Span).EndLinePosition.Line;
                var currLine = tree.GetLineSpan(curr.Span).StartLinePosition.Line;

                if (currLine == prevLine)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, curr.Span)));
                }
            }

        }

        private static int GetMinimumArguments(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue(ThresholdOptionKey, out var rawValue)
                && int.TryParse(rawValue, out var parsed)
                && parsed > 1)
            {
                return parsed;
            }

            return DefaultMinArguments;
        }

        private static bool IsComplexExpression(ArgumentSyntax argument)
        {
            var expr = argument.Expression;
            
            // Lambda expressions
            if (expr is LambdaExpressionSyntax)
                return true;
            
            // Anonymous methods
            if (expr is AnonymousMethodExpressionSyntax)
                return true;
            
            // Anonymous types
            if (expr is AnonymousObjectCreationExpressionSyntax)
                return true;
            
            // Object creation with initializer
            if (expr is ObjectCreationExpressionSyntax objCreation && objCreation.Initializer != null)
                return true;
            
            return false;
        }
    }
}
