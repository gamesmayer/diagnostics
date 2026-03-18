using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0040Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0040";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "All fluent-chain segments must be on their own line if any segment is",
            messageFormat: "Move this segment to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a fluent-chain statement, if one segment is written after a line break, then all the segments must be on their own line.");

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

            foreach (var node in chainRoot.DescendantNodesAndSelf())
            {
                if (node is ExpressionSyntax candidate && IsFluentChainStart(candidate))
                {
                    AnalyzeChain(context, candidate, tree);
                }
            }
        }

        private static void AnalyzeChain(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax chainExpression,
            SyntaxTree tree)
        {
            var normalizedChain = GetChainRoot(chainExpression);
            if (normalizedChain == null)
                return;

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(normalizedChain, boundaries);
            if (boundaries.Count == 0)
                return;

            // A segment is defined as the chain of member accesses up to and including the
            // next invocation. Only invocation boundaries are checked and reported.
            var invocationBoundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            foreach (var boundary in boundaries)
            {
                if (boundary.SegmentExpression is InvocationExpressionSyntax)
                    invocationBoundaries.Add(boundary);
            }

            if (invocationBoundaries.Count == 0)
                return;

            var onOwnLine = new bool[invocationBoundaries.Count];
            bool anyOnOwnLine = false;

            for (int i = 0; i < invocationBoundaries.Count; i++)
            {
                var (leftExpression, dotToken, _, _) = invocationBoundaries[i];

                // For the first invocation, compare against the end of the root expression.
                // For subsequent invocations, compare against the end of the previous invocation
                // so that non-invocation member accesses between two invocations are treated
                // as part of the same segment.
                SyntaxToken referenceToken = i == 0
                    ? leftExpression.GetLastToken()
                    : invocationBoundaries[i - 1].SegmentExpression.GetLastToken();

                var referenceLine = tree.GetLineSpan(referenceToken.Span).EndLinePosition.Line;
                var dotLine = tree.GetLineSpan(dotToken.Span).StartLinePosition.Line;

                onOwnLine[i] = dotLine > referenceLine;
                if (onOwnLine[i])
                    anyOnOwnLine = true;
            }

            if (!anyOnOwnLine)
                return;

            for (int i = 0; i < invocationBoundaries.Count; i++)
            {
                if (!onOwnLine[i])
                {
                    var (_, dotToken, _, segmentExpression) = invocationBoundaries[i];
                    var diagnosticSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(dotToken.SpanStart, segmentExpression.Span.End);
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, diagnosticSpan)));
                }
            }
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
