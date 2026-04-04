using System;
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
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0020CodeFixProvider))]
    [Shared]
    public sealed class GM0020CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0020Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var braceStyle = GetBraceStyle(diagnostic);
            var braceKind = GetBraceKind(diagnostic);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: GetCodeFixTitle(braceStyle, braceKind),
                    createChangedDocument: ct => FixBraceAsync(context.Document, diagnostic.Location.SourceSpan.Start, braceStyle, braceKind, ct),
                    equivalenceKey: $"{nameof(GM0020CodeFixProvider)}_{braceStyle}_{braceKind}"),
                diagnostic);

            return Task.CompletedTask;
        }

        private static BraceStyle GetBraceStyle(Diagnostic diagnostic)
        {
            if (diagnostic.Properties.TryGetValue(GM0020Analyzer.StylePropertyKey, out var styleValue))
            {
                return AnalyzerConfigCategoryParser.ParseBraceStyle(styleValue);
            }

            return BraceStyle.Allman;
        }

        private static string GetBraceKind(Diagnostic diagnostic)
        {
            if (diagnostic.Properties.TryGetValue(GM0020Analyzer.BraceKindPropertyKey, out var braceKind)
                && !string.IsNullOrWhiteSpace(braceKind))
            {
                return braceKind!;
            }

            return "opening";
        }

        private static string GetCodeFixTitle(BraceStyle braceStyle, string braceKind)
        {
            if (braceKind == "closing")
            {
                return "Move closing brace to a new line";
            }

            return braceStyle == BraceStyle.Allman
                ? "Move opening brace to a new line"
                : "Move opening brace to the declaration line";
        }

        private static Task<Document> FixBraceAsync(
            Document document,
            int bracePosition,
            BraceStyle braceStyle,
            string braceKind,
            CancellationToken cancellationToken)
        {
            if (braceKind == "closing")
            {
                return MoveClosingBraceToNewLineAsync(document, bracePosition, cancellationToken);
            }

            return braceStyle == BraceStyle.Allman
                ? MoveOpeningBraceToNewLineAsync(document, bracePosition, cancellationToken)
                : MoveOpeningBraceToDeclarationLineAsync(document, bracePosition, cancellationToken);
        }

        private static async Task<Document> MoveOpeningBraceToNewLineAsync(
            Document document,
            int bracePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var openBrace = root.FindToken(bracePosition);
            if (!openBrace.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OpenBraceToken))
            {
                return document;
            }

            var previousToken = openBrace.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenTokensSpan = TextSpan.FromBounds(previousToken.Span.End, openBrace.Span.Start);
            var betweenTokensText = sourceText.ToString(betweenTokensSpan);
            var preservedBetweenTokensText = betweenTokensText.TrimEnd(' ', '\t');
            var indent = GetLineIndentation(sourceText, previousToken.SpanStart);

            var replacement = string.Concat(preservedBetweenTokensText, Environment.NewLine, indent);
            var updatedText = sourceText.WithChanges(new TextChange(betweenTokensSpan, replacement));

            return document.WithText(updatedText);
        }

        private static async Task<Document> MoveOpeningBraceToDeclarationLineAsync(
            Document document,
            int bracePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var openBrace = root.FindToken(bracePosition);
            if (!openBrace.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OpenBraceToken))
            {
                return document;
            }

            var previousToken = openBrace.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenTokensSpan = TextSpan.FromBounds(previousToken.Span.End, openBrace.Span.Start);
            var betweenTokensText = sourceText.ToString(betweenTokensSpan);
            var replacement = GetDeclarationLineReplacement(betweenTokensText);
            if (replacement == betweenTokensText)
            {
                return document;
            }

            var updatedText = sourceText.WithChanges(new TextChange(betweenTokensSpan, replacement));

            return document.WithText(updatedText);
        }

        private static async Task<Document> MoveClosingBraceToNewLineAsync(
            Document document,
            int bracePosition,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var closeBrace = root.FindToken(bracePosition);
            if (!closeBrace.IsKind(SyntaxKind.CloseBraceToken))
            {
                return document;
            }

            if (!TryGetBracePair(closeBrace, out var openBrace, out _))
            {
                return document;
            }

            var previousToken = closeBrace.GetPreviousToken(includeZeroWidth: true);
            if (previousToken == default)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var betweenTokensSpan = TextSpan.FromBounds(previousToken.Span.End, closeBrace.SpanStart);
            var betweenTokensText = sourceText.ToString(betweenTokensSpan);
            var preservedBetweenTokensText = betweenTokensText.TrimEnd(' ', '\t');
            var indent = GetLineIndentation(sourceText, openBrace.SpanStart);
            var replacement = string.Concat(preservedBetweenTokensText, Environment.NewLine, indent);
            var updatedText = sourceText.WithChanges(new TextChange(betweenTokensSpan, replacement));

            return document.WithText(updatedText);
        }

        private static string GetDeclarationLineReplacement(string betweenTokensText)
        {
            var preservedBetweenTokensText = betweenTokensText.Trim();
            if (preservedBetweenTokensText.Length == 0)
            {
                return " ";
            }

            if (preservedBetweenTokensText.IndexOf("//", StringComparison.Ordinal) >= 0)
            {
                return betweenTokensText;
            }

            return string.Concat(" ", preservedBetweenTokensText, " ");
        }

        private static bool TryGetBracePair(SyntaxToken braceToken, out SyntaxToken openBrace, out SyntaxToken closeBrace)
        {
            switch (braceToken.Parent)
            {
                case BlockSyntax block:
                    openBrace = block.OpenBraceToken;
                    closeBrace = block.CloseBraceToken;
                    return true;
                case AccessorListSyntax accessorList:
                    openBrace = accessorList.OpenBraceToken;
                    closeBrace = accessorList.CloseBraceToken;
                    return true;
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    openBrace = namespaceDeclaration.OpenBraceToken;
                    closeBrace = namespaceDeclaration.CloseBraceToken;
                    return true;
                case TypeDeclarationSyntax typeDeclaration:
                    openBrace = typeDeclaration.OpenBraceToken;
                    closeBrace = typeDeclaration.CloseBraceToken;
                    return true;
                case EnumDeclarationSyntax enumDeclaration:
                    openBrace = enumDeclaration.OpenBraceToken;
                    closeBrace = enumDeclaration.CloseBraceToken;
                    return true;
                case SwitchStatementSyntax switchStatement:
                    openBrace = switchStatement.OpenBraceToken;
                    closeBrace = switchStatement.CloseBraceToken;
                    return true;
                case InitializerExpressionSyntax initializerExpression:
                    openBrace = initializerExpression.OpenBraceToken;
                    closeBrace = initializerExpression.CloseBraceToken;
                    return true;
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreation:
                    openBrace = anonymousObjectCreation.OpenBraceToken;
                    closeBrace = anonymousObjectCreation.CloseBraceToken;
                    return true;
                default:
                    openBrace = default;
                    closeBrace = default;
                    return false;
            }
        }

        private static string GetLineIndentation(SourceText text, int position)
        {
            var line = text.Lines.GetLineFromPosition(position);
            var lineText = text.ToString(line.Span);
            var index = 0;

            while (index < lineText.Length && (lineText[index] == ' ' || lineText[index] == '\t'))
            {
                index++;
            }

            return lineText.Substring(0, index);
        }
    }
}
