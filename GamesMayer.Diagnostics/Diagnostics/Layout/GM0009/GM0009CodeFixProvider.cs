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

namespace GamesMayer.Diagnostics
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GM0009CodeFixProvider))]
    [Shared]
    public sealed class GM0009CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GM0009Analyzer.DiagnosticId);

        public override FixAllProvider? GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove line breaks in declaration",
                    createChangedDocument: ct => RemoveLineBreaksAsync(context.Document, node, ct),
                    equivalenceKey: nameof(GM0009CodeFixProvider)),
                diagnostic);
        }

        private static Dictionary<SyntaxToken, SyntaxToken> CreateTokenReplacements(IReadOnlyList<SyntaxToken> tokens)
        {
            var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                SyntaxToken newToken;

                if (i == 0)
                {
                    newToken = token.WithTrailingTrivia();
                }
                else
                {
                    var previous = tokens[i - 1];
                    bool noSpaceBeforeCurrent = RequiresCompactSpacing(previous, token);
                    var leadingTrivia = noSpaceBeforeCurrent
                        ? default(SyntaxTriviaList)
                        : SyntaxFactory.TriviaList(SyntaxFactory.Space);

                    newToken = token.WithLeadingTrivia(leadingTrivia);

                    if (i != tokens.Count - 1)
                        newToken = newToken.WithTrailingTrivia();
                }

                replacements[token] = newToken;
            }

            return replacements;
        }

        private static bool RequiresCompactSpacing(SyntaxToken previous, SyntaxToken current)
        {
            if (current.IsKind(SyntaxKind.OpenBracketToken) ||
                current.IsKind(SyntaxKind.CloseBracketToken) ||
                current.IsKind(SyntaxKind.LessThanToken) ||
                current.IsKind(SyntaxKind.GreaterThanToken))
            {
                return true;
            }

            if (current.IsKind(SyntaxKind.CommaToken))
            {
                return true;
            }

            if (previous.IsKind(SyntaxKind.OpenBracketToken) ||
                previous.IsKind(SyntaxKind.LessThanToken))
            {
                return true;
            }

            if (previous.IsKind(SyntaxKind.CommaToken) && previous.Parent is ArrayRankSpecifierSyntax)
            {
                return true;
            }

            return false;
        }

        private static async Task<Document> RemoveLineBreaksAsync(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            SyntaxNode newNode = node switch
            {
                ClassDeclarationSyntax classDecl => RemoveLineBreaksFromClassDeclaration(classDecl),
                StructDeclarationSyntax structDecl => RemoveLineBreaksFromStructDeclaration(structDecl),
                RecordDeclarationSyntax recordDecl => RemoveLineBreaksFromRecordDeclaration(recordDecl),
                InterfaceDeclarationSyntax interfaceDecl => RemoveLineBreaksFromInterfaceDeclaration(interfaceDecl),
                FieldDeclarationSyntax fieldDecl => RemoveLineBreaksFromFieldDeclaration(fieldDecl),
                PropertyDeclarationSyntax propertyDecl => RemoveLineBreaksFromPropertyDeclaration(propertyDecl),
                MethodDeclarationSyntax methodDecl => RemoveLineBreaksFromMethodDeclaration(methodDecl),
                EventDeclarationSyntax eventDecl => RemoveLineBreaksFromEventDeclaration(eventDecl),
                ConstructorDeclarationSyntax constructorDecl => RemoveLineBreaksFromConstructorDeclaration(constructorDecl),
                _ => node
            };

            var newRoot = root.ReplaceNode(node, newNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private static SyntaxNode RemoveLineBreaksFromClassDeclaration(ClassDeclarationSyntax classDecl)
        {
            return RemoveLineBreaksFromTypeDeclaration(
                classDecl,
                classDecl.Modifiers,
                classDecl.Keyword,
                classDecl.Identifier,
                (decl, mods) => decl.WithModifiers(mods),
                (decl, keyword) => decl.WithKeyword(keyword),
                (decl, id) => decl.WithIdentifier(id));
        }

        private static SyntaxNode RemoveLineBreaksFromStructDeclaration(StructDeclarationSyntax structDecl)
        {
            return RemoveLineBreaksFromTypeDeclaration(
                structDecl,
                structDecl.Modifiers,
                structDecl.Keyword,
                structDecl.Identifier,
                (decl, mods) => decl.WithModifiers(mods),
                (decl, keyword) => decl.WithKeyword(keyword),
                (decl, id) => decl.WithIdentifier(id));
        }

        private static SyntaxNode RemoveLineBreaksFromRecordDeclaration(RecordDeclarationSyntax recordDecl)
        {
            return RemoveLineBreaksFromTypeDeclaration(
                recordDecl,
                recordDecl.Modifiers,
                recordDecl.Keyword,
                recordDecl.Identifier,
                (decl, mods) => decl.WithModifiers(mods),
                (decl, keyword) => decl.WithKeyword(keyword),
                (decl, id) => decl.WithIdentifier(id));
        }

        private static SyntaxNode RemoveLineBreaksFromInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDecl)
        {
            return RemoveLineBreaksFromTypeDeclaration(
                interfaceDecl,
                interfaceDecl.Modifiers,
                interfaceDecl.Keyword,
                interfaceDecl.Identifier,
                (decl, mods) => decl.WithModifiers(mods),
                (decl, keyword) => decl.WithKeyword(keyword),
                (decl, id) => decl.WithIdentifier(id));
        }

        private static T RemoveLineBreaksFromTypeDeclaration<T>(
            T declaration,
            SyntaxTokenList modifiers,
            SyntaxToken keyword,
            SyntaxToken identifier,
            System.Func<T, SyntaxTokenList, T> withModifiers,
            System.Func<T, SyntaxToken, T> withKeyword,
            System.Func<T, SyntaxToken, T> withIdentifier) where T : SyntaxNode
        {
            var firstToken = modifiers.Count > 0 ? modifiers[0] : keyword;
            var lastToken = identifier;
            
            var tokens = new List<SyntaxToken>();
            var current = firstToken;
            while (current.Span.Start <= lastToken.Span.Start)
            {
                tokens.Add(current);
                if (current == lastToken) break;
                current = current.GetNextToken();
            }

            var replacements = CreateTokenReplacements(tokens);

            return (T)declaration.ReplaceTokens(replacements.Keys, (oldToken, _) => replacements[oldToken]);
        }

        private static SyntaxNode RemoveLineBreaksFromFieldDeclaration(FieldDeclarationSyntax fieldDecl)
        {
            var firstToken = fieldDecl.AttributeLists.Count > 0
                ? fieldDecl.AttributeLists.Last().GetLastToken().GetNextToken()
                : fieldDecl.GetFirstToken();

            var lastToken = fieldDecl.Declaration.Variables.FirstOrDefault()?.Identifier ?? fieldDecl.GetFirstToken();
            
            var tokens = new List<SyntaxToken>();
            var current = firstToken;
            while (current.Span.Start <= lastToken.Span.Start)
            {
                tokens.Add(current);
                if (current == lastToken) break;
                current = current.GetNextToken();
            }

            var replacements = CreateTokenReplacements(tokens);

            return fieldDecl.ReplaceTokens(replacements.Keys, (oldToken, _) => replacements[oldToken]);
        }

        private static SyntaxNode RemoveLineBreaksFromPropertyDeclaration(PropertyDeclarationSyntax propertyDecl)
        {
            var firstToken = propertyDecl.AttributeLists.Count > 0
                ? propertyDecl.AttributeLists.Last().GetLastToken().GetNextToken()
                : propertyDecl.GetFirstToken();

            var lastToken = propertyDecl.Identifier;
            
            var tokens = new List<SyntaxToken>();
            var current = firstToken;
            while (current.Span.Start <= lastToken.Span.Start)
            {
                tokens.Add(current);
                if (current == lastToken) break;
                current = current.GetNextToken();
            }

            var replacements = CreateTokenReplacements(tokens);

            return propertyDecl.ReplaceTokens(replacements.Keys, (oldToken, _) => replacements[oldToken]);
        }

        private static SyntaxNode RemoveLineBreaksFromMethodDeclaration(MethodDeclarationSyntax methodDecl)
        {
            var firstToken = methodDecl.AttributeLists.Count > 0
                ? methodDecl.AttributeLists.Last().GetLastToken().GetNextToken()
                : methodDecl.GetFirstToken();

            var lastToken = methodDecl.Identifier;
            
            var tokens = new List<SyntaxToken>();
            var current = firstToken;
            while (current.Span.Start <= lastToken.Span.Start)
            {
                tokens.Add(current);
                if (current == lastToken) break;
                current = current.GetNextToken();
            }

            var replacements = CreateTokenReplacements(tokens);

            return methodDecl.ReplaceTokens(replacements.Keys, (oldToken, _) => replacements[oldToken]);
        }

        private static SyntaxNode RemoveLineBreaksFromEventDeclaration(EventDeclarationSyntax eventDecl)
        {
            var firstToken = eventDecl.AttributeLists.Count > 0
                ? eventDecl.AttributeLists.Last().GetLastToken().GetNextToken()
                : eventDecl.GetFirstToken();

            var lastToken = eventDecl.Identifier;
            
            var tokens = new List<SyntaxToken>();
            var current = firstToken;
            while (current.Span.Start <= lastToken.Span.Start)
            {
                tokens.Add(current);
                if (current == lastToken) break;
                current = current.GetNextToken();
            }

            var replacements = CreateTokenReplacements(tokens);

            return eventDecl.ReplaceTokens(replacements.Keys, (oldToken, _) => replacements[oldToken]);
        }

        private static SyntaxNode RemoveLineBreaksFromConstructorDeclaration(ConstructorDeclarationSyntax constructorDecl)
        {
            var firstToken = constructorDecl.AttributeLists.Count > 0
                ? constructorDecl.AttributeLists.Last().GetLastToken().GetNextToken()
                : constructorDecl.GetFirstToken();

            var lastToken = constructorDecl.Identifier;
            
            var tokens = new List<SyntaxToken>();
            var current = firstToken;
            while (current.Span.Start <= lastToken.Span.Start)
            {
                tokens.Add(current);
                if (current == lastToken) break;
                current = current.GetNextToken();
            }

            var replacements = CreateTokenReplacements(tokens);

            return constructorDecl.ReplaceTokens(replacements.Keys, (oldToken, _) => replacements[oldToken]);
        }
    }
}
