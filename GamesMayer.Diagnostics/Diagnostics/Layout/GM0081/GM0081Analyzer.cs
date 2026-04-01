using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0081Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0081";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Case labels must be indented one step right from the switch keyword",
            messageFormat: "Indent the case label one step right from the switch keyword",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Case labels in a switch statement must be indented exactly one step right from the switch keyword.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SwitchStatement);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;
            var tree = context.Node.SyntaxTree;

            var switchCol = tree.GetLineSpan(switchStatement.SwitchKeyword.Span).StartLinePosition.Character;
            var indentStep = DetectIndentStep(switchStatement, tree, switchCol);
            var expectedCol = switchCol + indentStep;

            foreach (var section in switchStatement.Sections)
            {
                foreach (var label in section.Labels)
                {
                    var labelToken = label.GetFirstToken();
                    var labelCol = tree.GetLineSpan(labelToken.Span).StartLinePosition.Character;

                    if (labelCol != expectedCol)
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, label.GetLocation()));
                }
            }
        }

        internal static int DetectIndentStep(SwitchStatementSyntax switchStatement, SyntaxTree tree, int switchCol)
        {
            if (switchStatement.Parent is BlockSyntax block && block.Parent != null)
            {
                var parentCol = tree.GetLineSpan(block.Parent.GetFirstToken().Span).StartLinePosition.Character;
                var step = switchCol - parentCol;

                if (step > 0)
                    return step;
            }

            return 4;
        }
    }
}
