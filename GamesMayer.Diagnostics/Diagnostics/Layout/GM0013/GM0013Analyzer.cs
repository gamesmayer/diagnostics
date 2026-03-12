using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0013Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0013";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Switch case block must be wrapped in braces",
            messageFormat: "Wrap the switch case block in braces",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A switch case code block must be wrapped in braces, including the break statement.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SwitchSection);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var section = (SwitchSectionSyntax)context.Node;

            if (section.Statements.Count == 0)
                return;

            if (section.Statements.Count == 1 && section.Statements[0] is BlockSyntax)
                return;

            var location = Location.Create(
                section.SyntaxTree,
                TextSpan.FromBounds(section.Statements[0].SpanStart, section.Statements.Last().Span.End));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
