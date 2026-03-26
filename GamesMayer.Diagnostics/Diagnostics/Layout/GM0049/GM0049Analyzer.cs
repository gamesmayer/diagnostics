using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0049Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0049";

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
            title: "Wrapped operator-expression item must be indented one step from expression start",
            messageFormat: "Indent this item one step from the expression start line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a multi-line operator expression, each wrapped item must be indented exactly one step from the expression start line.");

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
            var tree = binary.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var prevLastToken = binary.Left.GetLastToken();
            var operatorToken = binary.OperatorToken;
            var rightFirstToken = binary.Right.GetFirstToken();

            var prevLastTokenLine = tree.GetLineSpan(prevLastToken.Span).EndLinePosition.Line;
            var operatorLine = tree.GetLineSpan(operatorToken.Span).StartLinePosition.Line;
            var rightLine = tree.GetLineSpan(rightFirstToken.Span).StartLinePosition.Line;

            if (rightLine <= operatorLine)
                return;

            // GM0049 complements GM0046, so only check indentation once operator placement is already valid.
            if (operatorLine != prevLastTokenLine)
                return;

            var chainRoot = GetTopMostBinaryExpression(binary);
            var expressionStartLine = tree.GetLineSpan(chainRoot.GetFirstToken().Span).StartLinePosition.Line;

            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, expressionStartLine);
            var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, rightLine);
            var expectedIndentation = GetExpectedIndentation(baseIndentation, GetIndentSize(context));

            if (actualIndentation != expectedIndentation)
            {
                var rightTextLine = sourceText.Lines[rightLine];
                var diagnosticSpan = TextSpan.FromBounds(rightFirstToken.SpanStart, rightTextLine.End);

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    Location.Create(tree, diagnosticSpan)));
            }
        }

        internal static BinaryExpressionSyntax GetTopMostBinaryExpression(BinaryExpressionSyntax binary)
        {
            var current = binary;

            while (current.Parent is BinaryExpressionSyntax parent && IsSupportedBinaryKind(parent.Kind()))
                current = parent;

            return current;
        }

        internal static bool IsSupportedBinaryKind(SyntaxKind kind)
        {
            for (int i = 0; i < BinaryExpressionKinds.Length; i++)
            {
                if (BinaryExpressionKinds[i] == kind)
                    return true;
            }

            return false;
        }

        internal static string GetExpectedIndentation(string baseIndentation, int indentSize)
        {
            string indentUnit = baseIndentation.Length > 0 && baseIndentation[0] == '\t'
                ? "\t"
                : new string(' ', indentSize);
            return baseIndentation + indentUnit;
        }

        internal static int GetIndentationLength(string line)
        {
            int length = 0;
            while (length < line.Length && (line[length] == ' ' || line[length] == '\t'))
                length++;
            return length;
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
