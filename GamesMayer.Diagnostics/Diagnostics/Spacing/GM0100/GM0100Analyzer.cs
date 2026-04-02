using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0100Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0100";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0100.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space before semicolon in for statement",
            messageFormat: "{0} the space before semicolon in for statement",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required before each semicolon in a for statement. Equivalent to the csharp_space_before_semicolon_in_for_statement EditorConfig option.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
        }

        private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
        {
            var enabled = GetEnabled(context);
            var forStatement = (ForStatementSyntax)context.Node;

            CheckSemicolon(context, forStatement.FirstSemicolonToken, enabled);
            CheckSemicolon(context, forStatement.SecondSemicolonToken, enabled);
        }

        private static void CheckSemicolon(SyntaxNodeAnalysisContext context, SyntaxToken semicolon, bool enabled)
        {
            if (semicolon.IsKind(SyntaxKind.None))
                return;

            var prevToken = semicolon.GetPreviousToken();
            if (prevToken.IsKind(SyntaxKind.None))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, prevToken, semicolon, out var hasSpace))
                return;

            if (hasSpace == enabled)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                semicolon.GetLocation(),
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
