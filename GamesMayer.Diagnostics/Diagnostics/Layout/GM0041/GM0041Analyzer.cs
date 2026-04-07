using System.Collections.Generic;
using System.Collections.Immutable;
using GamesMayer.Diagnostics.Utils;
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
            title: "Fluent chain segments must be on the expected line based on threshold",
            messageFormat: "Move this segment to the expected line based on threshold",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a fluent-chain expression, segments must be on their own lines when the invocation count meets the threshold; otherwise they must be on the same line.");

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
            var chainRoot = FluentChainUtils.GetChainRoot(expression);
            if (chainRoot == null)
                return;

            int minInvocations = GetMinimumInvocations(context);

            var tree = context.Node.SyntaxTree;

            foreach (var node in chainRoot.DescendantNodesAndSelf())
            {
                if (node is ExpressionSyntax candidate && FluentChainUtils.IsFluentChainStart(candidate))
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
            var normalizedChain = FluentChainUtils.GetChainRoot(chainExpression);
            if (normalizedChain == null)
                return;

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression)>();
            FluentChainUtils.CollectFluentChainBoundaries(normalizedChain, boundaries);
            if (boundaries.Count == 0)
                return;

            // Count invocation segments and find the first one
            int invocationCount = 0;
            int firstInvocationIndex = -1;
            for (int i = 0; i < boundaries.Count; i++)
            {
                if (FluentChainUtils.IsInvocationSegment(boundaries[i].SegmentExpression))
                {
                    invocationCount++;
                    if (firstInvocationIndex < 0)
                        firstInvocationIndex = i;
                }
            }

            if (firstInvocationIndex < 0)
                return;

            var segmentRanges = new List<(int StartIndex, int EndIndex)>();
            int currentSegmentStart = firstInvocationIndex;
            for (int i = firstInvocationIndex; i < boundaries.Count; i++)
            {
                if (FluentChainUtils.IsInvocationSegment(boundaries[i].SegmentExpression))
                {
                    segmentRanges.Add((currentSegmentStart, i));
                    currentSegmentStart = i + 1;
                }
            }

            var enforceOwnLine = invocationCount >= minInvocations;

            if (enforceOwnLine && segmentRanges.Count <= 1)
                return;

            if (enforceOwnLine)
            {
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

                // All invocation segments are on the same line — report each one (expand).
                // A segment can include property-access subchains plus the next invocation.
                foreach (var segmentRange in segmentRanges)
                {
                    var startBoundary = boundaries[segmentRange.StartIndex];
                    var endBoundary = boundaries[segmentRange.EndIndex];
                    var diagnosticSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(
                        GetSegmentSpanStart(startBoundary, tree),
                        endBoundary.SegmentExpression.Span.End);
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, diagnosticSpan)));
                }
            }
            else
            {
                // Below threshold — segments should be on the same line as the chain root.
                // Report each segment that is on its own line (collapse).
                foreach (var segmentRange in segmentRanges)
                {
                    var startBoundary = boundaries[segmentRange.StartIndex];
                    var leftLastToken = startBoundary.LeftExpression.GetLastToken();
                    var leftLine = tree.GetLineSpan(leftLastToken.Span).EndLinePosition.Line;
                    var dotLine = tree.GetLineSpan(startBoundary.DotToken.Span).StartLinePosition.Line;

                    if (dotLine <= leftLine)
                        continue;

                    var endBoundary = boundaries[segmentRange.EndIndex];
                    var diagnosticSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(
                        GetSegmentSpanStart(startBoundary, tree),
                        endBoundary.SegmentExpression.Span.End);
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, diagnosticSpan)));
                }
            }
        }

        private static int GetSegmentSpanStart(
            (ExpressionSyntax LeftExpression, SyntaxToken DotToken, ExpressionSyntax SegmentExpression) boundary,
            SyntaxTree tree)
        {
            if (boundary.SegmentExpression is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                var leftLine = tree.GetLineSpan(boundary.LeftExpression.GetLastToken().Span).EndLinePosition.Line;
                var questionLine = tree.GetLineSpan(conditionalAccess.OperatorToken.Span).StartLinePosition.Line;
                if (questionLine > leftLine)
                    return conditionalAccess.OperatorToken.SpanStart;
            }
            return boundary.DotToken.SpanStart;
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
    }
}
