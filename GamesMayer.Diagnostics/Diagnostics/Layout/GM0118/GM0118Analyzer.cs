using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0118Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0118";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Comma must be on the same line as the previous token",
            messageFormat: "Move the comma to the end of the previous line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Commas must be placed at the end of the preceding item, not at the beginning of the next line or on their own line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.ArgumentList,
                SyntaxKind.ParameterList,
                SyntaxKind.BracketedArgumentList,
                SyntaxKind.BracketedParameterList,
                SyntaxKind.AttributeArgumentList,
                SyntaxKind.TypeArgumentList,
                SyntaxKind.TypeParameterList,
                SyntaxKind.ArrayInitializerExpression,
                SyntaxKind.ObjectInitializerExpression,
                SyntaxKind.CollectionInitializerExpression,
                SyntaxKind.ComplexElementInitializerExpression,
                SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var tree = context.Node.SyntaxTree;

            foreach (var child in context.Node.ChildNodesAndTokens())
            {
                if (!child.IsToken || !child.AsToken().IsKind(SyntaxKind.CommaToken))
                    continue;

                var comma = child.AsToken();
                var previousToken = comma.GetPreviousToken();
                if (previousToken == default)
                    continue;

                var commaLine = tree.GetLineSpan(comma.Span).StartLinePosition.Line;
                var previousLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;

                if (commaLine != previousLine)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, comma.GetLocation()));
            }
        }
    }
}
