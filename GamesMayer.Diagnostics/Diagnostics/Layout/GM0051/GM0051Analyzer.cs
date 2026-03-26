using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0051Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0051";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Return expression must start on the same line as 'return'",
            messageFormat: "Move the return expression to start on the same line as 'return'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line return statement, the returned expression must begin on the same line as the 'return' keyword.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var returnStatement = (ReturnStatementSyntax)context.Node;

            if (returnStatement.Expression == null)
                return;

            var tree = returnStatement.SyntaxTree;

            var returnKeyword = returnStatement.ReturnKeyword;
            var expressionFirstToken = returnStatement.Expression.GetFirstToken();

            if (expressionFirstToken == default)
                return;

            var returnLine = tree.GetLineSpan(returnKeyword.Span).EndLinePosition.Line;
            var expressionLine = tree.GetLineSpan(expressionFirstToken.Span).StartLinePosition.Line;

            if (expressionLine != returnLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    returnStatement.Expression.GetLocation()));
            }
        }
    }
}
