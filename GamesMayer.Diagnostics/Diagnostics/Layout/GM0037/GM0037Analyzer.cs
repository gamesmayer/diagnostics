using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0037Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0037";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Parameter declaration header must be on a single line",
            messageFormat: "Write the parameter declaration header on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter declaration headers (modifiers, type, identifier, and assignment operator when present) must be on a single line. Default values may span multiple lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        }

        private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
        {
            var parameter = (ParameterSyntax)context.Node;
            var firstHeaderToken = GetFirstNonAttributeToken(parameter.AttributeLists, parameter.GetFirstToken());
            var identifierToken = parameter.Identifier;
            var headerEndToken = parameter.Default?.EqualsToken ?? identifierToken;

            var tree = parameter.SyntaxTree;
            var startLine = tree.GetLineSpan(firstHeaderToken.Span).StartLinePosition.Line;
            var identifierLine = tree.GetLineSpan(identifierToken.Span).StartLinePosition.Line;
            if (startLine != identifierLine)
            {
                ReportParameterDiagnostic(context, parameter, firstHeaderToken);
                return;
            }

            if (parameter.Default == null)
            {
                return;
            }

            var equalsLine = tree.GetLineSpan(headerEndToken.Span).StartLinePosition.Line;
            if (identifierLine != equalsLine)
            {
                ReportParameterDiagnostic(context, parameter, firstHeaderToken);
            }
        }

        private static void ReportParameterDiagnostic(
            SyntaxNodeAnalysisContext context,
            ParameterSyntax parameter,
            SyntaxToken firstHeaderToken)
        {
            var span = TextSpan.FromBounds(firstHeaderToken.SpanStart, parameter.Span.End);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(parameter.SyntaxTree, span)));
        }

        private static SyntaxToken GetFirstNonAttributeToken(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken defaultToken)
        {
            return attributeLists.Count > 0
                ? attributeLists[attributeLists.Count - 1].GetLastToken().GetNextToken()
                : defaultToken;
        }
    }
}
