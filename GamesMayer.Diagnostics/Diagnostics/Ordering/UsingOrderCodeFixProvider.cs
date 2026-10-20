using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using GamesMayer.Diagnostics.Utils;

namespace GamesMayer.Diagnostics
{
    /// <summary>
    /// Sorts the using directives of a file or namespace in one go, so every using rule is satisfied at once:
    /// global usings first, then regular usings with System first (GM1208) by namespace (GM1210), then using static
    /// (GM1216) with System first by name (GM1215), then aliases (GM1209) by alias name (GM1211). Each position keeps
    /// its leading trivia, so a file header stays on top. Lists with preprocessor directives are left as they are.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsingOrderCodeFixProvider))]
    [Shared]
    public sealed class UsingOrderCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(
                GM1208Analyzer.DiagnosticId,
                GM1209Analyzer.DiagnosticId,
                GM1210Analyzer.DiagnosticId,
                GM1211Analyzer.DiagnosticId,
                GM1215Analyzer.DiagnosticId,
                GM1216Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            new SequentialFixAllProvider(this);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var container = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<UsingDirectiveSyntax>()?.Parent;

            if (container == null || SortUsings(container) == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Sort using directives",
                    createChangedDocument: ct => FixAsync(context.Document, container, ct),
                    equivalenceKey: nameof(UsingOrderCodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> FixAsync(Document document, SyntaxNode container, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var sorted = SortUsings(container);

            if (root == null || sorted == null)
                return document;

            return document.WithSyntaxRoot(root.ReplaceNode(container, sorted));
        }

        private static SyntaxNode? SortUsings(SyntaxNode container)
        {
            switch (container)
            {
                case CompilationUnitSyntax unit:
                    return Sort(unit.Usings) is { } unitUsings ? unit.WithUsings(unitUsings) : null;
                case BaseNamespaceDeclarationSyntax ns:
                    return Sort(ns.Usings) is { } namespaceUsings ? ns.WithUsings(namespaceUsings) : null;
                default:
                    return null;
            }
        }

        private static SyntaxList<UsingDirectiveSyntax>? Sort(SyntaxList<UsingDirectiveSyntax> usings)
        {
            if (usings.Count < 2 || usings.Any(u => u.ContainsDirectives))
                return null;

            var sorted = usings
                .Select((usingDirective, index) => (Using: usingDirective, Index: index))
                .OrderBy(entry => entry.Using.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) ? 0 : 1)
                .ThenBy(entry => Group(entry.Using))
                .ThenBy(entry => IsSystem(entry.Using) ? 0 : 1)
                .ThenBy(entry => SortName(entry.Using), StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.Index)
                .Select(entry => entry.Using)
                .ToList();

            if (sorted.SequenceEqual(usings))
                return null;

            // Leading trivia belongs to the position (a file header stays above the first using).
            return SyntaxFactory.List(sorted.Select((usingDirective, i) => usingDirective.WithLeadingTrivia(usings[i].GetLeadingTrivia())));
        }

        private static int Group(UsingDirectiveSyntax usingDirective)
        {
            if (usingDirective.Alias != null)
                return 2;

            return usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) ? 1 : 0;
        }

        private static bool IsSystem(UsingDirectiveSyntax usingDirective) =>
            usingDirective.Alias == null &&
            OrderingHelpers.IsSystemUsingDirective(usingDirective) &&
            !OrderingHelpers.HasNamespaceAliasQualifier(usingDirective);

        private static string SortName(UsingDirectiveSyntax usingDirective) =>
            usingDirective.Alias != null
                ? usingDirective.Alias.Name.Identifier.ValueText.ToLowerInvariant()
                : OrderingHelpers.GetNormalizedNamespace(usingDirective.Name!);
    }
}
