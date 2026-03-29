using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0072Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0072";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Blank line required before #if directive",
            messageFormat: "Add a blank line before the '#if' directive",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A '#if' preprocessor directive must be preceded by a blank line unless it is the first statement in its enclosing block.");

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
                if (!trivia.IsKind(SyntaxKind.IfDirectiveTrivia))
                    continue;

                int lineNumber = sourceText.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber;

                if (lineNumber == 0)
                    continue;

                if (IsFirstInBlock(sourceText, lineNumber))
                    continue;

                if (!HasBlankLineBefore(sourceText, lineNumber)
                    && trivia.GetStructure() is IfDirectiveTriviaSyntax ifDirective)
                {
                    var location = Location.Create(
                        context.Tree,
                        TextSpan.FromBounds(ifDirective.HashToken.SpanStart, ifDirective.IfKeyword.Span.End));
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
                }
            }
        }

        private static bool IsFirstInBlock(SourceText sourceText, int lineNumber)
        {
            for (int i = lineNumber - 1; i >= 0; i--)
            {
                var lineText = sourceText.Lines[i].ToString().Trim();

                if (lineText.Length == 0)
                    continue;

                return lineText.EndsWith("{");
            }

            return true;
        }

        private static bool HasBlankLineBefore(SourceText sourceText, int lineNumber)
        {
            return sourceText.Lines[lineNumber - 1].ToString().Trim().Length == 0;
        }
    }
}
