using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0132Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0132";
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0132.threshold";
        private const int DefaultThreshold = 2;

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each parent class or interface in inheritance must be on its own line",
            messageFormat: "Move this parent type to the expected line based on threshold",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In class and interface inheritance clauses, parent types must be on their own line when the count reaches the threshold; otherwise they must stay on the declaration line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBaseList, SyntaxKind.BaseList);
        }

        private static void AnalyzeBaseList(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;
            if (baseList.Parent is not ClassDeclarationSyntax and not InterfaceDeclarationSyntax)
            {
                return;
            }

            if (baseList.Types.Count == 0)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var threshold = GetThreshold(context);
            var enforceOwnLine = baseList.Types.Count >= threshold;
            var declarationLine = tree.GetLineSpan(baseList.ColonToken.Span).StartLinePosition.Line;

            for (int i = 0; i < baseList.Types.Count; i++)
            {
                var baseType = baseList.Types[i];
                var firstToken = baseType.GetFirstToken();
                if (firstToken == default)
                {
                    continue;
                }

                var previousToken = i == 0
                    ? baseList.ColonToken
                    : baseList.Types.GetSeparator(i - 1);

                var previousTokenLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
                var currentTokenLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;

                var isOnOwnLine = previousTokenLine != currentTokenLine;
                if (enforceOwnLine)
                {
                    if (isOnOwnLine)
                    {
                        continue;
                    }
                }
                else
                {
                    if (currentTokenLine == declarationLine)
                    {
                        continue;
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, baseType.GetLocation()));
            }
        }

        private static int GetThreshold(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue(ThresholdOptionKey, out var rawValue)
                && int.TryParse(rawValue, out var parsed)
                && parsed > 0)
            {
                return parsed;
            }

            return DefaultThreshold;
        }
    }
}