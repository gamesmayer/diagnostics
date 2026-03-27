using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0056Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0056";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "'where' constraint clause must be written on a single line",
            messageFormat: "Write the 'where' constraint clause on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Each 'where' constraint clause must be written entirely on a single line. Line breaks between tokens of the clause are not allowed.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeConstraintClause, SyntaxKind.TypeParameterConstraintClause);
        }

        private static void AnalyzeConstraintClause(SyntaxNodeAnalysisContext context)
        {
            var constraintClause = (TypeParameterConstraintClauseSyntax)context.Node;
            var tree = constraintClause.SyntaxTree;

            var firstToken = constraintClause.GetFirstToken();
            var lastToken = constraintClause.GetLastToken();

            var firstLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
            var lastLine = tree.GetLineSpan(lastToken.Span).StartLinePosition.Line;

            if (firstLine != lastLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, constraintClause.GetLocation()));
            }
        }
    }
}
