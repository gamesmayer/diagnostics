using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0078Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0078";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Brace must be indented at the declaration level",
            messageFormat: "Indent this brace to match the declaration indentation",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Opening and closing braces must be indented at the same level as the containing declaration. Applies to blocks, class/struct/namespace declarations, array initializers, and object initializers.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
            context.RegisterSyntaxNodeAction(AnalyzeDeclarationBraces,
                SyntaxKind.NamespaceDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeArrayCreation,
                SyntaxKind.ArrayCreationExpression,
                SyntaxKind.ImplicitArrayCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression,
                SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;

            if (block.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var openBraceLine = tree.GetLineSpan(block.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(block.CloseBraceToken.Span).StartLinePosition.Line;
            if (openBraceLine == closeBraceLine)
                return;

            var declarationFirstToken = block.Parent?.GetFirstToken() ?? block.OpenBraceToken;
            var declarationLine = GetDeclarationLine(tree, block.OpenBraceToken, declarationFirstToken);
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());

            CheckBrace(context, sourceText, tree, block.OpenBraceToken, declarationLine, declarationIndent);
            CheckBrace(context, sourceText, tree, block.CloseBraceToken, declarationLine, declarationIndent);
        }

        private static void AnalyzeDeclarationBraces(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken openBrace, closeBrace;

            switch (context.Node)
            {
                case NamespaceDeclarationSyntax ns:
                    openBrace = ns.OpenBraceToken;
                    closeBrace = ns.CloseBraceToken;
                    break;
                case ClassDeclarationSyntax cls:
                    openBrace = cls.OpenBraceToken;
                    closeBrace = cls.CloseBraceToken;
                    break;
                case StructDeclarationSyntax str:
                    openBrace = str.OpenBraceToken;
                    closeBrace = str.CloseBraceToken;
                    break;
                default:
                    return;
            }

            if (context.Node.ContainsDirectives)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var declarationFirstToken = context.Node.GetFirstToken();
            var declarationLine = GetDeclarationLine(tree, openBrace, declarationFirstToken);
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());

            CheckBrace(context, sourceText, tree, openBrace, declarationLine, declarationIndent);
            CheckBrace(context, sourceText, tree, closeBrace, declarationLine, declarationIndent);
        }

        private static void AnalyzeArrayCreation(SyntaxNodeAnalysisContext context)
        {
            InitializerExpressionSyntax? initializer;
            SyntaxToken newKeyword;

            if (context.Node is ArrayCreationExpressionSyntax arrayCreation)
            {
                initializer = arrayCreation.Initializer;
                newKeyword = arrayCreation.NewKeyword;
            }
            else if (context.Node is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
            {
                initializer = implicitArrayCreation.Initializer;
                newKeyword = implicitArrayCreation.NewKeyword;
            }
            else
                return;

            if (initializer == null)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var newKeywordLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[newKeywordLine].ToString());

            CheckBrace(context, sourceText, tree, initializer.OpenBraceToken, newKeywordLine, declarationIndent);
            CheckBrace(context, sourceText, tree, initializer.CloseBraceToken, newKeywordLine, declarationIndent);
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken openBrace, closeBrace, newKeyword;

            if (context.Node is ObjectCreationExpressionSyntax objectCreation)
            {
                var initializer = objectCreation.Initializer;
                if (initializer == null ||
                    (!initializer.IsKind(SyntaxKind.ObjectInitializerExpression) &&
                     !initializer.IsKind(SyntaxKind.CollectionInitializerExpression)))
                    return;
                openBrace = initializer.OpenBraceToken;
                closeBrace = initializer.CloseBraceToken;
                newKeyword = objectCreation.NewKeyword;
            }
            else if (context.Node is ImplicitObjectCreationExpressionSyntax implicitCreation)
            {
                var initializer = implicitCreation.Initializer;
                if (initializer == null ||
                    (!initializer.IsKind(SyntaxKind.ObjectInitializerExpression) &&
                     !initializer.IsKind(SyntaxKind.CollectionInitializerExpression)))
                    return;
                openBrace = initializer.OpenBraceToken;
                closeBrace = initializer.CloseBraceToken;
                newKeyword = implicitCreation.NewKeyword;
            }
            else if (context.Node is AnonymousObjectCreationExpressionSyntax anonymousCreation)
            {
                openBrace = anonymousCreation.OpenBraceToken;
                closeBrace = anonymousCreation.CloseBraceToken;
                newKeyword = anonymousCreation.NewKeyword;
            }
            else
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var newKeywordLine = tree.GetLineSpan(newKeyword.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[newKeywordLine].ToString());

            CheckBrace(context, sourceText, tree, openBrace, newKeywordLine, declarationIndent);
            CheckBrace(context, sourceText, tree, closeBrace, newKeywordLine, declarationIndent);
        }

        private static void CheckBrace(
            SyntaxNodeAnalysisContext context,
            SourceText sourceText,
            SyntaxTree tree,
            SyntaxToken braceToken,
            int declarationLine,
            int declarationIndent)
        {
            if (braceToken.IsMissing)
                return;

            var braceLine = tree.GetLineSpan(braceToken.Span).StartLinePosition.Line;
            if (braceLine == declarationLine)
                return;

            var actualIndent = CountLeadingWhitespace(sourceText.Lines[braceLine].ToString());
            if (actualIndent != declarationIndent)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, braceToken.GetLocation()));
            }
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
        }

        private static int GetDeclarationLine(
            SyntaxTree tree,
            SyntaxToken openBraceToken,
            SyntaxToken fallbackToken)
        {
            var previousToken = openBraceToken.GetPreviousToken();
            if (previousToken != default && previousToken.SpanStart < openBraceToken.SpanStart)
                return tree.GetLineSpan(previousToken.Span).StartLinePosition.Line;

            return tree.GetLineSpan(fallbackToken.Span).StartLinePosition.Line;
        }

        internal static string GetLeadingWhitespace(string text)
        {
            int count = CountLeadingWhitespace(text);
            return text.Substring(0, count);
        }

        internal static SyntaxToken FindDeclarationFirstToken(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is FieldDeclarationSyntax or
                    LocalDeclarationStatementSyntax or
                    PropertyDeclarationSyntax or
                    StatementSyntax)
                    return current.GetFirstToken();
                current = current.Parent;
            }
            return default;
        }

        internal static SyntaxToken FindNewKeyword(SyntaxNode node)
        {
            var current = node;
            while (current != null)
            {
                if (current is ObjectCreationExpressionSyntax objectCreation)
                    return objectCreation.NewKeyword;
                if (current is ImplicitObjectCreationExpressionSyntax implicitCreation)
                    return implicitCreation.NewKeyword;
                if (current is AnonymousObjectCreationExpressionSyntax anonymousCreation)
                    return anonymousCreation.NewKeyword;
                current = current.Parent;
            }
            return default;
        }
    }
}
