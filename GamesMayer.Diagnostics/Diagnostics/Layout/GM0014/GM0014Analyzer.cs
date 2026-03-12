using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0014Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0014";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Namespace identifier must be on a single line",
            messageFormat: "Place the namespace identifier on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Namespace identifiers must not span multiple lines. All segments of a qualified namespace name must appear on the same line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.FileScopedNamespaceDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var nameNode = context.Node is NamespaceDeclarationSyntax ns
                ? ns.Name
                : ((FileScopedNamespaceDeclarationSyntax)context.Node).Name;

            var firstToken = nameNode.GetFirstToken();
            var lastToken = nameNode.GetLastToken();

            var startLine = firstToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            var endLine = lastToken.GetLocation().GetLineSpan().EndLinePosition.Line;

            if (startLine == endLine)
                return;

            var location = Location.Create(
                context.Node.SyntaxTree,
                TextSpan.FromBounds(firstToken.SpanStart, lastToken.Span.End));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
