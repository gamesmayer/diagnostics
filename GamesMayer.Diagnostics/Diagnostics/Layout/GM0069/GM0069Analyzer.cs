using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0069Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0069";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Properties not autoimplemented cannot be single line statements",
            messageFormat: "Expand the property accessor list across multiple lines",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A property with non-autoimplemented accessors must not have its accessor list on a single line.");

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

            if (accessorList == null)
                return;

            bool hasNonAutoAccessor = accessorList.Accessors.Any(
                a => a.Body != null || a.ExpressionBody != null);

            if (!hasNonAutoAccessor)
                return;

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(accessorList.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(accessorList.CloseBraceToken.Span).EndLinePosition.Line;

            if (openBraceLine != closeBraceLine)
                return;

            var span = TextSpan.FromBounds(accessorList.OpenBraceToken.SpanStart, accessorList.CloseBraceToken.Span.End);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, span)));
        }
    }
}
