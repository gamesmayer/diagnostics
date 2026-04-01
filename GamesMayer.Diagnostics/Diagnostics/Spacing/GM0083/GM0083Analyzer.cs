using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0083Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0083";
        public const string EnabledOptionKey = "dotnet_diagnostic.GM0083.enabled";
        public const string EnabledProperty = "enabled";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Space after control-flow keywords",
            messageFormat: "{0} the space after control-flow keyword '{1}'",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Controls whether a space is required after control-flow keywords before the opening parenthesis.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.IfStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.ForEachStatement,
                SyntaxKind.ForEachVariableStatement,
                SyntaxKind.WhileStatement,
                SyntaxKind.DoStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.LockStatement,
                SyntaxKind.UsingStatement,
                SyntaxKind.CatchClause);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!TryGetKeywordAndOpenParen(context.Node, out var keywordToken, out var openParenToken))
                return;

            if (!TryHasSpaceBetweenTokens(context.Node.SyntaxTree, keywordToken, openParenToken, out var hasSpace))
                return;

            var enabled = GetEnabled(context);
            if (enabled == hasSpace)
                return;

            var action = enabled ? "Add" : "Remove";
            var properties = ImmutableDictionary<string, string?>.Empty
                .Add(EnabledProperty, enabled.ToString());

            context.ReportDiagnostic(Diagnostic.Create(
                Descriptor,
                keywordToken.GetLocation(),
                properties,
                action,
                keywordToken.Text));
        }

        private static bool TryGetKeywordAndOpenParen(
            SyntaxNode node,
            out SyntaxToken keywordToken,
            out SyntaxToken openParenToken)
        {
            switch (node)
            {
                case Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax ifStatement:
                    keywordToken = ifStatement.IfKeyword;
                    openParenToken = ifStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax forStatement:
                    keywordToken = forStatement.ForKeyword;
                    openParenToken = forStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax forEachStatement:
                    keywordToken = forEachStatement.ForEachKeyword;
                    openParenToken = forEachStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.ForEachVariableStatementSyntax forEachVariableStatement:
                    keywordToken = forEachVariableStatement.ForEachKeyword;
                    openParenToken = forEachVariableStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax whileStatement:
                    keywordToken = whileStatement.WhileKeyword;
                    openParenToken = whileStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax doStatement:
                    keywordToken = doStatement.WhileKeyword;
                    openParenToken = doStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax switchStatement:
                    keywordToken = switchStatement.SwitchKeyword;
                    openParenToken = switchStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.LockStatementSyntax lockStatement:
                    keywordToken = lockStatement.LockKeyword;
                    openParenToken = lockStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax usingStatement when !usingStatement.OpenParenToken.IsKind(SyntaxKind.None):
                    keywordToken = usingStatement.UsingKeyword;
                    openParenToken = usingStatement.OpenParenToken;
                    return true;
                case Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax catchClause when catchClause.Declaration != null:
                    keywordToken = catchClause.CatchKeyword;
                    openParenToken = catchClause.Declaration.OpenParenToken;
                    return true;
                default:
                    keywordToken = default;
                    openParenToken = default;
                    return false;
            }
        }

        private static bool TryHasSpaceBetweenTokens(
            SyntaxTree tree,
            SyntaxToken keywordToken,
            SyntaxToken openParenToken,
            out bool hasSpace)
        {
            var betweenSpan = TextSpan.FromBounds(keywordToken.Span.End, openParenToken.SpanStart);
            var betweenText = tree.GetText().ToString(betweenSpan);

            // Keep the rule focused on same-line whitespace only.
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
