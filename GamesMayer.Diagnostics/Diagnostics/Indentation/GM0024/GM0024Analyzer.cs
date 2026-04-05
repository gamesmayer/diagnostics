using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0024Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0024";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Parent type in inheritance must be indented one step from the declaration",
            messageFormat: "Indent this parent type one step from the declaration",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Each parent class or interface in class and interface inheritance must be indented exactly one step to the right of the declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBaseList, SyntaxKind.BaseList);
        }

        private static void AnalyzeBaseList(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;
            if (baseList.Parent is not ClassDeclarationSyntax and not InterfaceDeclarationSyntax)
            {
                return;
            }

            if (baseList.Types.Count == 0)
            {
                return;
            }

            var tree = baseList.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var declarationLine = tree.GetLineSpan(baseList.Parent.GetFirstToken().Span).StartLinePosition.Line;
            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, declarationLine);
            var expectedIndentation = GM0049Analyzer.GetExpectedIndentation(baseIndentation, GetIndentSize(context));

            for (int i = 0; i < baseList.Types.Count; i++)
            {
                var baseType = baseList.Types[i];
                var firstToken = baseType.GetFirstToken();
                if (firstToken == default)
                {
                    continue;
                }

                var previousToken = i == 0
                    ? baseList.ColonToken
                    : baseList.Types.GetSeparator(i - 1);

                var previousTokenLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
                var currentTokenLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;

                if (previousTokenLine == currentTokenLine)
                {
                    continue;
                }

                var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, currentTokenLine);
                if (actualIndentation != expectedIndentation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, baseType.GetLocation()));
                }
            }
        }

        private static int GetIndentSize(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue("indent_size", out var value) && int.TryParse(value, out var size) && size > 0)
            {
                return size;
            }

            return 4;
        }
    }
}