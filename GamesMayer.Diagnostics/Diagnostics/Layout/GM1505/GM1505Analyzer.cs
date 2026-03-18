using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM1505Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM1505";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Opening brace must not be followed by blank line",
            messageFormat: "Remove the blank line after the opening brace",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "An opening brace within a C# element, statement, or expression must not be followed by a blank line.");

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
            bool lastNonBlankLineEndsWithOpenBrace = false;

            for (int i = 0; i < text.Lines.Count; i++)
            {
                var lineText = text.ToString(text.Lines[i].Span);

                if (string.IsNullOrWhiteSpace(lineText))
                {
                    if (lastNonBlankLineEndsWithOpenBrace)
                    {
                        var location = Location.Create(tree, new TextSpan(text.Lines[i].Start, 0));
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                    }
                }
                else
                {
                    var trimmed = lineText.TrimEnd();
                    lastNonBlankLineEndsWithOpenBrace = trimmed.Length > 0 && trimmed[trimmed.Length - 1] == '{';
                }
            }
        }
    }
}
