using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0117Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0117";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each item in array initializer must be on its own line",
            messageFormat: "Place this item on its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Each item in the initializer of an array declaration must be on its own dedicated line.");

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

            var tree = context.Node.SyntaxTree;

            var openBraceLine = tree.GetLineSpan(initializer.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(initializer.CloseBraceToken.Span).StartLinePosition.Line;

            if (openBraceLine == closeBraceLine)
                return;

            var expressions = initializer.Expressions;

            var firstItemToken = expressions[0].GetFirstToken();
            if (firstItemToken != default)
            {
                var firstItemLine = tree.GetLineSpan(firstItemToken.Span).StartLinePosition.Line;
                if (firstItemLine == openBraceLine)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, expressions[0].GetLocation()));
            }

            for (int i = 1; i < expressions.Count; i++)
            {
                var current = expressions[i];
                var previous = expressions[i - 1];

                var currentToken = current.GetFirstToken();
                var previousToken = previous.GetFirstToken();

                if (currentToken == default || previousToken == default)
                    continue;

                var currentLine = tree.GetLineSpan(currentToken.Span).StartLinePosition.Line;
                var previousLine = tree.GetLineSpan(previousToken.Span).StartLinePosition.Line;

                if (currentLine == previousLine)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, current.GetLocation()));
            }
        }
    }
}
