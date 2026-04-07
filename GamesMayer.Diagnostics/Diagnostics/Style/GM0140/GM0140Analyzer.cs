using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0140Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0140";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Unused using directive",
            messageFormat: "Remove unused 'using {0}'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: "Using directives that are not referenced by any code in the file should be removed.",
            customTags: new[] { WellKnownDiagnosticTags.Unnecessary });

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSemanticModelAction(Analyze);
        }

        private static void Analyze(SemanticModelAnalysisContext context)
        {
            var root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);
            if (root is not CompilationUnitSyntax compilationUnit)
                return;

            var compilerDiagnostics = context.SemanticModel.GetDiagnostics(cancellationToken: context.CancellationToken);

            foreach (var diagnostic in compilerDiagnostics)
            {
                if (diagnostic.Id != "CS8019")
                    continue;

                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                var usingDirective = node as UsingDirectiveSyntax
                    ?? node.AncestorsAndSelf().OfType<UsingDirectiveSyntax>().FirstOrDefault();

                if (usingDirective == null)
                    continue;

                var nameText = usingDirective.Alias != null
                    ? usingDirective.Alias.Name.ToString()
                    : usingDirective.Name?.ToString() ?? "?";

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    usingDirective.GetLocation(),
                    nameText));
            }
        }
    }
}
