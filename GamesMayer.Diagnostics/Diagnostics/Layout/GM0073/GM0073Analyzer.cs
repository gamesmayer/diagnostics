using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0073Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0073";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Blank line required after #endif directive",
            messageFormat: "Add a blank line after the '#endif' directive",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A '#endif' preprocessor directive must be followed by a blank line unless it is the last statement in its enclosing block.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeTree);
        }

        private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);
            var sourceText = context.Tree.GetText(context.CancellationToken);

            foreach (var trivia in root.DescendantTrivia())
            {
                if (!trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
                    continue;

                int lineNumber = sourceText.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber;

                if (lineNumber >= sourceText.Lines.Count - 1)
                    continue;

                if (IsLastInBlock(sourceText, lineNumber))
                    continue;

                if (!HasBlankLineAfter(sourceText, lineNumber)
                    && trivia.GetStructure() is EndIfDirectiveTriviaSyntax endIfDirective)
                {
                    var location = Location.Create(
                        context.Tree,
                        TextSpan.FromBounds(endIfDirective.HashToken.SpanStart, endIfDirective.EndIfKeyword.Span.End));
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                }
            }
        }

        private static bool IsLastInBlock(SourceText sourceText, int lineNumber)
        {
            for (int i = lineNumber + 1; i < sourceText.Lines.Count; i++)
            {
                var lineText = sourceText.Lines[i].ToString().Trim();

                if (lineText.Length == 0)
                    continue;

                return lineText.StartsWith("}");
            }

            return true;
        }

        private static bool HasBlankLineAfter(SourceText sourceText, int lineNumber)
        {
            if (lineNumber + 1 >= sourceText.Lines.Count)
                return true;

            return sourceText.Lines[lineNumber + 1].ToString().Trim().Length == 0;
        }
    }
}
