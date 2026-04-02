using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0089CodeFixProvider))]
    [Shared]
    public sealed class GM0089CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0089Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var enabled = diagnostic.Properties.TryGetValue(GM0089Analyzer.EnabledProperty, out var value)
                && bool.TryParse(value, out var parsed)
                && parsed;

            var title = enabled
                ? "Add spaces between parentheses"
                : "Remove spaces between parentheses";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => FixSpacingAsync(context.Document, diagnostic, enabled, ct),
                    equivalenceKey: nameof(GM0089CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        private static async Task<Document> FixSpacingAsync(
            Document document,
            Diagnostic diagnostic,
            bool enabled,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var openParen = root.FindToken(diagnostic.Location.SourceSpan.Start);
            if (!TryGetCloseParen(openParen, out var closeParen))
                return document;

            var firstToken = openParen.GetNextToken();
            var lastToken = closeParen.GetPreviousToken();

            if (firstToken == closeParen || firstToken.SpanStart >= closeParen.SpanStart)
                return document;

            var openSpan = TextSpan.FromBounds(openParen.Span.End, firstToken.SpanStart);
            var closeSpan = TextSpan.FromBounds(lastToken.Span.End, closeParen.SpanStart);
            var replacement = enabled ? " " : string.Empty;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var changes = new List<TextChange>();

            if (text.ToString(openSpan) != replacement)
                changes.Add(new TextChange(openSpan, replacement));

            if (text.ToString(closeSpan) != replacement)
                changes.Add(new TextChange(closeSpan, replacement));

            if (changes.Count == 0)
                return document;

            var newText = text.WithChanges(changes);
            return document.WithText(newText);
        }

        private static bool TryGetCloseParen(SyntaxToken openParen, out SyntaxToken closeParen)
        {
            var parent = openParen.Parent;
            switch (parent)
            {
                case IfStatementSyntax ifStatement:
                    closeParen = ifStatement.CloseParenToken;
                    return true;
                case ForStatementSyntax forStatement:
                    closeParen = forStatement.CloseParenToken;
                    return true;
                case ForEachStatementSyntax forEachStatement:
                    closeParen = forEachStatement.CloseParenToken;
                    return true;
                case WhileStatementSyntax whileStatement:
                    closeParen = whileStatement.CloseParenToken;
                    return true;
                case DoStatementSyntax doStatement:
                    closeParen = doStatement.CloseParenToken;
                    return true;
                case SwitchStatementSyntax switchStatement:
                    closeParen = switchStatement.CloseParenToken;
                    return true;
                case LockStatementSyntax lockStatement:
                    closeParen = lockStatement.CloseParenToken;
                    return true;
                case UsingStatementSyntax usingStatement:
                    closeParen = usingStatement.CloseParenToken;
                    return true;
                case FixedStatementSyntax fixedStatement:
                    closeParen = fixedStatement.CloseParenToken;
                    return true;
                case ParenthesizedExpressionSyntax parenthesizedExpression:
                    closeParen = parenthesizedExpression.CloseParenToken;
                    return true;
                case CastExpressionSyntax castExpression:
                    closeParen = castExpression.CloseParenToken;
                    return true;
                case TupleTypeSyntax tupleType:
                    closeParen = tupleType.CloseParenToken;
                    return true;
                case TupleExpressionSyntax tupleExpression:
                    closeParen = tupleExpression.CloseParenToken;
                    return true;
                default:
                    closeParen = default;
                    return false;
            }
        }
    }
}
