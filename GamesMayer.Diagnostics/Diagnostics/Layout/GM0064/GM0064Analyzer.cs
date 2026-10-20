using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0064Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0064";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Early return in function",
            messageFormat: "Remove the early return statement",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Functions should have a single return statement at the end. Early return statements make control flow harder to follow and reason about.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.LocalFunctionStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            BlockSyntax? block = context.Node switch
            {
                MethodDeclarationSyntax m => m.Body,
                LocalFunctionStatementSyntax lf => lf.Body,
                _ => null
            };

            if (block == null)
                return;

            var returnStatements = GetDirectReturnStatements(block).ToList();

            if (returnStatements.Count == 0)
                return;

            var lastStatement = block.Statements.LastOrDefault();
            var acceptableReturn = lastStatement as ReturnStatementSyntax;

            foreach (var returnStatement in returnStatements)
            {
                if (acceptableReturn != null && returnStatement == acceptableReturn)
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, returnStatement.GetLocation()));
            }
        }

        private static IEnumerable<ReturnStatementSyntax> GetDirectReturnStatements(BlockSyntax block)
        {
            return block.DescendantNodes(n =>
                !(n is LambdaExpressionSyntax ||
                  n is AnonymousMethodExpressionSyntax ||
                  n is LocalFunctionStatementSyntax))
                .OfType<ReturnStatementSyntax>();
        }
    }
}
