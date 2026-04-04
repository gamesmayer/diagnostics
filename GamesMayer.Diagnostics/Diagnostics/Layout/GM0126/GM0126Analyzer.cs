using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0126Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0126";
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0126.threshold";
        private const int DefaultMinParameters = 4;
        internal const string FixTypeKey = "FixType";
        internal const string CollapseFixType = "CollapseToSingleLine";

        private static readonly DiagnosticDescriptor SplitDescriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each parameter in a large parameter list must be on its own line",
            messageFormat: "Move this parameter to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When a parameter list has at least the configured threshold of parameters, every parameter must be on its own line for clarity.");

        private static readonly DiagnosticDescriptor SingleLineDescriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "A small parameter list split across lines should be on a single line",
            messageFormat: "This parameter list has fewer items than the configured threshold and should be written on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When a parameter list has fewer than the configured threshold of parameters, all parameters should be on a single line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(SplitDescriptor, SingleLineDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ParameterList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var paramList = (ParameterListSyntax)context.Node;
            var parameters = paramList.Parameters;

            int minParameters = GetMinimumParameters(context);

            var tree = context.Node.SyntaxTree;

            if (parameters.Count < minParameters)
            {
                if (parameters.Count == 0)
                    return;

                var openParenLine = tree.GetLineSpan(paramList.OpenParenToken.Span).EndLinePosition.Line;
                var firstParamLine = tree.GetLineSpan(parameters[0].Span).StartLinePosition.Line;

                bool isMultiLine = firstParamLine != openParenLine;

                if (!isMultiLine)
                {
                    for (int i = 1; i < parameters.Count; i++)
                    {
                        var prevLine = tree.GetLineSpan(parameters[i - 1].Span).EndLinePosition.Line;
                        var currLine = tree.GetLineSpan(parameters[i].Span).StartLinePosition.Line;
                        if (currLine != prevLine)
                        {
                            isMultiLine = true;
                            break;
                        }
                    }
                }

                if (!isMultiLine)
                {
                    var lastParamLine = tree.GetLineSpan(parameters[parameters.Count - 1].Span).EndLinePosition.Line;
                    var closeParenLine = tree.GetLineSpan(paramList.CloseParenToken.Span).StartLinePosition.Line;
                    if (closeParenLine != lastParamLine)
                        isMultiLine = true;
                }

                if (isMultiLine)
                {
                    var properties = ImmutableDictionary.Create<string, string?>().Add(FixTypeKey, CollapseFixType);
                    context.ReportDiagnostic(Diagnostic.Create(SingleLineDescriptor, Location.Create(tree, paramList.Span), properties));
                }

                return;
            }

            var openParenLineSplit = tree.GetLineSpan(paramList.OpenParenToken.Span).EndLinePosition.Line;
            var firstParamLineSplit = tree.GetLineSpan(parameters[0].Span).StartLinePosition.Line;

            if (firstParamLineSplit == openParenLineSplit)
            {
                context.ReportDiagnostic(Diagnostic.Create(SplitDescriptor, Location.Create(tree, parameters[0].Span)));
            }

            for (int i = 1; i < parameters.Count; i++)
            {
                var prev = parameters[i - 1];
                var curr = parameters[i];

                var prevLine = tree.GetLineSpan(prev.Span).EndLinePosition.Line;
                var currLine = tree.GetLineSpan(curr.Span).StartLinePosition.Line;

                if (currLine == prevLine)
                {
                    context.ReportDiagnostic(Diagnostic.Create(SplitDescriptor, Location.Create(tree, curr.Span)));
                }
            }
        }

        private static int GetMinimumParameters(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue(ThresholdOptionKey, out var rawValue)
                && int.TryParse(rawValue, out var parsed)
                && parsed > 1)
            {
                return parsed;
            }

            return DefaultMinParameters;
        }
    }
}
