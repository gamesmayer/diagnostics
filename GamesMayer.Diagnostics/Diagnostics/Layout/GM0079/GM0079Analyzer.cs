using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0079Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0079";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No more than one space between tokens",
            messageFormat: "Reduce multiple spaces between tokens to a single space",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Adjacent tokens on the same line must be separated by at most one space.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);
            var sourceText = context.Tree.GetText(context.CancellationToken);

            SyntaxToken? previousToken = null;
            foreach (var currentToken in root.DescendantTokens())
            {
                if (previousToken is not SyntaxToken previous)
                {
                    previousToken = currentToken;
                    continue;
                }

                if (previous.IsMissing || currentToken.IsMissing)
                {
                    previousToken = currentToken;
                    continue;
                }

                var separatorSpan = TextSpan.FromBounds(previous.Span.End, currentToken.SpanStart);
                if (separatorSpan.Length <= 1)
                {
                    previousToken = currentToken;
                    continue;
                }

                var separatorText = sourceText.ToString(separatorSpan);
                if (!ContainsOnlySpaces(separatorText) || separatorText.Length <= 1)
                {
                    previousToken = currentToken;
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(context.Tree, separatorSpan)));
                previousToken = currentToken;
            }
        }

        private static bool ContainsOnlySpaces(string text)
        {
            foreach (var ch in text)
            {
                if (ch != ' ')
                    return false;
            }

            return true;
        }
    }
}
