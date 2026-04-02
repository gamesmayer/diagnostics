using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0089Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0089";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0089.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between parentheses",
            messageFormat: "{0} spaces between parentheses",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether spaces are required between parentheses in control flow statements, parenthesized expressions, and cast expressions. Equivalent to the csharp_space_between_parentheses EditorConfig option.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.IfStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.WhileStatement,
                SyntaxKind.DoStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.LockStatement,
                SyntaxKind.UsingStatement,
                SyntaxKind.FixedStatement,
                SyntaxKind.ParenthesizedExpression,
                SyntaxKind.CastExpression,
                SyntaxKind.TupleType,
                SyntaxKind.TupleExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!TryGetParentheses(context.Node, out var openParen, out var closeParen))
                return;

            var firstToken = openParen.GetNextToken();
            var lastToken = closeParen.GetPreviousToken();

            if (firstToken == closeParen || firstToken.SpanStart >= closeParen.SpanStart)
                return;

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    openParen,
                    firstToken,
                    out var hasOpenSpace))
            {
                return;
            }

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    lastToken,
                    closeParen,
                    out var hasCloseSpace))
            {
                return;
            }

            var enabled = GetEnabled(context);
            var expectedHasSpace = enabled;

            if (hasOpenSpace == expectedHasSpace && hasCloseSpace == expectedHasSpace)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                openParen.GetLocation(),
                properties,
                action));
        }

        private static bool TryGetParentheses(SyntaxNode node, out SyntaxToken openParen, out SyntaxToken closeParen)
        {
            switch (node)
            {
                case IfStatementSyntax ifStatement:
                    openParen = ifStatement.OpenParenToken;
                    closeParen = ifStatement.CloseParenToken;
                    return true;
                case ForStatementSyntax forStatement:
                    openParen = forStatement.OpenParenToken;
                    closeParen = forStatement.CloseParenToken;
                    return true;
                case ForEachStatementSyntax forEachStatement:
                    openParen = forEachStatement.OpenParenToken;
                    closeParen = forEachStatement.CloseParenToken;
                    return true;
                case WhileStatementSyntax whileStatement:
                    openParen = whileStatement.OpenParenToken;
                    closeParen = whileStatement.CloseParenToken;
                    return true;
                case DoStatementSyntax doStatement:
                    openParen = doStatement.OpenParenToken;
                    closeParen = doStatement.CloseParenToken;
                    return true;
                case SwitchStatementSyntax switchStatement:
                    openParen = switchStatement.OpenParenToken;
                    closeParen = switchStatement.CloseParenToken;
                    return true;
                case LockStatementSyntax lockStatement:
                    openParen = lockStatement.OpenParenToken;
                    closeParen = lockStatement.CloseParenToken;
                    return true;
                case UsingStatementSyntax usingStatement when !usingStatement.OpenParenToken.IsMissing:
                    openParen = usingStatement.OpenParenToken;
                    closeParen = usingStatement.CloseParenToken;
                    return true;
                case FixedStatementSyntax fixedStatement:
                    openParen = fixedStatement.OpenParenToken;
                    closeParen = fixedStatement.CloseParenToken;
                    return true;
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    openParen = parenthesizedExpression.OpenParenToken;
                    closeParen = parenthesizedExpression.CloseParenToken;
                    return true;
                case CastExpressionSyntax castExpression:
                    openParen = castExpression.OpenParenToken;
                    closeParen = castExpression.CloseParenToken;
                    return true;
                case TupleTypeSyntax tupleType:
                    openParen = tupleType.OpenParenToken;
                    closeParen = tupleType.CloseParenToken;
                    return true;
                case TupleExpressionSyntax tupleExpression:
                    openParen = tupleExpression.OpenParenToken;
                    closeParen = tupleExpression.CloseParenToken;
                    return true;
                default:
                    openParen = default;
                    closeParen = default;
                    return false;
            }
        }

        private static bool TryHasSpaceBetweenTokens(
            SyntaxTree tree,
            SyntaxToken leftToken,
            SyntaxToken rightToken,
            out bool hasSpace)
        {
            var betweenSpan = TextSpan.FromBounds(leftToken.Span.End, rightToken.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            if (betweenText.IndexOf('\n') >= 0 || betweenText.IndexOf('\r') >= 0)
            {
                hasSpace = false;
                return false;
            }

            foreach (var ch in betweenText)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    hasSpace = false;
                    return false;
                }
            }

            hasSpace = betweenText.Length > 0;
            return true;
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return false;

            return bool.TryParse(value.Trim(), out var parsed) && parsed;
        }
    }
}
