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
        internal const string FixTypeKey = "FixType";
        internal const string CollapseFixType = "CollapseToSingleLine";

        private static readonly DiagnosticDescriptor SplitDescriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each argument in a large argument list must be on its own line",
            messageFormat: "Move this argument to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When an argument list has at least the configured threshold of arguments, every argument must be on its own line for clarity.");

        private static readonly DiagnosticDescriptor SingleLineDescriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "A small argument list split across lines should be on a single line",
            messageFormat: "This argument list has fewer items than the configured threshold and should be written on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When an argument list has fewer than the configured threshold of arguments and no complex expressions, all arguments should be on a single line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(SplitDescriptor, SingleLineDescriptor);

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

            var tree = context.Node.SyntaxTree;

            if (arguments.Count < minArguments && !hasComplexExpression)
            {
                if (arguments.Count == 0)
                    return;

                var openParenLine = tree.GetLineSpan(argList.OpenParenToken.Span).EndLinePosition.Line;
                var firstArgLine = tree.GetLineSpan(arguments[0].Span).StartLinePosition.Line;

                bool isMultiLine = firstArgLine != openParenLine;

                if (!isMultiLine)
                {
                    for (int i = 1; i < arguments.Count; i++)
                    {
                        var prevLine = tree.GetLineSpan(arguments[i - 1].Span).EndLinePosition.Line;
                        var currLine = tree.GetLineSpan(arguments[i].Span).StartLinePosition.Line;
                        if (currLine != prevLine)
                        {
                            isMultiLine = true;
                            break;
                        }
                    }
                }

                if (!isMultiLine)
                {
                    var lastArgLine = tree.GetLineSpan(arguments[arguments.Count - 1].Span).EndLinePosition.Line;
                    var closeParenLine = tree.GetLineSpan(argList.CloseParenToken.Span).StartLinePosition.Line;
                    if (closeParenLine != lastArgLine)
                        isMultiLine = true;
                }

                if (isMultiLine)
                {
                    var properties = ImmutableDictionary.Create<string, string?>().Add(FixTypeKey, CollapseFixType);
                    context.ReportDiagnostic(Diagnostic.Create(SingleLineDescriptor, Location.Create(tree, argList.Span), properties));
                }

                return;
            }

            var openParenLineSplit = tree.GetLineSpan(argList.OpenParenToken.Span).EndLinePosition.Line;
            var firstArgLineSplit = tree.GetLineSpan(arguments[0].Span).StartLinePosition.Line;

            if (firstArgLineSplit == openParenLineSplit)
            {
                context.ReportDiagnostic(Diagnostic.Create(SplitDescriptor, Location.Create(tree, arguments[0].Span)));
            }

            for (int i = 1; i < arguments.Count; i++)
            {
                var prev = arguments[i - 1];
                var curr = arguments[i];

                var prevLine = tree.GetLineSpan(prev.Span).EndLinePosition.Line;
                var currLine = tree.GetLineSpan(curr.Span).StartLinePosition.Line;

                if (currLine == prevLine)
                {
                    context.ReportDiagnostic(Diagnostic.Create(SplitDescriptor, Location.Create(tree, curr.Span)));
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
