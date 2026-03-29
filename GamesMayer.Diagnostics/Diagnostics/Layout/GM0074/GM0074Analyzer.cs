using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0074Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0074";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "#if / #else / #elif directive must not be followed by blank line",
            messageFormat: "Remove the blank line after the '{0}' directive",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A '#if', '#else', or '#elif' preprocessor directive must not be followed by a blank line.");

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
            string? lastOpeningDirective = null;

            for (int i = 0; i < text.Lines.Count; i++)
            {
                var lineText = text.ToString(text.Lines[i].Span);

                if (string.IsNullOrWhiteSpace(lineText))
                {
                    if (lastOpeningDirective != null)
                    {
                        var location = Location.Create(tree, new TextSpan(text.Lines[i].Start, 0));
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, lastOpeningDirective));
                    }
                }
                else
                {
                    lastOpeningDirective = GetOpeningDirective(lineText.TrimStart());
                }
            }
        }

        private static string? GetOpeningDirective(string trimmedLine)
        {
            if (trimmedLine.StartsWith("#if ") || trimmedLine == "#if")
                return "#if";
            if (trimmedLine.StartsWith("#else") && (trimmedLine.Length == 5 || trimmedLine[5] == ' ' || trimmedLine[5] == '/'))
                return "#else";
            if (trimmedLine.StartsWith("#elif ") || trimmedLine == "#elif")
                return "#elif";
            return null;
        }
    }
}
