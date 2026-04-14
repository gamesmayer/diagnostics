using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0081Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0081";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Case labels must be indented one step right from the switch keyword",
            messageFormat: "Indent the case label one step right from the switch keyword",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Case labels in a switch statement must be indented exactly one step right from the switch keyword.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchExpression, SyntaxKind.SwitchExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;
            var tree = context.Node.SyntaxTree;

            var switchCol = tree.GetLineSpan(switchStatement.SwitchKeyword.Span).StartLinePosition.Character;
            var indentStep = DetectIndentStep(switchStatement, tree, switchCol);
            var expectedCol = switchCol + indentStep;

            foreach (var section in switchStatement.Sections)
            {
                foreach (var label in section.Labels)
                {
                    var labelToken = label.GetFirstToken();
                    var labelCol = tree.GetLineSpan(labelToken.Span).StartLinePosition.Character;

                    if (labelCol != expectedCol)
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, label.GetLocation()));
                }
            }
        }

        private static void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context)
        {
            var switchExpr = (SwitchExpressionSyntax)context.Node;
            var tree = context.Node.SyntaxTree;

            var openBraceLine = tree.GetLineSpan(switchExpr.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(switchExpr.CloseBraceToken.Span).StartLinePosition.Line;
            if (openBraceLine == closeBraceLine)
                return;

            var containingStatement = FindContainingStatement(switchExpr);
            var refToken = containingStatement != null
                ? containingStatement.GetFirstToken()
                : switchExpr.GetFirstToken();
            var refCol = tree.GetLineSpan(refToken.Span).StartLinePosition.Character;

            var indentStep = DetectIndentStepForExpression(switchExpr, tree, refCol);
            var expectedCol = refCol + indentStep;

            foreach (var arm in switchExpr.Arms)
            {
                var armToken = arm.GetFirstToken();
                var armLine = tree.GetLineSpan(armToken.Span).StartLinePosition.Line;
                if (armLine == openBraceLine)
                    continue;

                var armCol = tree.GetLineSpan(armToken.Span).StartLinePosition.Character;
                if (armCol != expectedCol)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, arm.GetLocation()));
            }
        }

        private static SyntaxNode? FindContainingStatement(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is StatementSyntax)
                    return current;
                current = current.Parent;
            }
            return null;
        }

        internal static int DetectIndentStepForExpression(SwitchExpressionSyntax switchExpr, SyntaxTree tree, int refCol)
        {
            var current = (SyntaxNode)switchExpr;
            while (current != null)
            {
                if (current is BlockSyntax block && block.Parent != null)
                {
                    var parentCol = tree.GetLineSpan(block.Parent.GetFirstToken().Span).StartLinePosition.Character;
                    var step = refCol - parentCol;
                    if (step > 0)
                        return step;
                    break;
                }
                current = current.Parent;
            }
            return 4;
        }

        internal static int DetectIndentStep(SwitchStatementSyntax switchStatement, SyntaxTree tree, int switchCol)
        {
            if (switchStatement.Parent is BlockSyntax block && block.Parent != null)
            {
                var parentCol = tree.GetLineSpan(block.Parent.GetFirstToken().Span).StartLinePosition.Character;
                var step = switchCol - parentCol;

                if (step > 0)
                    return step;
            }

            return 4;
        }
    }
}
