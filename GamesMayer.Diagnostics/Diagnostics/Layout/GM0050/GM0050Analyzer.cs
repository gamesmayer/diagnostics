using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0050Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0050";

        private static readonly SyntaxKind[] BinaryExpressionKinds =
        {
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.CoalesceExpression,
        };

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between multi-line operator-expression items",
            messageFormat: "Remove the blank line between operator-expression items",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line operator expression, items must be contiguous with no blank lines between consecutive items.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, BinaryExpressionKinds);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            var syntaxTree = binary.SyntaxTree;
            var sourceText = syntaxTree.GetText(context.CancellationToken);

            var operatorLine = syntaxTree.GetLineSpan(binary.OperatorToken.Span).StartLinePosition.Line;
            var rightLine = syntaxTree.GetLineSpan(binary.Right.GetFirstToken().Span).StartLinePosition.Line;

            if (rightLine <= operatorLine + 1)
                return;

            for (var line = operatorLine + 1; line < rightLine; line++)
            {
                var lineText = sourceText.Lines[line].ToString();
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan)));
            }
        }
    }
}
