using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0053Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0053";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each 'where' constraint clause must be on its own line",
            messageFormat: "The 'where' constraint clause for '{0}' must be on its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In method or type declarations with generic type parameters, each 'where' constraint clause must be written on its own separate line.");

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
            var whereKeyword = constraintClause.WhereKeyword;
            var previousToken = whereKeyword.GetPreviousToken();

            if (previousToken == default)
                return;

            var whereKeywordLine = whereKeyword.GetLocation().GetLineSpan().StartLinePosition.Line;
            var previousTokenLine = previousToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (whereKeywordLine == previousTokenLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    constraintClause.GetLocation(),
                    constraintClause.Name.Identifier.Text));
            }
        }
    }
}
