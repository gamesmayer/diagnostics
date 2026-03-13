using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0016Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0016";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Attribute must be on a single line",
            messageFormat: "Write the attribute on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Attributes must be declared on a single line. No line breaks are allowed between the opening '[' and closing ']'.");

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
            var tree = attributeList.SyntaxTree;

            var openLine = tree.GetLineSpan(attributeList.OpenBracketToken.Span).StartLinePosition.Line;
            var closeLine = tree.GetLineSpan(attributeList.CloseBracketToken.Span).StartLinePosition.Line;

            if (openLine != closeLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, attributeList.GetLocation()));
            }
        }
    }
}
