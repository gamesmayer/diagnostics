using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0122Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0122";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Object initializer members must be indented one step from the declaration",
            messageFormat: "Indent this member one step from the object instantiation",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Members inside the braces of an object initializer must be indented exactly one step to the right of the line containing the 'new' keyword.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            InitializerExpressionSyntax? initializer;
            SyntaxToken newKeyword;

            if (context.Node is ObjectCreationExpressionSyntax objectCreation)
            {
                initializer = objectCreation.Initializer;
                newKeyword = objectCreation.NewKeyword;
            }
            else if (context.Node is ImplicitObjectCreationExpressionSyntax implicitCreation)
            {
                initializer = implicitCreation.Initializer;
                newKeyword = implicitCreation.NewKeyword;
            }
            else
                return;

            if (initializer == null || !initializer.IsKind(SyntaxKind.ObjectInitializerExpression))
                return;

            if (initializer.Expressions.Count == 0)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var indentStep = GetIndentStep(context);

            var newKeywordLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
            var declarationIndent = GM0121Analyzer.CountLeadingWhitespace(sourceText.Lines[newKeywordLine].ToString());
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

                var actualIndent = GM0121Analyzer.CountLeadingWhitespace(sourceText.Lines[itemLine].ToString());
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
