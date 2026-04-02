using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0105Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0105";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0105.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space around ternary operators '?' and ':'",
            messageFormat: "{0} spaces around ternary operators '?' and ':'",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether spaces are required around ternary operators '?' and ':' in conditional expressions.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ConditionalExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var conditionalExpression = (ConditionalExpressionSyntax)context.Node;
            var questionToken = conditionalExpression.QuestionToken;
            var colonToken = conditionalExpression.ColonToken;
            var conditionLastToken = conditionalExpression.Condition.GetLastToken();
            var whenTrueFirstToken = conditionalExpression.WhenTrue.GetFirstToken();
            var whenTrueLastToken = conditionalExpression.WhenTrue.GetLastToken();
            var whenFalseFirstToken = conditionalExpression.WhenFalse.GetFirstToken();

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, conditionLastToken, questionToken, out var hasSpaceBeforeQuestion))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, questionToken, whenTrueFirstToken, out var hasSpaceAfterQuestion))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, whenTrueLastToken, colonToken, out var hasSpaceBeforeColon))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, colonToken, whenFalseFirstToken, out var hasSpaceAfterColon))
                return;

            var enabled = GetEnabled(context);
            if (hasSpaceBeforeQuestion != enabled || hasSpaceAfterQuestion != enabled)
                ReportDiagnostic(context, questionToken, enabled);

            if (hasSpaceBeforeColon != enabled || hasSpaceAfterColon != enabled)
                ReportDiagnostic(context, colonToken, enabled);
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            SyntaxToken operatorToken,
            bool enabled)
        {
            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                operatorToken.GetLocation(),
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
                return true;

            return bool.TryParse(value.Trim(), out var parsed) && parsed;
        }
    }
}
