using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0057Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0057";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between declaration and first 'where' constraint clause",
            messageFormat: "Remove the blank line between the declaration and the first 'where' constraint clause",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A method or type declaration must not have blank lines between its header and the first 'where' constraint clause.");

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

            // Only check the first constraint clause in the declaration
            if (!IsFirstConstraintClause(constraintClause))
                return;

            var whereKeyword = constraintClause.WhereKeyword;
            var previousToken = whereKeyword.GetPreviousToken();

            if (previousToken == default)
                return;

            var syntaxTree = constraintClause.SyntaxTree;
            var sourceText = syntaxTree.GetText(context.CancellationToken);

            var previousTokenLine = syntaxTree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
            var whereKeywordLine = syntaxTree.GetLineSpan(whereKeyword.Span).StartLinePosition.Line;

            if (whereKeywordLine <= previousTokenLine + 1)
                return;

            for (var line = previousTokenLine + 1; line < whereKeywordLine; line++)
            {
                var lineText = sourceText.Lines[line].ToString();
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan)));
            }
        }

        private static bool IsFirstConstraintClause(TypeParameterConstraintClauseSyntax constraintClause)
        {
            var parent = constraintClause.Parent;
            if (parent == null)
                return false;

            foreach (var child in parent.ChildNodes())
            {
                if (child is TypeParameterConstraintClauseSyntax clause)
                    return clause == constraintClause;
            }

            return false;
        }
    }
}
