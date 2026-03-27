using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0055Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0055";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between 'where' constraint clauses",
            messageFormat: "Remove the blank line between 'where' constraint clauses",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Consecutive 'where' constraint clauses must not be separated by blank lines.");

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
            var nextClause = GetNextConstraintClause(constraintClause);
            if (nextClause == null)
                return;

            var syntaxTree = constraintClause.SyntaxTree;
            var sourceText = syntaxTree.GetText(context.CancellationToken);

            var currentLastLine = syntaxTree.GetLineSpan(constraintClause.GetLastToken().Span).EndLinePosition.Line;
            var nextFirstLine = syntaxTree.GetLineSpan(nextClause.GetFirstToken().Span).StartLinePosition.Line;

            if (nextFirstLine <= currentLastLine + 1)
                return;

            for (var line = currentLastLine + 1; line < nextFirstLine; line++)
            {
                var lineText = sourceText.Lines[line].ToString();
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan)));
            }
        }

        private static TypeParameterConstraintClauseSyntax? GetNextConstraintClause(
            TypeParameterConstraintClauseSyntax constraintClause)
        {
            var parent = constraintClause.Parent;
            if (parent == null)
                return null;

            bool foundCurrent = false;
            foreach (var child in parent.ChildNodes())
            {
                if (child is TypeParameterConstraintClauseSyntax clause)
                {
                    if (foundCurrent)
                        return clause;
                    if (clause == constraintClause)
                        foundCurrent = true;
                }
            }

            return null;
        }
    }
}
