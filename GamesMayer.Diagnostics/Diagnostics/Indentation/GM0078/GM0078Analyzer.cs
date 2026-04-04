using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0078Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0078";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Block brace must be indented at the declaration level",
            messageFormat: "Indent this brace to match the declaration indentation",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Opening and closing braces of a block must be indented at the same level as the containing declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(AnalyzeDeclarationBraces,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            if (block.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());

            CheckBrace(context, sourceText, tree, block.OpenBraceToken, declarationLine, declarationIndent);
            CheckBrace(context, sourceText, tree, block.CloseBraceToken, declarationLine, declarationIndent);
        }

        private static void AnalyzeDeclarationBraces(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken openBrace, closeBrace;

            switch (context.Node)
            {
                case NamespaceDeclarationSyntax ns:
                    openBrace = ns.OpenBraceToken;
                    closeBrace = ns.CloseBraceToken;
                    break;
                case ClassDeclarationSyntax cls:
                    openBrace = cls.OpenBraceToken;
                    closeBrace = cls.CloseBraceToken;
                    break;
                default:
                    return;
            }

            if (context.Node.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var declarationFirstToken = context.Node.GetFirstToken();
            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());

            CheckBrace(context, sourceText, tree, openBrace, declarationLine, declarationIndent);
            CheckBrace(context, sourceText, tree, closeBrace, declarationLine, declarationIndent);
        }

        private static void CheckBrace(
            SyntaxNodeAnalysisContext context,
            SourceText sourceText,
            SyntaxTree tree,
            SyntaxToken braceToken,
            int declarationLine,
            int declarationIndent)
        {
            if (braceToken.IsMissing)
                return;

            var braceLine = tree.GetLineSpan(braceToken.Span).StartLinePosition.Line;
            if (braceLine == declarationLine)
                return;

            var actualIndent = CountLeadingWhitespace(sourceText.Lines[braceLine].ToString());
            if (actualIndent != declarationIndent)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, braceToken.GetLocation()));
            }
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
        }
    }
}
