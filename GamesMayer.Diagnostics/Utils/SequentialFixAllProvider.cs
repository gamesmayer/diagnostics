using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics.Utils
{
    /// <summary>
    /// Fixes a document's diagnostics one at a time, each on the document the previous fix produced, analyzing it
    /// again in between. The batch fixer computes every fix on the original document and merges their text changes,
    /// which mangles fixes on nested nodes (an initializer inside an initializer, a call inside an argument list).
    /// </summary>
    public sealed class SequentialFixAllProvider : FixAllProvider
    {
        private readonly CodeFixProvider _codeFixProvider;

        public SequentialFixAllProvider(CodeFixProvider codeFixProvider)
        {
            _codeFixProvider = codeFixProvider;
        }

        public override IEnumerable<FixAllScope> GetSupportedFixAllScopes() =>
            new[] { FixAllScope.Document, FixAllScope.Project, FixAllScope.Solution };

        public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
        {
            var entries = new List<(Document Document, ImmutableArray<Diagnostic> Diagnostics)>();

            foreach (var document in GetDocuments(fixAllContext))
            {
                var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);

                if (diagnostics.Length > 0)
                    entries.Add((document, diagnostics));
            }

            if (entries.Count == 0)
                return null;

            return CodeAction.Create(
                fixAllContext.CodeActionEquivalenceKey ?? "Fix all",
                async ct =>
                {
                    var solution = fixAllContext.Solution;

                    foreach (var (document, diagnostics) in entries)
                    {
                        var fixedDocument = await FixDocumentAsync(document, diagnostics, fixAllContext.CodeActionEquivalenceKey, ct).ConfigureAwait(false);
                        solution = solution.WithDocumentText(document.Id, await fixedDocument.GetTextAsync(ct).ConfigureAwait(false));
                    }

                    return solution;
                },
                fixAllContext.CodeActionEquivalenceKey);
        }

        private static IEnumerable<Document> GetDocuments(FixAllContext fixAllContext)
        {
            switch (fixAllContext.Scope)
            {
                case FixAllScope.Document:
                    return fixAllContext.Document != null ? new[] { fixAllContext.Document } : Enumerable.Empty<Document>();
                case FixAllScope.Project:
                    return fixAllContext.Project.Documents;
                case FixAllScope.Solution:
                    return fixAllContext.Solution.Projects.SelectMany(project => project.Documents);
                default:
                    return Enumerable.Empty<Document>();
            }
        }

        private async Task<Document> FixDocumentAsync(Document document, ImmutableArray<Diagnostic> diagnostics, string? equivalenceKey, CancellationToken cancellationToken)
        {
            var ids = diagnostics.Select(diagnostic => diagnostic.Id).ToImmutableHashSet();
            var analyzers = GetAnalyzers(ids);

            if (analyzers.IsEmpty)
                return document;

            // A fix can change more than its own node (reformatting a whole expression fixes its other operators
            // too), so the analyzer runs again on every fixed document and only what it still reports gets fixed.
            // Last first: a fix only moves code at or after its own position.
            var failed = new HashSet<(string Id, int Start, int Length)>();

            for (var iteration = 0; iteration < MaxFixesPerDocument; iteration++)
            {
                var remaining = (await AnalyzeAsync(document, analyzers, ids, cancellationToken).ConfigureAwait(false))
                    .Where(diagnostic => !failed.Contains((diagnostic.Id, diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)))
                    .OrderByDescending(diagnostic => diagnostic.Location.SourceSpan.Start)
                    .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Length)
                    .ToList();

                if (remaining.Count == 0)
                    break;

                var next = remaining[0];
                var fixedDocument = await ApplyFixAsync(document, next, equivalenceKey, cancellationToken).ConfigureAwait(false);
                var unchanged = fixedDocument == null ||
                    (await fixedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false)).ContentEquals(await document.GetTextAsync(cancellationToken).ConfigureAwait(false));

                if (unchanged)
                    failed.Add((next.Id, next.Location.SourceSpan.Start, next.Location.SourceSpan.Length));
                else
                    document = fixedDocument!;
            }

            return document;
        }

        private const int MaxFixesPerDocument = 2000;

        private static ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(ImmutableHashSet<string> ids)
        {
            return AllAnalyzers.Value
                .Where(analyzer => analyzer.SupportedDiagnostics.Any(descriptor => ids.Contains(descriptor.Id)))
                .ToImmutableArray();
        }

        private static readonly Lazy<ImmutableArray<DiagnosticAnalyzer>> AllAnalyzers = new Lazy<ImmutableArray<DiagnosticAnalyzer>>(() =>
            typeof(SequentialFixAllProvider).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type)!)
                .ToImmutableArray());

        private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(Document document, ImmutableArray<DiagnosticAnalyzer> analyzers, ImmutableHashSet<string> ids, CancellationToken cancellationToken)
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (compilation == null || tree == null)
                return ImmutableArray<Diagnostic>.Empty;

            var withAnalyzers = compilation.WithAnalyzers(analyzers, document.Project.AnalyzerOptions, cancellationToken);
            var syntax = await withAnalyzers.GetAnalyzerSyntaxDiagnosticsAsync(tree, cancellationToken).ConfigureAwait(false);
            var semantic = await withAnalyzers.GetAnalyzerSemanticDiagnosticsAsync(compilation.GetSemanticModel(tree), null, cancellationToken).ConfigureAwait(false);

            return syntax.Concat(semantic)
                .Where(diagnostic => ids.Contains(diagnostic.Id) && diagnostic.Location.IsInSource && diagnostic.Severity != DiagnosticSeverity.Hidden)
                .ToImmutableArray();
        }

        private async Task<Document?> ApplyFixAsync(Document document, Diagnostic diagnostic, string? equivalenceKey, CancellationToken cancellationToken)
        {
            CodeAction? action = null;
            var context = new CodeFixContext(
                document,
                diagnostic,
                (registered, _) =>
                {
                    if (action == null && (equivalenceKey == null || registered.EquivalenceKey == equivalenceKey))
                        action = registered;
                },
                cancellationToken);

            await _codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

            if (action == null)
                return null;

            var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
            var apply = operations.OfType<ApplyChangesOperation>().FirstOrDefault();

            return apply?.ChangedSolution.GetDocument(document.Id);
        }
    }
}
