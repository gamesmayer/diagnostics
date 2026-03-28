using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0067Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0067";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Block statement must be followed by a blank line",
            messageFormat: "Add a blank line after the block statement",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A block-producing control flow statement (if, switch, for, foreach, while, do, try, lock, using) must be followed by exactly one blank line when it is not the last statement in its enclosing block.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.IfStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.WhileStatement,
                SyntaxKind.DoStatement,
                SyntaxKind.TryStatement,
                SyntaxKind.LockStatement,
                SyntaxKind.UsingStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var statement = (StatementSyntax)context.Node;

            if (statement.Parent is not BlockSyntax block)
                return;

            var index = block.Statements.IndexOf(statement);

            if (index >= block.Statements.Count - 1)
                return;

            var nextStatement = block.Statements[index + 1];
            var lastToken = statement.GetLastToken();
            var tree = context.Node.SyntaxTree;

            var statementEndLine = tree.GetLineSpan(lastToken.Span).EndLinePosition.Line;
            var nextStatementStartLine = tree.GetLineSpan(nextStatement.GetFirstToken().Span).StartLinePosition.Line;

            var linesBetween = nextStatementStartLine - statementEndLine;

            if (linesBetween < 2)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, lastToken.GetLocation()));
        }
    }
}
