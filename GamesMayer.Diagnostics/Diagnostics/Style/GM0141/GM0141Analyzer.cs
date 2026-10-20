using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0141Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0141";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "continue statement is not allowed",
            messageFormat: "Remove the 'continue' statement",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The 'continue' keyword adds complexity to code readability and should not be used.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ContinueStatement);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }
    }
}
