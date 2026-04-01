using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0077Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0077";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Block contents must be indented one step from the block braces",
            messageFormat: "Indent the block content one step from the opening brace",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Statements inside a block must be indented by exactly one step relative to the indentation of the line containing the opening brace.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;
            if (block.Statements.Count == 0)
                return;

            if (block.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentSize = GetIndentSize(context);

            var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationLineText = sourceText.Lines[declarationLine].ToString();
            var declarationIndent = CountLeadingWhitespace(declarationLineText);
            var expectedIndent = declarationIndent + indentSize;

            var openBraceLine = tree.GetLineSpan(block.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var statement in block.Statements)
            {
                var firstToken = statement.GetFirstToken();
                if (firstToken == default)
                    continue;

                var statementLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                if (statementLine == openBraceLine)
                    continue;

                var statementLineText = sourceText.Lines[statementLine].ToString();
                var actualIndent = CountLeadingWhitespace(statementLineText);

                if (actualIndent != expectedIndent)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
                }
            }
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
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
