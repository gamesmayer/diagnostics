using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0005Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0005";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Attributes separated by commas",
            messageFormat: "Use separate attribute declarations",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Attributes should be declared in separate brackets instead of being separated by commas in the same bracket.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.AttributeList);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var attributeList = (AttributeListSyntax)context.Node;

            // If the attribute list contains more than one attribute (comma-separated)
            if (attributeList.Attributes.Count > 1)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Descriptor, attributeList.GetLocation()));
            }
        }
    }
}
