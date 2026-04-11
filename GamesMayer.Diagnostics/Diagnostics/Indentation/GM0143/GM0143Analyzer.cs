using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0143Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0143";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each clause in a compound statement must start on its own line",
            messageFormat: "Move '{0}' to its own line",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In compound statements (if/else if/else and try/catch/finally), each clause keyword must appear on its own line and must not follow the closing brace of the preceding block on the same line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeAction(AnalyzeFinallyClause, SyntaxKind.FinallyClause);
        }

        private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;
            if (elseClause.Parent is not IfStatementSyntax ifStatement)
                return;

            if (ifStatement.Statement is not BlockSyntax block)
                return;

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(block.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(block.CloseBraceToken.Span).StartLinePosition.Line;

            if (openBraceLine == closeBraceLine)
                return;

            var elseKeywordLine = tree.GetLineSpan(elseClause.ElseKeyword.Span).StartLinePosition.Line;
            if (elseKeywordLine == closeBraceLine)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, elseClause.ElseKeyword.GetLocation(), "else"));
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            if (catchClause.Parent is not TryStatementSyntax tryStatement)
                return;

            BlockSyntax precedingBlock;
            int catchIndex = tryStatement.Catches.IndexOf(catchClause);
            if (catchIndex == 0)
                precedingBlock = tryStatement.Block;
            else
                precedingBlock = tryStatement.Catches[catchIndex - 1].Block;

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(precedingBlock.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(precedingBlock.CloseBraceToken.Span).StartLinePosition.Line;

            if (openBraceLine == closeBraceLine)
                return;

            var catchKeywordLine = tree.GetLineSpan(catchClause.CatchKeyword.Span).StartLinePosition.Line;
            if (catchKeywordLine == closeBraceLine)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, catchClause.CatchKeyword.GetLocation(), "catch"));
        }

        private static void AnalyzeFinallyClause(SyntaxNodeAnalysisContext context)
        {
            var finallyClause = (FinallyClauseSyntax)context.Node;
            if (finallyClause.Parent is not TryStatementSyntax tryStatement)
                return;

            BlockSyntax precedingBlock;
            if (tryStatement.Catches.Count > 0)
                precedingBlock = tryStatement.Catches[tryStatement.Catches.Count - 1].Block;
            else
                precedingBlock = tryStatement.Block;

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(precedingBlock.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(precedingBlock.CloseBraceToken.Span).StartLinePosition.Line;

            if (openBraceLine == closeBraceLine)
                return;

            var finallyKeywordLine = tree.GetLineSpan(finallyClause.FinallyKeyword.Span).StartLinePosition.Line;
            if (finallyKeywordLine == closeBraceLine)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, finallyClause.FinallyKeyword.GetLocation(), "finally"));
        }
    }
}
