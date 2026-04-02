using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0038Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0038";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Fluent-chain segment indentation",
            messageFormat: "Indent this fluent-chain segment one step right from the start indentation",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line fluent-chain statement, each segment's leading dot must be indented exactly one step to the right of the chain's starting indentation.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
            context.RegisterSyntaxNodeAction(AnalyzeLocalDeclarationStatement, SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
            context.RegisterSyntaxNodeAction(AnalyzeArrowExpressionClause, SyntaxKind.ArrowExpressionClause);
        }

        private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
        {
            var statement = (ExpressionStatementSyntax)context.Node;
            AnalyzeExpression(context, statement.Expression);
        }

        private static void AnalyzeLocalDeclarationStatement(SyntaxNodeAnalysisContext context)
        {
            var declaration = (LocalDeclarationStatementSyntax)context.Node;
            foreach (var variable in declaration.Declaration.Variables)
            {
                if (variable.Initializer?.Value is { } value)
                    AnalyzeExpression(context, value);
            }
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var statement = (ReturnStatementSyntax)context.Node;
            if (statement.Expression is { } expression)
                AnalyzeExpression(context, expression);
        }

        private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            var arrow = (ArrowExpressionClauseSyntax)context.Node;
            AnalyzeExpression(context, arrow.Expression);
        }

        private static void AnalyzeExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            var chainRoot = GetChainRoot(expression);
            if (chainRoot == null)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            if (expression is AssignmentExpressionSyntax assignment)
            {
                foreach (var node in assignment.Left.DescendantNodesAndSelf())
                {
                    if (node is ExpressionSyntax candidate && IsFluentChainStart(candidate))
                        AnalyzeChain(context, candidate, tree, sourceText);
                }
            }

            foreach (var node in chainRoot.DescendantNodesAndSelf())
            {
                if (node is ExpressionSyntax candidate && IsFluentChainStart(candidate))
                {
                    AnalyzeChain(context, candidate, tree, sourceText);
                }
            }
        }

        private static void AnalyzeChain(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax chainExpression,
            SyntaxTree tree,
            Microsoft.CodeAnalysis.Text.SourceText sourceText)
        {
            var normalizedChain = GetChainRoot(chainExpression);
            if (normalizedChain == null)
            {
                return;
            }

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(normalizedChain, boundaries);
            if (boundaries.Count == 0)
            {
                return;
            }

            var chainStartLine = tree.GetLineSpan(normalizedChain.GetFirstToken().Span).StartLinePosition.Line;
            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, chainStartLine);
            var indentSize = GetIndentSize(context);
            var expectedIndentation = GetExpectedIndentation(baseIndentation, indentSize);

            foreach (var (leftExpression, dotToken, segmentExpression) in boundaries)
            {
                var previousEndLine = tree.GetLineSpan(leftExpression.GetLastToken().Span).EndLinePosition.Line;
                var dotLine = tree.GetLineSpan(dotToken.Span).StartLinePosition.Line;

                if (dotLine <= previousEndLine)
                    continue;

                var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, dotLine);
                if (actualIndentation != expectedIndentation)
                {
                    var diagnosticSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(dotToken.SpanStart, segmentExpression.Span.End);
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, diagnosticSpan)));
                }
            }
        }

        private static bool IsFluentChainStart(ExpressionSyntax expression)
        {
            if (!IsFluentChainExpression(expression))
            {
                return false;
            }

            if (expression.Parent is InvocationExpressionSyntax parentInvocation
                && parentInvocation.Expression == expression)
            {
                return false;
            }

            if (expression.Parent is MemberAccessExpressionSyntax parentMemberAccess
                && parentMemberAccess.Expression == expression)
            {
                return false;
            }

            return true;
        }

        private static bool IsFluentChainExpression(ExpressionSyntax expression)
        {
            return expression is MemberAccessExpressionSyntax
                || (expression is InvocationExpressionSyntax invocation
                    && invocation.Expression is MemberAccessExpressionSyntax);
        }

        internal static string GetExpectedIndentation(string baseIndentation, int indentSize)
        {
            string indentUnit = baseIndentation.Length > 0 && baseIndentation[0] == '\t'
                ? "\t"
                : new string(' ', indentSize);
            return baseIndentation + indentUnit;
        }

        private static ExpressionSyntax GetChainRoot(ExpressionSyntax expression)
        {
            var current = expression;
            while (true)
            {
                switch (current)
                {
                    case ParenthesizedExpressionSyntax p:
                        current = p.Expression;
                        continue;
                    case AwaitExpressionSyntax a:
                        current = a.Expression;
                        continue;
                    case AssignmentExpressionSyntax ae:
                        current = ae.Right;
                        continue;
                    default:
                        return current;
                }
            }
        }

        private static void CollectFluentChainBoundaries(
            ExpressionSyntax expression,
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax invMemberAccess)
            {
                CollectFluentChainBoundaries(invMemberAccess.Expression, boundaries);
                boundaries.Add((invMemberAccess.Expression, invMemberAccess.OperatorToken, invocation));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                CollectFluentChainBoundaries(memberAccess.Expression, boundaries);
                boundaries.Add((memberAccess.Expression, memberAccess.OperatorToken, memberAccess));
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
