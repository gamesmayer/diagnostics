using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0040CodeFixProvider))]
    [Shared]
    public sealed class GM0040CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0040Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            GM0040FixAllProvider.Instance;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Move segment to its own line",
                    createChangedDocument: ct => FixDocumentAsync(context.Document, ImmutableArray.Create(diagnostic), ct),
                    equivalenceKey: nameof(GM0040CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        internal static async Task<Document> FixDocumentAsync(
            Document document,
            ImmutableArray<Diagnostic> diagnostics,
            CancellationToken cancellationToken)
        {
            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var tree = root.SyntaxTree;
            var processedChainStarts = new HashSet<int>();
            var allChanges = new List<TextChange>();

            foreach (var diagnostic in diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                if (diagnosticSpan.Start >= sourceText.Length || sourceText[diagnosticSpan.Start] != '.')
                    continue;

                var chainTop = FindChainTop(root, diagnosticSpan.Start);
                if (chainTop == null)
                    continue;

                if (!processedChainStarts.Add(chainTop.SpanStart))
                    continue;

                var changes = ComputeChainFixes(sourceText, tree, chainTop);
                allChanges.AddRange(changes);
            }

            if (allChanges.Count == 0)
                return document;

            allChanges.Sort((a, b) => a.Span.Start.CompareTo(b.Span.Start));
            return document.WithText(sourceText.WithChanges(allChanges));
        }

        private static List<TextChange> ComputeChainFixes(
            SourceText sourceText,
            SyntaxTree tree,
            ExpressionSyntax chainTop)
        {
            var boundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            CollectFluentChainBoundaries(chainTop, boundaries);

            var invocationBoundaries = new List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)>();
            foreach (var b in boundaries)
            {
                if (b.SegmentExpression is InvocationExpressionSyntax)
                    invocationBoundaries.Add(b);
            }

            if (invocationBoundaries.Count == 0)
                return new List<TextChange>();

            string correctIndent = ComputeCorrectIndent(sourceText, invocationBoundaries[0].LeftExpression);
            string newlineStr = DetectNewline(sourceText);

            var changes = new List<TextChange>();

            for (int i = 0; i < invocationBoundaries.Count; i++)
            {
                var (leftExpression, dotToken, _, _) = invocationBoundaries[i];

                SyntaxToken referenceToken = i == 0
                    ? leftExpression.GetLastToken()
                    : invocationBoundaries[i - 1].SegmentExpression.GetLastToken();

                var referenceLine = tree.GetLineSpan(referenceToken.Span).EndLinePosition.Line;
                var dotLineNum = tree.GetLineSpan(dotToken.Span).StartLinePosition.Line;
                bool isOnOwnLine = dotLineNum > referenceLine;

                int dotPos = dotToken.SpanStart;

                if (!isOnOwnLine)
                {
                    int removeStart = dotPos;
                    while (removeStart > 0 && (sourceText[removeStart - 1] == ' ' || sourceText[removeStart - 1] == '\t'))
                        removeStart--;
                    changes.Add(new TextChange(new TextSpan(removeStart, dotPos - removeStart), newlineStr + correctIndent));
                }
                else
                {
                    int lineStart = dotPos;
                    while (lineStart > 0 && sourceText[lineStart - 1] != '\n' && sourceText[lineStart - 1] != '\r')
                        lineStart--;

                    int currentIndentLength = dotPos - lineStart;
                    string currentIndent = sourceText.GetSubText(new TextSpan(lineStart, currentIndentLength)).ToString();
                    if (currentIndent != correctIndent)
                        changes.Add(new TextChange(new TextSpan(lineStart, currentIndentLength), correctIndent));
                }
            }

            return changes;
        }

        private static ExpressionSyntax? FindChainTop(SyntaxNode root, int dotPos)
        {
            var token = root.FindToken(dotPos);
            var current = token.Parent as ExpressionSyntax;
            while (current != null)
            {
                var parent = current.Parent;
                if (parent is InvocationExpressionSyntax inv && inv.Expression == current)
                    current = inv;
                else if (parent is MemberAccessExpressionSyntax ma && ma.Expression == current)
                    current = ma;
                else
                    break;
            }
            return current;
        }

        private static void CollectFluentChainBoundaries(
            ExpressionSyntax expression,
            List<(ExpressionSyntax LeftExpression, SyntaxToken DotToken, SyntaxToken NextToken, ExpressionSyntax SegmentExpression)> boundaries)
        {
            if (expression is InvocationExpressionSyntax invocation
                && invocation.Expression is MemberAccessExpressionSyntax invMemberAccess)
            {
                CollectFluentChainBoundaries(invMemberAccess.Expression, boundaries);
                boundaries.Add((invMemberAccess.Expression, invMemberAccess.OperatorToken, invMemberAccess.Name.GetFirstToken(), invocation));
                return;
            }

            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                CollectFluentChainBoundaries(memberAccess.Expression, boundaries);
                boundaries.Add((memberAccess.Expression, memberAccess.OperatorToken, memberAccess.Name.GetFirstToken(), memberAccess));
            }
        }

        private static string ComputeCorrectIndent(SourceText sourceText, ExpressionSyntax rootExpression)
        {
            var rootFirstToken = rootExpression.GetFirstToken();
            var rootLine = sourceText.Lines.GetLineFromPosition(rootFirstToken.SpanStart);
            var rootLineText = rootLine.ToString();

            int baseEnd = 0;
            while (baseEnd < rootLineText.Length && (rootLineText[baseEnd] == ' ' || rootLineText[baseEnd] == '\t'))
                baseEnd++;

            string baseIndent = rootLineText.Substring(0, baseEnd);
            string extraIndent = baseIndent.IndexOf('\t') >= 0 ? "\t" : "    ";
            return baseIndent + extraIndent;
        }

        private static string DetectNewline(SourceText sourceText)
        {
            for (int i = 0; i < sourceText.Length - 1; i++)
            {
                if (sourceText[i] == '\r' && sourceText[i + 1] == '\n') return "\r\n";
                if (sourceText[i] == '\n') return "\n";
            }
            return "\n";
        }

        private sealed class GM0040FixAllProvider : FixAllProvider
        {
            public static readonly GM0040FixAllProvider Instance = new GM0040FixAllProvider();

            public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
            {
                var entries = new List<(Document Document, ImmutableArray<Diagnostic> Diagnostics)>();

                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        if (fixAllContext.Document != null)
                        {
                            var diags = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document).ConfigureAwait(false);
                            if (diags.Length > 0)
                                entries.Add((fixAllContext.Document, diags));
                        }
                        break;
                    case FixAllScope.Project:
                        foreach (var doc in fixAllContext.Project.Documents)
                        {
                            var diags = await fixAllContext.GetDocumentDiagnosticsAsync(doc).ConfigureAwait(false);
                            if (diags.Length > 0)
                                entries.Add((doc, diags));
                        }
                        break;
                    case FixAllScope.Solution:
                        foreach (var project in fixAllContext.Solution.Projects)
                        foreach (var doc in project.Documents)
                        {
                            var diags = await fixAllContext.GetDocumentDiagnosticsAsync(doc).ConfigureAwait(false);
                            if (diags.Length > 0)
                                entries.Add((doc, diags));
                        }
                        break;
                }

                if (entries.Count == 0)
                    return null;

                return CodeAction.Create(
                    "Move segment to its own line",
                    async ct =>
                    {
                        var solution = fixAllContext.Solution;
                        foreach (var (document, diagnostics) in entries)
                        {
                            var newDoc = await FixDocumentAsync(document, diagnostics, ct).ConfigureAwait(false);
                            solution = solution.WithDocumentText(document.Id, await newDoc.GetTextAsync(ct).ConfigureAwait(false));
                        }
                        return solution;
                    },
                    nameof(GM0040FixAllProvider));
            }
        }
    }
}
