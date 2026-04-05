using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0119Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0119";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank lines between array initializer items",
            messageFormat: "Remove the blank line between array initializer items",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Items in an array initializer must not be separated by blank lines.");

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

            if (initializer == null || initializer.Expressions.Count < 2)
                return;

            if (context.Node.Parent is not EqualsValueClauseSyntax &&
                context.Node.Parent is not ArgumentSyntax)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);
            var expressions = initializer.Expressions;

            for (int i = 1; i < expressions.Count; i++)
            {
                var previousLastToken = expressions[i - 1].GetLastToken();
                var currentFirstToken = expressions[i].GetFirstToken();

                if (previousLastToken == default || currentFirstToken == default)
                    continue;

                var previousLastLine = tree.GetLineSpan(previousLastToken.Span).EndLinePosition.Line;
                var currentFirstLine = tree.GetLineSpan(currentFirstToken.Span).StartLinePosition.Line;

                if (currentFirstLine <= previousLastLine + 1)
                    continue;

                for (var line = previousLastLine + 1; line < currentFirstLine; line++)
                {
                    var lineText = sourceText.Lines[line].ToString();
                    if (!string.IsNullOrWhiteSpace(lineText))
                        continue;

                    var lineSpan = sourceText.Lines[line].SpanIncludingLineBreak;
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, lineSpan)));
                }
            }
        }
    }
}
