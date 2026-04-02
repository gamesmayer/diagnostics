using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0033Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0033";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Opening parenthesis indentation in multi-line argument/parameter list",
            messageFormat: "Indent opening parenthesis at the same level as the declaration",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When a multi-line argument or parameter list uses Allman-style opening parenthesis placement, the opening parenthesis line must be indented at the same level as the declaration line.");

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
            AnalyzeList(context, argumentList.Arguments, argumentList.OpenParenToken, argumentList.CloseParenToken);
        }

        private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context)
        {
            var parameterList = (ParameterListSyntax)context.Node;
            AnalyzeList(context, parameterList.Parameters, parameterList.OpenParenToken, parameterList.CloseParenToken);
        }

        private static void AnalyzeList<TNode>(
            SyntaxNodeAnalysisContext context,
            SeparatedSyntaxList<TNode> items,
            SyntaxToken openParenToken,
            SyntaxToken closeParenToken)
            where TNode : SyntaxNode
        {
            if (items.Count == 0)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var openParenLine = tree.GetLineSpan(openParenToken.Span).StartLinePosition.Line;
            var closeParenLine = tree.GetLineSpan(closeParenToken.Span).StartLinePosition.Line;

            // The rule applies only to multi-line lists.
            if (openParenLine == closeParenLine)
            {
                return;
            }

            var previousToken = openParenToken.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return;
            }

            var declarationLine = tree.GetLineSpan(previousToken.Span).EndLinePosition.Line;

            // Allman-only scope: opening parenthesis must be on a separate line.
            if (openParenLine == declarationLine)
            {
                return;
            }

            var sourceText = tree.GetText(context.CancellationToken);
            if (!IsLineOnlyOpeningParenthesis(sourceText, openParenLine))
            {
                return;
            }

            var expectedIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, declarationLine);
            var actualIndentation = GM0023Analyzer.GetActualLineIndentation(sourceText, openParenLine);

            if (actualIndentation != expectedIndentation)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, openParenToken.GetLocation()));
            }
        }

        private static bool IsLineOnlyOpeningParenthesis(Microsoft.CodeAnalysis.Text.SourceText sourceText, int lineNumber)
        {
            var lineText = sourceText.Lines[lineNumber].ToString().Trim();
            return lineText == "(";
        }
    }
}