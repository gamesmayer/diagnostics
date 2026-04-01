using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0082Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0082";
        public const string OptionKey = "dotnet_diagnostic.GM0082.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Cast expression spacing",
            messageFormat: "{0} the space after the cast expression",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required after a cast expression. When set to true, a space is required (e.g., (int) value). When set to false, no space is allowed (e.g., (int)value).");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CastExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var castExpr = (CastExpressionSyntax)context.Node;
            var enabled = GetEnabled(context);
            var expressionFirstToken = castExpr.Expression.GetFirstToken();

            if (!AreOnSameLine(castExpr, expressionFirstToken))
                return;

            var hasSpace = HasSpaceBetweenCloseParenAndExpression(castExpr, expressionFirstToken);

            if (enabled == hasSpace)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                castExpr.CloseParenToken.GetLocation(),
                properties,
                action));
        }

        private static bool AreOnSameLine(CastExpressionSyntax castExpr, SyntaxToken expressionFirstToken)
        {
            var tree = castExpr.SyntaxTree;
            var closeParenLine = tree.GetLineSpan(castExpr.CloseParenToken.Span).EndLinePosition.Line;
            var expressionLine = tree.GetLineSpan(expressionFirstToken.Span).StartLinePosition.Line;
            return closeParenLine == expressionLine;
        }

        internal static bool HasSpaceBetweenCloseParenAndExpression(
            CastExpressionSyntax castExpr,
            SyntaxToken expressionFirstToken)
        {
            foreach (var trivia in castExpr.CloseParenToken.TrailingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    return true;
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    return false;
            }

            foreach (var trivia in expressionFirstToken.LeadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    return true;
                if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
                    return false;
            }

            return false;
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(OptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return false;

            return value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
