using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0054Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0054";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "'where' constraint clause must be indented one step from the declaration",
            messageFormat: "Indent the 'where' constraint clause one step from the declaration",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Each 'where' constraint clause must be indented exactly one step to the right of the containing declaration.");

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

            var tree = constraintClause.SyntaxTree;
            var whereKeywordLine = tree.GetLineSpan(whereKeyword.Span).StartLinePosition.Line;
            var previousTokenLine = tree.GetLineSpan(previousToken.Span).StartLinePosition.Line;

            // Only check indentation when the where clause is on its own line
            if (whereKeywordLine == previousTokenLine)
                return;

            var sourceText = tree.GetText(context.CancellationToken);
            var declaration = constraintClause.Parent;
            if (declaration == null)
                return;
            var declarationFirstTokenLine = tree.GetLineSpan(declaration.GetFirstToken().Span).StartLinePosition.Line;

            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, declarationFirstTokenLine);
            var indentSize = GetIndentSize(context);
            var expectedIndentation = GM0049Analyzer.GetExpectedIndentation(baseIndentation, indentSize);
            var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, whereKeywordLine);

            if (actualIndentation != expectedIndentation)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    constraintClause.GetLocation()));
            }
        }

        private static int GetIndentSize(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue("indent_size", out var value) && int.TryParse(value, out var size) && size > 0)
                return size;
            return 4;
        }
    }
}
