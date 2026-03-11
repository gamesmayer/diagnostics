using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0010Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0010";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No two consecutive blank lines",
            messageFormat: "Remove the consecutive blank line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Code must not contain two or more consecutive blank lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(Analyze);
        }

        private static void Analyze(SyntaxTreeAnalysisContext context)
        {
            var tree = context.Tree;
            var text = tree.GetText(context.CancellationToken);
            int consecutiveBlankLines = 0;

            for (int i = 0; i < text.Lines.Count; i++)
            {
                var line = text.Lines[i];
                var lineText = text.ToString(line.Span);

                if (string.IsNullOrWhiteSpace(lineText))
                {
                    consecutiveBlankLines++;

                    if (consecutiveBlankLines >= 2)
                    {
                        var location = Location.Create(tree, new TextSpan(line.Start, 0));
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                    }
                }
                else
                {
                    consecutiveBlankLines = 0;
                }
            }
        }
    }
}
