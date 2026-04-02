using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0110Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0110";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0110.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space after assignment operator '='",
            messageFormat: "{0} the space after assignment operator '='",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required after assignment operator '='.");

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
                if (!IsAssignmentEqualsToken(token))
                    continue;

                var nextToken = token.GetNextToken();
                if (nextToken.IsKind(SyntaxKind.None))
                    continue;

                if (!TryHasSpaceBetweenTokens(context.Tree, token, nextToken, out var hasSpace))
                    continue;

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

        private static bool IsAssignmentEqualsToken(SyntaxToken token)
        {
            if (!token.IsKind(SyntaxKind.EqualsToken))
                return false;

            return token.Parent is EqualsValueClauseSyntax
                || (token.Parent is AssignmentExpressionSyntax assignment
                    && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression));
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

        private static bool GetEnabled(SyntaxTreeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return true;

            return bool.TryParse(value.Trim(), out var parsed) ? parsed : true;
        }
    }
}
