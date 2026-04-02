using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0112Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0112";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between consecutive variables",
            messageFormat: "Remove the blank line between consecutive variable declarations",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Consecutive local variable declarations must not be separated by blank lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeDeclaration, SyntaxKind.LocalDeclarationStatement);
        }

        private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (LocalDeclarationStatementSyntax)context.Node;
            if (declaration.Parent is not BlockSyntax block)
                return;

            var statementIndex = block.Statements.IndexOf(declaration);
            if (statementIndex <= 0)
                return;

            if (block.Statements[statementIndex - 1] is not LocalDeclarationStatementSyntax previousDeclaration)
                return;

            var syntaxTree = declaration.SyntaxTree;
            var sourceText = syntaxTree.GetText(context.CancellationToken);

            var previousLastLine = syntaxTree.GetLineSpan(previousDeclaration.GetLastToken().Span).EndLinePosition.Line;
            var currentFirstLine = syntaxTree.GetLineSpan(declaration.GetFirstToken().Span).StartLinePosition.Line;

            if (currentFirstLine <= previousLastLine + 1)
                return;

            for (var line = previousLastLine + 1; line < currentFirstLine; line++)
            {
                var lineText = sourceText.Lines[line].ToString();
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(syntaxTree, lineSpan)));
            }
        }
    }
}