using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0035Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0035";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between fluent-chain segments",
            messageFormat: "Remove the blank line between fluent-chain segments",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Fluent-chain segments must be contiguous with no blank lines between consecutive segments.");

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
                if (variable.Initializer?.Value is { } initializerExpression)
                {
                    AnalyzeExpression(context, initializerExpression);
                }
            }
        }

        private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
        {
            var statement = (ReturnStatementSyntax)context.Node;
            if (statement.Expression is { } expression)
            {
                AnalyzeExpression(context, expression);
            }
        }

        private static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context)
        {
            var arrowExpressionClause = (ArrowExpressionClauseSyntax)context.Node;
            AnalyzeExpression(context, arrowExpressionClause.Expression);
        }

        private static void AnalyzeExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            var chainRoot = GetChainRoot(expression);
            if (chainRoot == null)
            {
                return;
            }

            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken)>();
            CollectFluentChainBoundaries(chainRoot, boundaries);
            if (boundaries.Count == 0)
            {
                return;
            }

            var syntaxTree = context.Node.SyntaxTree;
            var sourceText = syntaxTree.GetText(context.CancellationToken);

            foreach (var (leftExpression, dotToken) in boundaries)
            {
                var previousEndLine = syntaxTree.GetLineSpan(leftExpression.GetLastToken().Span).EndLinePosition.Line;
                var currentStartLine = syntaxTree.GetLineSpan(dotToken.Span).StartLinePosition.Line;

                if (currentStartLine <= previousEndLine + 1)
                {
                    continue;
                }

                for (var line = previousEndLine + 1; line < currentStartLine; line++)
                {
                    var lineText = sourceText.Lines[line].ToString();
                    if (!string.IsNullOrWhiteSpace(lineText))
                    {
                        continue;
                    }

                    var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan)));
                }
            }
        }

        private static ExpressionSyntax? GetChainRoot(ExpressionSyntax expression)
        {
            var current = expression;
            while (true)
            {
                switch (current)
                {
                    case ParenthesizedExpressionSyntax parenthesizedExpression:
                        current = parenthesizedExpression.Expression;
                        continue;
                    case AwaitExpressionSyntax awaitExpression:
                        current = awaitExpression.Expression;
                        continue;
                    case AssignmentExpressionSyntax assignmentExpression:
                        current = assignmentExpression.Right;
                        continue;
                    default:
                        return current;
                }
            }
        }

        private static void CollectFluentChainBoundaries(
            ExpressionSyntax expression,
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocationExpression
                && invocationExpression.Expression is MemberAccessExpressionSyntax invocationMemberAccess)
            {
                CollectFluentChainBoundaries(invocationMemberAccess.Expression, boundaries);
                boundaries.Add((invocationMemberAccess.Expression, invocationMemberAccess.OperatorToken));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccessExpression)
            {
                CollectFluentChainBoundaries(memberAccessExpression.Expression, boundaries);
                boundaries.Add((memberAccessExpression.Expression, memberAccessExpression.OperatorToken));
            }
        }
    }
}
