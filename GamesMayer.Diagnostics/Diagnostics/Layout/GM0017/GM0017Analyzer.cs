using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0017Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0017";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Control structure clause declaration must be on a single line",
            messageFormat: "Write the control structure clause declaration on a single line, or extract the condition into a local variable",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Control structure clause declarations such as if (...), foreach (...), for (...), while (...), switch (...), catch (...), lock (...), and using (...) must be written on a single line. If readability suffers, extract the condition into a local variable and keep the clause declaration itself on one line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachVariableStatement);
            context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
            context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, SyntaxKind.WhileStatement);
            context.RegisterSyntaxNodeAction(AnalyzeDoStatement, SyntaxKind.DoStatement);
            context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeAction(AnalyzeLockStatement, SyntaxKind.LockStatement);
            context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
        }

        private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (IfStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.IfKeyword, node.CloseParenToken);
        }

        private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (CommonForEachStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.ForEachKeyword, node.CloseParenToken);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (ForStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.ForKeyword, node.CloseParenToken);
        }

        private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (WhileStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.WhileKeyword, node.CloseParenToken);
        }

        private static void AnalyzeDoStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (DoStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.WhileKeyword, node.CloseParenToken);
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (SwitchStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.SwitchKeyword, node.CloseParenToken);
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var node = (CatchClauseSyntax)context.Node;

            if (node.Filter != null)
            {
                ReportIfMultiLine(context, node.CatchKeyword, node.Filter.CloseParenToken);
            }
            else if (node.Declaration != null)
            {
                ReportIfMultiLine(context, node.CatchKeyword, node.Declaration.CloseParenToken);
            }
        }

        private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (LockStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.LockKeyword, node.CloseParenToken);
        }

        private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
        {
            var node = (UsingStatementSyntax)context.Node;
            ReportIfMultiLine(context, node.UsingKeyword, node.CloseParenToken);
        }

        private static void ReportIfMultiLine(SyntaxNodeAnalysisContext context, SyntaxToken startToken, SyntaxToken endToken)
        {
            var tree = context.Node.SyntaxTree;
            var startLine = tree.GetLineSpan(startToken.Span).StartLinePosition.Line;
            var endLine = tree.GetLineSpan(endToken.Span).EndLinePosition.Line;

            if (startLine != endLine)
            {
                var location = Location.Create(tree, TextSpan.FromBounds(startToken.SpanStart, endToken.Span.End));
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
            }
        }
    }
}