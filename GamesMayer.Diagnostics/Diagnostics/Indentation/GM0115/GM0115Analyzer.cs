using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0115Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0115";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Array initializer braces must be indented at the declaration level",
            messageFormat: "Indent this brace to match the array declaration indentation",
            category: "Indentation",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Opening and closing braces of an array initializer must be indented at the same level as the containing declaration.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode,
                SyntaxKind.ArrayCreationExpression,
                SyntaxKind.ImplicitArrayCreationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            InitializerExpressionSyntax? initializer;

            if (context.Node is ArrayCreationExpressionSyntax arrayCreation)
                initializer = arrayCreation.Initializer;
            else if (context.Node is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
                initializer = implicitArrayCreation.Initializer;
            else
                return;

            if (initializer == null)
                return;

            if (context.Node.Parent is not EqualsValueClauseSyntax)
                return;

            var declarationFirstToken = FindDeclarationFirstToken(context.Node);
            if (declarationFirstToken == default)
                return;

            var tree = context.Node.SyntaxTree;
            var sourceText = tree.GetText(context.CancellationToken);

            var declarationLine = tree.GetLineSpan(declarationFirstToken.Span).StartLinePosition.Line;
            var declarationIndent = CountLeadingWhitespace(sourceText.Lines[declarationLine].ToString());

            CheckBrace(context, tree, sourceText, initializer.OpenBraceToken, declarationLine, declarationIndent);
            CheckBrace(context, tree, sourceText, initializer.CloseBraceToken, declarationLine, declarationIndent);
        }

        private static void CheckBrace(
            SyntaxNodeAnalysisContext context,
            SyntaxTree tree,
            SourceText sourceText,
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
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, braceToken.GetLocation()));
        }

        internal static SyntaxToken FindDeclarationFirstToken(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is FieldDeclarationSyntax or
                    LocalDeclarationStatementSyntax or
                    PropertyDeclarationSyntax)
                    return current.GetFirstToken();
                current = current.Parent;
            }
            return default;
        }

        internal static int CountLeadingWhitespace(string text)
        {
            int count = 0;
            while (count < text.Length && (text[count] == ' ' || text[count] == '\t'))
                count++;
            return count;
        }
    }
}
