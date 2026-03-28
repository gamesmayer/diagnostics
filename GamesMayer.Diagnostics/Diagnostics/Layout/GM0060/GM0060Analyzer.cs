using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0060Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0060";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Property/field access chain must be on a single line",
            messageFormat: "Write the property/field access chain on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A property or field access chain must be written entirely on a single line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            // Skip method call references — those are fluent chains handled by GM0039.
            if (memberAccess.Parent is InvocationExpressionSyntax inv && inv.Expression == memberAccess)
                return;

            // Only property/field chains rooted at an identifier or 'this'.
            if (!IsPropertyChain(memberAccess))
                return;

            var tree = context.Node.SyntaxTree;
            var exprLastToken = memberAccess.Expression.GetLastToken();
            var dotToken = memberAccess.OperatorToken;
            var nameFirstToken = memberAccess.Name.GetFirstToken();

            var exprLastLine = tree.GetLineSpan(exprLastToken.Span).EndLinePosition.Line;
            var dotLine = tree.GetLineSpan(dotToken.Span).StartLinePosition.Line;
            var nameFirstLine = tree.GetLineSpan(nameFirstToken.Span).StartLinePosition.Line;

            // Dot at start of a new line: expression ended on a previous line.
            bool dotOnNewLine = exprLastLine < dotLine;

            // Trailing dot: dot at end of a line, name on the next line.
            // Only flag in assignment targets where GM0039 does not apply.
            bool trailingDot = dotLine < nameFirstLine && IsInsideAssignmentLeft(memberAccess);

            if (!dotOnNewLine && !trailingDot)
                return;

            // Report from the dot through the name so spans are non-overlapping across a chain.
            var span = TextSpan.FromBounds(dotToken.SpanStart, nameFirstToken.Span.End);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, span)));
        }

        private static bool IsPropertyChain(ExpressionSyntax expression)
        {
            var current = expression;
            while (true)
            {
                if (current is MemberAccessExpressionSyntax ma)
                    current = ma.Expression;
                else if (current is IdentifierNameSyntax || current is ThisExpressionSyntax)
                    return true;
                else
                    return false;
            }
        }

        private static bool IsInsideAssignmentLeft(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is AssignmentExpressionSyntax assignment && assignment.Left.Contains(node))
                    return true;
                if (current is StatementSyntax)
                    return false;
                current = current.Parent;
            }
            return false;
        }
    }
}
