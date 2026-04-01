using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0085Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0085";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0085.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Spaces inside method call argument list parentheses",
            messageFormat: "{0} spaces inside method call argument list parentheses",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether spaces are required between method call argument list parentheses and arguments.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var argumentList = invocation.ArgumentList;

            if (argumentList.Arguments.Count == 0)
                return;

            var firstArgumentToken = argumentList.Arguments[0].GetFirstToken();
            var lastArgumentToken = argumentList.Arguments[argumentList.Arguments.Count - 1].GetLastToken();

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    argumentList.OpenParenToken,
                    firstArgumentToken,
                    out var hasOpenSpace))
            {
                return;
            }

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    lastArgumentToken,
                    argumentList.CloseParenToken,
                    out var hasCloseSpace))
            {
                return;
            }

            var enabled = GetEnabled(context);
            var expectedHasSpace = enabled;

            if (hasOpenSpace == expectedHasSpace && hasCloseSpace == expectedHasSpace)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                argumentList.OpenParenToken.GetLocation(),
                properties,
                action));
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
