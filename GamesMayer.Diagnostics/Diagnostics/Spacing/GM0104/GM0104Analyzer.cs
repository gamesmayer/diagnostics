using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0104Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0104";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0104.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between generic operators '<' and '>'",
            messageFormat: "{0} spaces between generic operators '<' and '>'",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether spaces are required immediately inside generic angle brackets in type parameter and type argument lists.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.TypeParameterList,
                SyntaxKind.TypeArgumentList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!TryGetAngleBrackets(context.Node, out var lessThanToken, out var greaterThanToken))
                return;

            var firstToken = lessThanToken.GetNextToken();
            var lastToken = greaterThanToken.GetPreviousToken();

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, lessThanToken, firstToken, out var hasOpenSpace))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, lastToken, greaterThanToken, out var hasCloseSpace))
                return;

            var enabled = GetEnabled(context);
            if (hasOpenSpace == enabled && hasCloseSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                lessThanToken.GetLocation(),
                properties,
                action));
        }

        private static bool TryGetAngleBrackets(SyntaxNode node, out SyntaxToken lessThanToken, out SyntaxToken greaterThanToken)
        {
            switch (node)
            {
                case TypeParameterListSyntax typeParameterList when typeParameterList.Parameters.Count > 0:
                    lessThanToken = typeParameterList.LessThanToken;
                    greaterThanToken = typeParameterList.GreaterThanToken;
                    return true;
                case TypeArgumentListSyntax typeArgumentList when typeArgumentList.Arguments.Count > 0:
                    lessThanToken = typeArgumentList.LessThanToken;
                    greaterThanToken = typeArgumentList.GreaterThanToken;
                    return true;
                default:
                    lessThanToken = default;
                    greaterThanToken = default;
                    return false;
            }
        }

        private static bool TryHasSpaceBetweenTokens(
            SyntaxTree tree,
            SyntaxToken leftToken,
            SyntaxToken rightToken,
            out bool hasSpace)
        {
            var betweenSpan = TextSpan.FromBounds(leftToken.Span.End, rightToken.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            if (betweenText.IndexOf('\n') >= 0 || betweenText.IndexOf('\r') >= 0)
            {
                hasSpace = false;
                return false;
            }

            foreach (var ch in betweenText)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    hasSpace = false;
                    return false;
                }
            }

            hasSpace = betweenText.Length > 0;
            return true;
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return false;

            return bool.TryParse(value.Trim(), out var parsed) && parsed;
        }
    }
}
