using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0015Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0015";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank line between control structure clauses",
            messageFormat: "Remove the blank line between control structure clauses",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Control structure clauses (else, catch, finally, case, default) must not be preceded by a blank line.");

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

                if (!string.IsNullOrWhiteSpace(lineText))
                    continue;

                // Find the next non-blank line
                int j = i + 1;
                while (j < text.Lines.Count && string.IsNullOrWhiteSpace(text.ToString(text.Lines[j].Span)))
                    j++;

                if (j < text.Lines.Count)
                {
                    var nextLine = text.ToString(text.Lines[j].Span).TrimStart();
                    if (IsControlStructureContinuation(nextLine))
                    {
                        var location = Location.Create(tree, new TextSpan(line.Start, 0));
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                    }
                }
            }
        }

        private static bool IsControlStructureContinuation(string trimmedLine)
        {
            return StartsWithKeyword(trimmedLine, "else") ||
                   StartsWithKeyword(trimmedLine, "catch") ||
                   StartsWithKeyword(trimmedLine, "finally") ||
                   StartsWithKeyword(trimmedLine, "case") ||
                   StartsWithKeyword(trimmedLine, "default");
        }

        private static bool StartsWithKeyword(string line, string keyword)
        {
            if (!line.StartsWith(keyword))
                return false;
            if (line.Length == keyword.Length)
                return true;
            char next = line[keyword.Length];
            return !char.IsLetterOrDigit(next) && next != '_';
        }
    }
}
