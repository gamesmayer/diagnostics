using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0101Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0101";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0101.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between empty square brackets",
            messageFormat: "{0} the space between empty square brackets",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required between empty square brackets. Equivalent to the csharp_space_between_empty_square_brackets EditorConfig option.");

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
            var enabled = GetEnabled(context);
            var root = context.Tree.GetRoot(context.CancellationToken);

            foreach (var token in root.DescendantTokens())
            {
                if (!token.IsKind(SyntaxKind.OpenBracketToken))
                    continue;

                var closeToken = token.GetNextToken();
                if (!closeToken.IsKind(SyntaxKind.CloseBracketToken))
                    continue;

                var hasSpace = HasSpaceBetweenTokens(context.Tree, token, closeToken);

                if (hasSpace == enabled)
                    continue;

                var action = enabled ? "Add" : "Remove";
                var properties = ImmutableDictionary<string, string?>.Empty
                    .Add(EnabledProperty, enabled.ToString());

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptor,
                    token.GetLocation(),
                    properties,
                    action));
            }
        }

        private static bool HasSpaceBetweenTokens(SyntaxTree tree, SyntaxToken openToken, SyntaxToken closeToken)
        {
            var betweenSpan = TextSpan.FromBounds(openToken.Span.End, closeToken.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            foreach (var ch in betweenText)
            {
                if (!char.IsWhiteSpace(ch))
                    return false;
            }

            return betweenText.Length > 0;
        }

        private static bool GetEnabled(SyntaxTreeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return false;

            return bool.TryParse(value.Trim(), out var parsed) && parsed;
        }
    }
}
