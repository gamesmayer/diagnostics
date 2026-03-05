using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0001Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0001";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Blank line between using directives",
            messageFormat: "Remove the blank line between 'using {0}' and 'using {1}'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Consecutive using directives must not be separated by blank lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CompilationUnit);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = (CompilationUnitSyntax)context.Node;
            var usings = compilationUnit.Usings;

            for (int i = 1; i < usings.Count; i++)
            {
                UsingDirectiveSyntax current = usings[i];
                SyntaxTrivia blankLine = default;

                foreach (SyntaxTrivia trivia in current.GetLeadingTrivia())
                {
                    if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    {
                        blankLine = trivia;
                        break;
                    }
                }

                if (blankLine != default)
                {
                    UsingDirectiveSyntax previous = usings[i - 1];
                    string previousName = previous.Name?.ToString() ?? "?";
                    string currentName = current.Name?.ToString() ?? "?";

                    context.ReportDiagnostic(
                        Diagnostic.Create(Descriptor, blankLine.GetLocation(),
                            previousName, currentName));
                }
            }
        }
    }
}
