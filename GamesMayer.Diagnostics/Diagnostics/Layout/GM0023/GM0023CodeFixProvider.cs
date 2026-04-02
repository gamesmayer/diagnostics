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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0023CodeFixProvider))]
    [Shared]
    public sealed class GM0023CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0023Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            GM0023FixAllProvider.Instance;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Fix indentation",
                    createChangedDocument: ct => FixDocumentAsync(context.Document, ImmutableArray.Create(diagnostic), ct),
                    equivalenceKey: nameof(GM0023CodeFixProvider)),
                diagnostic);

            return Task.CompletedTask;
        }

        internal static async Task<Document> FixDocumentAsync(
            Document document,
            ImmutableArray<Diagnostic> diagnostics,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var syntaxTree = root.SyntaxTree;

            var analyzerOptions = document.Project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
            int indentSize = 4;
            if (analyzerOptions.TryGetValue("indent_size", out var indentSizeStr)
                && int.TryParse(indentSizeStr, out var parsedSize)
                && parsedSize > 0)
            {
                indentSize = parsedSize;
            }

            var changes = new List<TextChange>();

            foreach (var diagnostic in diagnostics)
            {
                var tokenPosition = diagnostic.Location.SourceSpan.Start;
                var token = root.FindToken(tokenPosition);

                var node = token.Parent;
                SyntaxNode? itemNode = null;
                SyntaxToken openParen = default;

                while (node != null)
                {
                    if (node is ArgumentSyntax argument && argument.Parent is ArgumentListSyntax argList)
                    {
                        itemNode = argument;
                        openParen = argList.OpenParenToken;
                        break;
                    }

                    if (node is ParameterSyntax parameter && parameter.Parent is ParameterListSyntax paramList)
                    {
                        itemNode = parameter;
                        openParen = paramList.OpenParenToken;
                        break;
                    }

                    node = node.Parent;
                }

                if (itemNode == null || openParen == default)
                    continue;

                var openParenLineNumber = syntaxTree.GetLineSpan(openParen.Span).EndLinePosition.Line;
                var expectedIndentation = GM0023Analyzer.GetExpectedIndentation(sourceText, openParenLineNumber, indentSize);

                var itemFirstToken = itemNode.GetFirstToken();
                if (itemFirstToken == default)
                    continue;

                var tokenLineNumber = syntaxTree.GetLineSpan(itemFirstToken.Span).StartLinePosition.Line;
                var tokenLine = sourceText.Lines[tokenLineNumber];
                var lineStr = tokenLine.ToString();

                int actualLen = 0;
                while (actualLen < lineStr.Length && (lineStr[actualLen] == ' ' || lineStr[actualLen] == '\t'))
                    actualLen++;

                changes.Add(new TextChange(new TextSpan(tokenLine.Start, actualLen), expectedIndentation));
            }

            if (changes.Count == 0)
                return document;

            changes.Sort((a, b) => a.Span.Start.CompareTo(b.Span.Start));
            return document.WithText(sourceText.WithChanges(changes));
        }

        private sealed class GM0023FixAllProvider : FixAllProvider
        {
            public static readonly GM0023FixAllProvider Instance = new GM0023FixAllProvider();

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
                    "Fix indentation",
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
                    nameof(GM0023FixAllProvider));
            }
        }
    }
}
