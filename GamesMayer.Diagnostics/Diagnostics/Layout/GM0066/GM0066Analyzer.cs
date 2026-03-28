using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0066Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0066";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Block statement must be preceded by blank line",
            messageFormat: "Add a blank line before the '{0}' statement",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A block-producing control flow statement (if, switch, for, foreach, while, do, try, lock, using) must be preceded by exactly one blank line when it is not the first statement in its enclosing block.");

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

            if (!(statement.Parent is BlockSyntax block))
                return;

            var statements = block.Statements;
            int index = statements.IndexOf(statement);

            if (index <= 0)
                return;

            var previousStatement = statements[index - 1];

            var previousEnd = previousStatement.GetLocation().GetLineSpan().EndLinePosition.Line;
            var currentStart = statement.GetLocation().GetLineSpan().StartLinePosition.Line;

            int blankLines = currentStart - previousEnd - 1;

            if (blankLines < 1)
            {
                string keyword = GetKeyword(statement);
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, statement.GetFirstToken().GetLocation(), keyword));
            }
        }

        private static string GetKeyword(StatementSyntax statement)
        {
            return statement.Kind() switch
            {
                SyntaxKind.IfStatement => "if",
                SyntaxKind.SwitchStatement => "switch",
                SyntaxKind.ForStatement => "for",
                SyntaxKind.ForEachStatement => "foreach",
                SyntaxKind.WhileStatement => "while",
                SyntaxKind.DoStatement => "do",
                SyntaxKind.TryStatement => "try",
                SyntaxKind.LockStatement => "lock",
                SyntaxKind.UsingStatement => "using",
                _ => statement.Kind().ToString()
            };
        }
    }
}
