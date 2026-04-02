using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0024Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0024";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Anonymous function incorrectly indented",
            messageFormat: "Correct the indentation to match the anonymous function's declaration level",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Braces of an anonymous function must be at the same indentation level as the declaration, and statements inside must be indented one step further.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            BlockSyntax? block;
            SyntaxToken anchorToken;

            switch (context.Node)
            {
                case SimpleLambdaExpressionSyntax simpleLambda when simpleLambda.Body is BlockSyntax b:
                    block = b;
                    anchorToken = simpleLambda.ArrowToken;
                    break;
                case ParenthesizedLambdaExpressionSyntax lambda when lambda.Body is BlockSyntax b:
                    block = b;
                    anchorToken = lambda.ArrowToken;
                    break;
                case AnonymousMethodExpressionSyntax anonMethod when anonMethod.Block != null:
                    block = anonMethod.Block;
                    anchorToken = anonMethod.DelegateKeyword;
                    break;
                default:
                    return;
            }

            if (anchorToken == default)
                return;

            var tree = context.Node.SyntaxTree;
            var openBrace = block.OpenBraceToken;
            var closeBrace = block.CloseBraceToken;

            if (openBrace.IsMissing || closeBrace.IsMissing)
                return;

            var anchorLine = tree.GetLineSpan(anchorToken.Span).StartLinePosition.Line;
            var openBraceLine = tree.GetLineSpan(openBrace.Span).StartLinePosition.Line;

            // Only check when { is on a different line from the anchor token
            if (openBraceLine == anchorLine)
                return;

            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);
            var expectedBraceIndent = GetLineIndentation(sourceText, anchorLine);
            var indentUnit = GetIndentUnit(expectedBraceIndent, indentSize);
            var expectedStatementIndent = expectedBraceIndent + indentUnit;

            // Check opening brace
            if (GetLineIndentation(sourceText, openBraceLine) != expectedBraceIndent)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, openBrace.GetLocation()));
            }

            // Check statements
            foreach (var statement in block.Statements)
            {
                var firstToken = statement.GetFirstToken();
                if (firstToken == default)
                    continue;

                var stmtLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;

                // Skip statements on the same line as the opening brace
                if (stmtLine == openBraceLine)
                    continue;

                if (GetLineIndentation(sourceText, stmtLine) != expectedStatementIndent)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
                }
            }

            // Check closing brace
            var closeBraceLine = tree.GetLineSpan(closeBrace.Span).StartLinePosition.Line;
            if (closeBraceLine != openBraceLine
                && GetLineIndentation(sourceText, closeBraceLine) != expectedBraceIndent)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, closeBrace.GetLocation()));
            }
        }

        internal static string GetLineIndentation(SourceText sourceText, int lineNumber)
        {
            var line = sourceText.Lines[lineNumber].ToString();
            var sb = new StringBuilder();
            foreach (char c in line)
            {
                if (c == ' ' || c == '\t') sb.Append(c);
                else break;
            }
            return sb.ToString();
        }

        internal static string GetIndentUnit(string baseIndent, int indentSize)
        {
            return baseIndent.Length > 0 && baseIndent[0] == '\t'
                ? "\t"
                : new string(' ', indentSize);
        }

        internal static int GetIndentSize(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue("indent_size", out var value) && int.TryParse(value, out var size) && size > 0)
                return size;
            return 4;
        }
    }
}
