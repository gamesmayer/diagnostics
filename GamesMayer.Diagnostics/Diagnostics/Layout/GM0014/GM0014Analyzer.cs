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
            title: "Namespace declaration must be on a single line",
            messageFormat: "Place the namespace declaration on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The namespace keyword and its identifier must appear on the same line.");

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
            var nsDecl = (BaseNamespaceDeclarationSyntax)context.Node;
            var nsKeyword = nsDecl.NamespaceKeyword;
            var nameNode = nsDecl.Name;

            var keywordLine = nsKeyword.GetLocation().GetLineSpan().StartLinePosition.Line;
            var nameLastLine = nameNode.GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;

            if (keywordLine == nameLastLine)
                return;

            var location = Location.Create(
                context.Node.SyntaxTree,
                TextSpan.FromBounds(nsKeyword.SpanStart, nameNode.GetLastToken().Span.End));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
