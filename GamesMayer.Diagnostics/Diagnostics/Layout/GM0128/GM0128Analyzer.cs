using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0128Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0128";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "new keyword and type must be on the same line in object creation",
            messageFormat: "Move the type to the same line as 'new'",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In an object creation expression, the 'new' keyword and the type must be on the same line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            var tree = objectCreation.SyntaxTree;

            var newKeyword = objectCreation.NewKeyword;
            var typeFirstToken = objectCreation.Type.GetFirstToken();

            var newLine = tree.GetLineSpan(newKeyword.Span).EndLinePosition.Line;
            var typeLine = tree.GetLineSpan(typeFirstToken.Span).StartLinePosition.Line;

            if (typeLine != newLine)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, objectCreation.Type.GetLocation()));
            }
        }
    }
}
