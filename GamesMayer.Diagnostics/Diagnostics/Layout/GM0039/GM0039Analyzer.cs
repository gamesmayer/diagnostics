using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0039Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0039";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Fluent-chain dot must be on the same line as the next identifier",
            messageFormat: "Move the dot to the beginning of the next line with the identifier",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line fluent-chain statement, the dot must be on the same line as the next identifier, not at the end of the previous line.");

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

            foreach (var (leftExpression, dotToken, nextToken, segmentExpression) in boundaries)
            {
                var dotLine = tree.GetLineSpan(dotToken.Span).StartLinePosition.Line;
                var nextTokenLine = tree.GetLineSpan(nextToken.Span).StartLinePosition.Line;

                // If the dot and the next identifier are on different lines, it's a violation
                // The correct format is: identifier\n.nextIdentifier (not identifier.\nnextIdentifier)
                if (dotLine < nextTokenLine)
                {
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

            if (expression.Parent is ConditionalAccessExpressionSyntax parentConditionalAccess
                && parentConditionalAccess.Expression == expression)
                return false;

            return true;
        }

        private static bool IsFluentChainExpression(ExpressionSyntax expression)
        {
            return expression is MemberAccessExpressionSyntax
                || expression is ConditionalAccessExpressionSyntax
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
                return;
            }

            if (expression is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                CollectFluentChainBoundaries(conditionalAccess.Expression, boundaries);

                MemberBindingExpressionSyntax? memberBinding = null;
                if (conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax directBinding)
                    memberBinding = directBinding;
                else if (conditionalAccess.WhenNotNull is InvocationExpressionSyntax invocWhenNotNull
                    && invocWhenNotNull.Expression is MemberBindingExpressionSyntax invocBinding)
                    memberBinding = invocBinding;

                if (memberBinding != null)
                {
                    var dotToken = memberBinding.OperatorToken;
                    var nextToken = memberBinding.Name.GetFirstToken();
                    boundaries.Add((conditionalAccess.Expression, dotToken, nextToken, conditionalAccess));
                }
            }
        }
    }
}
