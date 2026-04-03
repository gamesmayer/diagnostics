using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0116Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0116";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Array initializer items must be indented one step from the declaration",
            messageFormat: "Indent this item one step from the array declaration",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Items inside the braces of an array initializer must be indented exactly one step to the right of the containing declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode,
                SyntaxKind.ArrayCreationExpression,
                SyntaxKind.ImplicitArrayCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            InitializerExpressionSyntax? initializer;

            if (context.Node is ArrayCreationExpressionSyntax arrayCreation)
                initializer = arrayCreation.Initializer;
            else if (context.Node is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
                initializer = implicitArrayCreation.Initializer;
            else
                return;

            if (initializer == null || initializer.Expressions.Count == 0)
                return;

            if (context.Node.Parent is not EqualsValueClauseSyntax)
                return;

            var declarationFirstToken = GM0115Analyzer.FindDeclarationFirstToken(context.Node);
            if (declarationFirstToken == default)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentStep = GetIndentStep(context);

            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationIndent = GM0115Analyzer.CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());
            var expectedIndent = declarationIndent + indentStep;

            var openBraceLine = tree.GetLineSpan(initializer.OpenBraceToken.Span).StartLinePosition.Line;

            foreach (var expression in initializer.Expressions)
            {
                var firstToken = expression.GetFirstToken();
                if (firstToken == default)
                    continue;

                var itemLine = tree.GetLineSpan(firstToken.Span).StartLinePosition.Line;
                if (itemLine == openBraceLine)
                    continue;

                var actualIndent = GM0115Analyzer.CountLeadingWhitespace(sourceText.Lines[itemLine].ToString());
                if (actualIndent != expectedIndent)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, firstToken.GetLocation()));
            }
        }

        internal static int GetIndentStep(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (options.TryGetValue("indent_size", out var value) && int.TryParse(value, out var size) && size > 0)
                return size;
            return 4;
        }
    }
}
