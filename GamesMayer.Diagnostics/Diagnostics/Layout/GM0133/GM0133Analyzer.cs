using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0133Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0133";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Constructor initializer must be on its own line",
            messageFormat: "Move the constructor initializer to its own line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Constructor initializer clauses ('base(...)' or 'this(...)') must be on their own line, not on the constructor declaration line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var initializer = (ConstructorInitializerSyntax)context.Node;
            var keyword = initializer.ThisOrBaseKeyword;

            var tree = context.Node.SyntaxTree;
            var colonLine = tree.GetLineSpan(initializer.ColonToken.Span).EndLinePosition.Line;
            var keywordLine = tree.GetLineSpan(keyword.Span).StartLinePosition.Line;

            if (colonLine != keywordLine)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, initializer.ThisOrBaseKeyword.GetLocation()));
        }
    }
}
