using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0121Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0121";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Object initializer braces must be indented at the declaration level",
            messageFormat: "Indent this brace to match the object instantiation indentation",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Opening and closing braces of an object initializer must be indented at the same level as the line containing the 'new' keyword.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.ImplicitObjectCreationExpression,
                SyntaxKind.AnonymousObjectCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            SyntaxToken openBrace, closeBrace, newKeyword;

            if (context.Node is ObjectCreationExpressionSyntax objectCreation)
            {
                var initializer = objectCreation.Initializer;
                if (initializer == null || !initializer.IsKind(SyntaxKind.ObjectInitializerExpression))
                    return;
                openBrace = initializer.OpenBraceToken;
                closeBrace = initializer.CloseBraceToken;
                newKeyword = objectCreation.NewKeyword;
            }
            else if (context.Node is ImplicitObjectCreationExpressionSyntax implicitCreation)
            {
                var initializer = implicitCreation.Initializer;
                if (initializer == null || !initializer.IsKind(SyntaxKind.ObjectInitializerExpression))
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

            CheckBrace(context, tree, sourceText, openBrace, newKeywordLine, declarationIndent);
            CheckBrace(context, tree, sourceText, closeBrace, newKeywordLine, declarationIndent);
        }

        private static void CheckBrace(
            SyntaxNodeAnalysisContext context,
            SyntaxTree tree,
            SourceText sourceText,
            SyntaxToken braceToken,
            int anchorLine,
            int expectedIndent)
        {
            if (braceToken.IsMissing)
                return;

            var braceLine = tree.GetLineSpan(braceToken.Span).StartLinePosition.Line;
            if (braceLine == anchorLine)
                return;

            var actualIndent = CountLeadingWhitespace(sourceText.Lines[braceLine].ToString());
            if (actualIndent != expectedIndent)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, braceToken.GetLocation()));
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
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
