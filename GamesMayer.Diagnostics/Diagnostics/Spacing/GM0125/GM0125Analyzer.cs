using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0125Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0125";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0125.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space before open brace",
            messageFormat: "{0} the space before '{{' on the same line as its declaration",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required before an opening brace '{' when it appears on the same line as its declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.Block,
                SyntaxKind.AccessorList);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var openBrace = GetOpenBrace(context.Node);
            if (openBrace == default)
                return;

            var prevToken = openBrace.GetPreviousToken();
            if (prevToken == default)
                return;

            var tree = context.Node.SyntaxTree;
            var prevLine = tree.GetLineSpan(prevToken.Span).EndLinePosition.Line;
            var openBraceLine = tree.GetLineSpan(openBrace.Span).StartLinePosition.Line;
            if (prevLine != openBraceLine)
                return;

            var sourceText = tree.GetText();
            var betweenSpan = TextSpan.FromBounds(prevToken.Span.End, openBrace.SpanStart);
            var betweenText = sourceText.ToString(betweenSpan);
            var hasSpace = betweenText.Length > 0;

            var enabled = GetEnabled(context);
            if (hasSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                openBrace.GetLocation(),
                properties,
                action));
        }

        private static SyntaxToken GetOpenBrace(SyntaxNode node)
        {
            switch (node)
            {
                case BlockSyntax block:
                    return block.OpenBraceToken;
                case AccessorListSyntax accessorList:
                    return accessorList.OpenBraceToken;
                default:
                    return default;
            }
        }

        private static bool GetEnabled(SyntaxNodeAnalysisContext context)
        {
            var fileOptions = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Node.SyntaxTree);
            if (!fileOptions.TryGetValue(EnabledOptionKey, out var value) || string.IsNullOrWhiteSpace(value))
                return true;

            return bool.TryParse(value.Trim(), out var parsed) ? parsed : true;
        }
    }
}
