using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0093Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0093";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0093.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between method call name and opening parenthesis",
            messageFormat: "{0} the space between method call name and opening parenthesis",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required between the method call name and the opening parenthesis. Equivalent to the csharp_space_between_method_call_name_and_opening_parenthesis EditorConfig option.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!TryGetNameTokenAndOpenParen(context.Node, out var nameToken, out var openParen))
                return;

            var hasSpace = HasSpaceBetweenTokens(context.Node.SyntaxTree, nameToken, openParen);
            var enabled = GetEnabled(context);

            if (hasSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                openParen.GetLocation(),
                properties,
                action));
        }

        private static bool TryGetNameTokenAndOpenParen(SyntaxNode node, out SyntaxToken nameToken, out SyntaxToken openParen)
        {
            switch (node)
            {
                case InvocationExpressionSyntax invocation:
                    nameToken = invocation.Expression.GetLastToken();
                    openParen = invocation.ArgumentList.OpenParenToken;
                    return true;
                case ObjectCreationExpressionSyntax objectCreation when objectCreation.ArgumentList != null:
                    nameToken = objectCreation.Type.GetLastToken();
                    openParen = objectCreation.ArgumentList.OpenParenToken;
                    return true;
                default:
                    nameToken = default;
                    openParen = default;
                    return false;
            }
        }

        private static bool HasSpaceBetweenTokens(SyntaxTree tree, SyntaxToken leftToken, SyntaxToken rightToken)
        {
            var betweenSpan = TextSpan.FromBounds(leftToken.Span.End, rightToken.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            if (betweenText.IndexOf('\n') >= 0 || betweenText.IndexOf('\r') >= 0)
                return false;

            foreach (var ch in betweenText)
            {
                if (!char.IsWhiteSpace(ch))
                    return false;
            }

            return betweenText.Length > 0;
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
