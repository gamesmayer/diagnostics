using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0075Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0075";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "#endif / #else / #elif directive must not be preceded by blank line",
            messageFormat: "Remove the blank line before the '{0}' directive",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A '#endif', '#else', or '#elif' preprocessor directive must not be preceded by a blank line.");

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
                var lineText = text.ToString(text.Lines[i].Span);
                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                // Find the next non-blank line
                int j = i + 1;
                while (j < text.Lines.Count && string.IsNullOrWhiteSpace(text.ToString(text.Lines[j].Span)))
                    j++;

                if (j < text.Lines.Count)
                {
                    var directive = GetClosingDirective(text.ToString(text.Lines[j].Span).TrimStart());
                    if (directive != null)
                    {
                        var location = Location.Create(tree, new TextSpan(text.Lines[i].Start, 0));
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, directive));
                    }
                }
            }
        }

        private static string? GetClosingDirective(string trimmedLine)
        {
            if (trimmedLine.StartsWith("#endif") && (trimmedLine.Length == 6 || trimmedLine[6] == ' ' || trimmedLine[6] == '/'))
                return "#endif";
            if (trimmedLine.StartsWith("#else") && (trimmedLine.Length == 5 || trimmedLine[5] == ' ' || trimmedLine[5] == '/'))
                return "#else";
            if (trimmedLine.StartsWith("#elif ") || trimmedLine == "#elif")
                return "#elif";
            return null;
        }
    }
}
