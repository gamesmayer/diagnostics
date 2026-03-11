using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0011Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0011";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank line at the beginning of file",
            messageFormat: "Remove the blank line at the beginning of the file",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Files must not begin with a blank line.");

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

            for (int i = 0; i < text.Lines.Count; i++)
            {
                var line = text.Lines[i];
                var lineText = text.ToString(line.Span);

                if (string.IsNullOrWhiteSpace(lineText))
                {
                    var location = Location.Create(tree, new TextSpan(line.Start, 0));
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                }
                else
                {
                    break;
                }
            }
        }
    }
}
