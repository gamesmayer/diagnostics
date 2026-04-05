using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0136Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0136";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Generic type argument list must be written on a single line",
            messageFormat: "Write the generic type argument list on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The generic type argument list from '<' to '>' must be written on a single line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.TypeArgumentList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var typeArgList = (TypeArgumentListSyntax)context.Node;
            var tree = context.Node.SyntaxTree;

            var lessThanLine = tree.GetLineSpan(typeArgList.LessThanToken.Span).EndLinePosition.Line;
            var greaterThanLine = tree.GetLineSpan(typeArgList.GreaterThanToken.Span).StartLinePosition.Line;

            if (lessThanLine != greaterThanLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, typeArgList.Span)));
            }
        }
    }
}
