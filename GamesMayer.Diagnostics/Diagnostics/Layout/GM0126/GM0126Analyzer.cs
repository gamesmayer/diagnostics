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

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each parameter in a large parameter list must be on its own line",
            messageFormat: "Move this parameter to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When a parameter list has at least the configured threshold of parameters, every parameter must be on its own line for clarity.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

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
            if (parameters.Count < minParameters)
                return;

            var tree = context.Node.SyntaxTree;

            for (int i = 1; i < parameters.Count; i++)
            {
                var prev = parameters[i - 1];
                var curr = parameters[i];

                var prevLine = tree.GetLineSpan(prev.Span).EndLinePosition.Line;
                var currLine = tree.GetLineSpan(curr.Span).StartLinePosition.Line;

                if (currLine == prevLine)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, curr.Span)));
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
