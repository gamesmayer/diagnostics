using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0030Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0030";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No blank line between declaration and opening parenthesis",
            messageFormat: "Remove the blank line between declaration and opening parenthesis",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "In argument or parameter lists where the opening parenthesis is on a separate line, there must not be blank lines between the declaration and the opening parenthesis.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeArgumentList, SyntaxKind.ArgumentList);
            context.RegisterSyntaxNodeAction(AnalyzeParameterList, SyntaxKind.ParameterList);
        }

        private static void AnalyzeArgumentList(SyntaxNodeAnalysisContext context)
        {
            var argumentList = (ArgumentListSyntax)context.Node;
            AnalyzeList(context, argumentList.OpenParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            AnalyzeList(context, parameterList.OpenParenToken);
        }

        private static void AnalyzeList(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openParenToken)
        {
            var tree = context.Node.SyntaxTree;
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;

            var previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return;
            }

            var declarationLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;
            if (openParenLine <= declarationLine + 1)
            {
                return;
            }

            var sourceText = tree.GetText(context.CancellationToken);
            for (int lineIndex = declarationLine + 1; lineIndex < openParenLine; lineIndex++)
            {
                var lineText = sourceText.ToString(sourceText.Lines[lineIndex].Span);
                if (!string.IsNullOrWhiteSpace(lineText))
                {
                    return;
                }
            }

            var firstBlankLine = sourceText.Lines[declarationLine + 1];
            var location = Location.Create(tree, new TextSpan(firstBlankLine.Start, 0));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
