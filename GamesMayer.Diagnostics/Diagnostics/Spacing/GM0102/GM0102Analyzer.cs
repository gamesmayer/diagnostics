using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0102Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0102";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0102.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between square brackets",
            messageFormat: "{0} spaces between square brackets",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether spaces are required inside non-empty square brackets. Equivalent to the csharp_space_between_square_brackets EditorConfig option.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.BracketedArgumentList,
                SyntaxKind.AttributeList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!TryGetBrackets(context.Node, out var openBracket, out var closeBracket))
                return;

            var firstToken = openBracket.GetNextToken();
            var lastToken = closeBracket.GetPreviousToken();

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    openBracket,
                    firstToken,
                    out var hasOpenSpace))
            {
                return;
            }

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    lastToken,
                    closeBracket,
                    out var hasCloseSpace))
            {
                return;
            }

            var enabled = GetEnabled(context);

            if (hasOpenSpace == enabled && hasCloseSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                openBracket.GetLocation(),
                properties,
                action));
        }

        private static bool TryGetBrackets(SyntaxNode node, out SyntaxToken openBracket, out SyntaxToken closeBracket)
        {
            switch (node)
            {
                case BracketedArgumentListSyntax bracketedList when bracketedList.Arguments.Count > 0:
                    openBracket = bracketedList.OpenBracketToken;
                    closeBracket = bracketedList.CloseBracketToken;
                    return true;
                case AttributeListSyntax attributeList when attributeList.Attributes.Count > 0:
                    openBracket = attributeList.OpenBracketToken;
                    closeBracket = attributeList.CloseBracketToken;
                    return true;
                default:
                    openBracket = default;
                    closeBracket = default;
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
