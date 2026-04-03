using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0123Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0123";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each member in object initializer must be on its own line",
            messageFormat: "Place this member on its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Each member in an object initializer must be on its own dedicated line.");

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

            if (context.Node is ObjectCreationExpressionSyntax objectCreation)
                initializer = objectCreation.Initializer;
            else if (context.Node is ImplicitObjectCreationExpressionSyntax implicitCreation)
                initializer = implicitCreation.Initializer;
            else
                return;

            if (initializer == null || !initializer.IsKind(SyntaxKind.ObjectInitializerExpression))
                return;

            if (initializer.Expressions.Count == 0)
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
