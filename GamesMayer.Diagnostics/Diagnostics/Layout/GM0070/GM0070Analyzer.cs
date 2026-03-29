using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0070Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0070";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Each accessor must be on its own line in non-autoimplemented properties",
            messageFormat: "Move the accessor to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In a property with non-autoimplemented accessors, each accessor must be on its own line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax)context.Node;
            var accessorList = property.AccessorList;

            if (accessorList == null || accessorList.Accessors.Count < 2)
                return;

            bool hasNonAutoAccessor = accessorList.Accessors.Any(
                a => a.Body != null || a.ExpressionBody != null);

            if (!hasNonAutoAccessor)
                return;

            var tree = context.Node.SyntaxTree;

            for (int i = 1; i < accessorList.Accessors.Count; i++)
            {
                var prev = accessorList.Accessors[i - 1];
                var curr = accessorList.Accessors[i];

                var prevEndLine = tree.GetLineSpan(prev.Span).EndLinePosition.Line;
                var currStartLine = tree.GetLineSpan(curr.Span).StartLinePosition.Line;

                if (prevEndLine == currStartLine)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, curr.GetLocation()));
            }
        }
    }
}
