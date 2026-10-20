using System.Collections.Generic;
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
    /// Reorders the members of a type, namespace or file in one go, so every ordering rule is satisfied at once:
    /// kind (GM1201), then access (GM1202), then constants first (GM1203), static first (GM1204) and readonly first
    /// (GM1214). The sort is stable and each member keeps its own comments. Lists with preprocessor directives or
    /// members the rules don't order are left as they are.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ElementOrderCodeFixProvider))]
    [Shared]
    public sealed class ElementOrderCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(
                GM1201Analyzer.DiagnosticId,
                GM1202Analyzer.DiagnosticId,
                GM1203Analyzer.DiagnosticId,
                GM1204Analyzer.DiagnosticId,
                GM1214Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            new SequentialFixAllProvider(this);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var member = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            var container = member?.Parent;

            if (container == null || ReorderMembers(container) == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Reorder members",
                    createChangedDocument: ct => FixAsync(context.Document, container, ct),
                    equivalenceKey: nameof(ElementOrderCodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> FixAsync(Document document, SyntaxNode container, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var reordered = ReorderMembers(container);

            if (root == null || reordered == null)
                return document;

            return document.WithSyntaxRoot(root.ReplaceNode(container, reordered));
        }

        private static SyntaxNode? ReorderMembers(SyntaxNode container)
        {
            switch (container)
            {
                case TypeDeclarationSyntax type:
                    return Sort(type.Members, TypeMemberKey, type.CloseBraceToken) is { } typeMembers ? type.WithMembers(typeMembers) : null;
                case BaseNamespaceDeclarationSyntax ns:
                    return Sort(ns.Members, OuterKey, default) is { } namespaceMembers ? ns.WithMembers(namespaceMembers) : null;
                case CompilationUnitSyntax unit:
                    return Sort(unit.Members, OuterKey, default) is { } unitMembers ? unit.WithMembers(unitMembers) : null;
                default:
                    return null;
            }
        }

        private static SyntaxList<MemberDeclarationSyntax>? Sort(
            SyntaxList<MemberDeclarationSyntax> members,
            System.Func<MemberDeclarationSyntax, int[]?> key,
            SyntaxToken closeBrace)
        {
            if (members.Count < 2 || members.Any(m => m.ContainsDirectives) || closeBrace.LeadingTrivia.Any(t => t.IsDirective))
                return null;

            var keys = members.Select(key).ToList();

            if (keys.Any(k => k == null))
                return null;

            var sorted = members
                .Select((member, index) => (Member: member, Key: keys[index]!, Index: index))
                .OrderBy(entry => entry.Key, KeyComparer.Instance)
                .ThenBy(entry => entry.Index)
                .Select(entry => entry.Member)
                .ToList();

            if (sorted.SequenceEqual(members))
                return null;

            // Blank lines belong to the position, comments to the member they document.
            return SyntaxFactory.List(sorted.Select((member, i) =>
                member.WithLeadingTrivia(PositionTrivia(members[i]).AddRange(MemberTrivia(member)))));
        }

        private static SyntaxTriviaList PositionTrivia(MemberDeclarationSyntax member)
        {
            var trivia = member.GetLeadingTrivia();
            return SyntaxFactory.TriviaList(trivia.Take(PositionTriviaLength(trivia)));
        }

        private static SyntaxTriviaList MemberTrivia(MemberDeclarationSyntax member)
        {
            var trivia = member.GetLeadingTrivia();
            return SyntaxFactory.TriviaList(trivia.Skip(PositionTriviaLength(trivia)));
        }

        // Whitespace and line breaks up to the last line break before the first comment (or the member itself).
        private static int PositionTriviaLength(SyntaxTriviaList trivia)
        {
            var length = 0;

            for (var i = 0; i < trivia.Count; i++)
            {
                if (trivia[i].IsKind(SyntaxKind.EndOfLineTrivia))
                    length = i + 1;
                else if (!trivia[i].IsKind(SyntaxKind.WhitespaceTrivia))
                    break;
            }

            return length;
        }

        private static int[]? TypeMemberKey(MemberDeclarationSyntax member)
        {
            var kind = GM1201Analyzer.TypeMemberOrder.IndexOf(GM1201Analyzer.GetOrderingKind(member.Kind()));

            if (kind < 0)
                return null;

            var modifiers = member.Modifiers;
            var isField = member is FieldDeclarationSyntax;
            var isConst = modifiers.Any(SyntaxKind.ConstKeyword);

            return new[]
            {
                kind,
                (int)OrderingHelpers.GetAccessLevel(modifiers),
                isField && isConst ? 0 : 1,
                modifiers.Any(SyntaxKind.StaticKeyword) || isConst ? 0 : 1,
                isField && modifiers.Any(SyntaxKind.ReadOnlyKeyword) ? 0 : 1,
            };
        }

        private static int[]? OuterKey(MemberDeclarationSyntax member)
        {
            var kind = GM1201Analyzer.OuterOrder.IndexOf(GM1201Analyzer.GetOrderingKind(member.Kind()));

            return kind < 0 ? null : new[] { kind };
        }

        private sealed class KeyComparer : IComparer<int[]>
        {
            public static readonly KeyComparer Instance = new KeyComparer();

            public int Compare(int[]? x, int[]? y)
            {
                for (var i = 0; i < x!.Length && i < y!.Length; i++)
                {
                    if (x[i] != y[i])
                        return x[i].CompareTo(y[i]);
                }

                return 0;
            }
        }
    }
}
