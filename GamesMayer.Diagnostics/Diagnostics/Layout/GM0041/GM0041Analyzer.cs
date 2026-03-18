using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0041Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0041";
        private const string ThresholdOptionKey = "dotnet_diagnostic.GM0041.threshold";
        private const int DefaultMinInvocations = 2;

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each segment in a multi-invocation fluent chain must be on its own line",
            messageFormat: "Move this segment to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a fluent-chain expression with more than one invocation, every segment must be on its own line for clarity.");

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

            int minInvocations = GetMinimumInvocations(context);

            var tree = context.Node.SyntaxTree;

            foreach (var node in chainRoot.DescendantNodesAndSelf())
            {
                if (node is ExpressionSyntax candidate && IsFluentChainStart(candidate))
                {
                    AnalyzeChain(context, candidate, tree, minInvocations);
                }
            }
        }

        private static void AnalyzeChain(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax chainExpression,
            SyntaxTree tree,
            int minInvocations)
        {
            var normalizedChain = GetChainRoot(chainExpression);
            if (normalizedChain == null)
                return;

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(normalizedChain, boundaries);
            if (boundaries.Count == 0)
                return;

            // Count invocation segments and find the first one
            int invocationCount = 0;
            int firstInvocationIndex = -1;
            for (int i = 0; i < boundaries.Count; i++)
            {
                if (boundaries[i].SegmentExpression is InvocationExpressionSyntax)
                {
                    invocationCount++;
                    if (firstInvocationIndex < 0)
                        firstInvocationIndex = i;
                }
            }

            if (invocationCount < minInvocations || firstInvocationIndex < 0)
                return;

            var segmentRanges = new List<(int StartIndex, int EndIndex)>();
            int currentSegmentStart = firstInvocationIndex;
            for (int i = firstInvocationIndex; i < boundaries.Count; i++)
            {
                if (boundaries[i].SegmentExpression is InvocationExpressionSyntax)
                {
                    segmentRanges.Add((currentSegmentStart, i));
                    currentSegmentStart = i + 1;
                }
            }

            if (segmentRanges.Count <= 1)
                return;

            // Check if any segment from the first invocation onward is already on its own line.
            // If so, this is a mixed-layout chain that GM0040 handles; skip it here.
            foreach (var segmentRange in segmentRanges)
            {
                var startBoundary = boundaries[segmentRange.StartIndex];
                var leftLastToken = startBoundary.LeftExpression.GetLastToken();
                var leftLine = tree.GetLineSpan(leftLastToken.Span).EndLinePosition.Line;
                var dotLine = tree.GetLineSpan(startBoundary.DotToken.Span).StartLinePosition.Line;

                if (dotLine > leftLine)
                    return;
            }

            // All invocation segments are on the same line — report each one.
            // A segment can include property-access subchains plus the next invocation.
            foreach (var segmentRange in segmentRanges)
            {
                var startBoundary = boundaries[segmentRange.StartIndex];
                var endBoundary = boundaries[segmentRange.EndIndex];
                var diagnosticSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(
                    startBoundary.DotToken.SpanStart,
                    endBoundary.SegmentExpression.Span.End);
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, diagnosticSpan)));
            }
        }

        private static int GetMinimumInvocations(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue(ThresholdOptionKey, out var rawValue)
                && int.TryParse(rawValue, out var parsed)
                && parsed > 1)
            {
                return parsed;
            }

            return DefaultMinInvocations;
        }

        private static bool IsFluentChainStart(ExpressionSyntax expression)
        {
            if (!IsFluentChainExpression(expression))
                return false;

            if (expression.Parent is InvocationExpressionSyntax parentInvocation
                && parentInvocation.Expression == expression)
                return false;

            if (expression.Parent is MemberAccessExpressionSyntax parentMemberAccess
                && parentMemberAccess.Expression == expression)
                return false;

            return true;
        }

        private static bool IsFluentChainExpression(ExpressionSyntax expression)
        {
            return expression is MemberAccessExpressionSyntax
                || (expression is InvocationExpressionSyntax invocation
                    && invocation.Expression is MemberAccessExpressionSyntax);
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
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax invMemberAccess)
            {
                CollectFluentChainBoundaries(invMemberAccess.Expression, boundaries);
                var nextToken = invMemberAccess.Name.GetFirstToken();
                boundaries.Add((invMemberAccess.Expression, invMemberAccess.OperatorToken, nextToken, invocation));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                CollectFluentChainBoundaries(memberAccess.Expression, boundaries);
                var nextToken = memberAccess.Name.GetFirstToken();
                boundaries.Add((memberAccess.Expression, memberAccess.OperatorToken, nextToken, memberAccess));
            }
        }
    }
}
