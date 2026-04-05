using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0115Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0115";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Constructor initializer must be indented one step from the declaration",
            messageFormat: "Indent this constructor initializer one step from the declaration",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Constructor initializer clauses ('base(...)' or 'this(...)') must be indented exactly one step to the right of the constructor declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var initializer = (ConstructorInitializerSyntax)context.Node;
            var keyword = initializer.ThisOrBaseKeyword;

            var tree = initializer.SyntaxTree;
            var colonLine = tree.GetLineSpan(initializer.ColonToken.Span).EndLinePosition.Line;
            var keywordLine = tree.GetLineSpan(keyword.Span).StartLinePosition.Line;

            if (colonLine == keywordLine)
            {
                return;
            }

            var constructorDeclaration = initializer.Parent as ConstructorDeclarationSyntax;
            if (constructorDeclaration == null)
            {
                return;
            }

            var sourceText = tree.GetText(context.CancellationToken);
            var declarationLine = tree.GetLineSpan(constructorDeclaration.GetFirstToken().Span).StartLinePosition.Line;
            var baseIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, declarationLine);
            var expectedIndentation = GM0049Analyzer.GetExpectedIndentation(baseIndentation, GetIndentSize(context));
            var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, keywordLine);

            if (actualIndentation != expectedIndentation)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, keyword.GetLocation()));
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
