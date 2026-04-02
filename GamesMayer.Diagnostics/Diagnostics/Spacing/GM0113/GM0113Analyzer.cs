using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0113Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0113";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0113.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space between single-line block braces",
            messageFormat: "{0} spaces after '{' and before '}' in single-line blocks",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether spaces are required inside non-empty single-line blocks and accessor lists.");

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
            if (!TryGetBraces(context.Node, out var openBrace, out var closeBrace))
                return;

            var lineSpan = context.Node.SyntaxTree.GetLineSpan(TextSpan.FromBounds(openBrace.SpanStart, closeBrace.Span.End));
            if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
                return;

            var firstToken = openBrace.GetNextToken();
            var lastToken = closeBrace.GetPreviousToken();

            if (firstToken == closeBrace || firstToken.SpanStart >= closeBrace.SpanStart)
                return;

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    openBrace,
                    firstToken,
                    out var hasOpenSpace))
            {
                return;
            }

            if (!TryHasSpaceBetweenTokens(
                    context.Node.SyntaxTree,
                    lastToken,
                    closeBrace,
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
                openBrace.GetLocation(),
                properties,
                action));
        }

        private static bool TryGetBraces(SyntaxNode node, out SyntaxToken openBrace, out SyntaxToken closeBrace)
        {
            switch (node)
            {
                case BlockSyntax block when block.Statements.Count > 0:
                    openBrace = block.OpenBraceToken;
                    closeBrace = block.CloseBraceToken;
                    return true;
                case AccessorListSyntax accessorList when accessorList.Accessors.Count > 0:
                    openBrace = accessorList.OpenBraceToken;
                    closeBrace = accessorList.CloseBraceToken;
                    return true;
                default:
                    openBrace = default;
                    closeBrace = default;
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
                return true;

            return bool.TryParse(value.Trim(), out var parsed) ? parsed : true;
        }
    }
}