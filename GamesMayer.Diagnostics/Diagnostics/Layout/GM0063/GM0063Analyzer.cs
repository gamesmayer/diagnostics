using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0063Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0063";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Methods and lambda expressions with braces cannot be single line statements",
            messageFormat: "Expand the block body across multiple lines",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A method, constructor, destructor, operator, local function, or lambda expression whose body uses braces must not have the opening and closing brace on the same line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(
                AnalyzeNode,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.LocalFunctionStatement,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.AnonymousMethodExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            BlockSyntax? block = context.Node switch
            {
                MethodDeclarationSyntax m => m.Body,
                ConstructorDeclarationSyntax c => c.Body,
                DestructorDeclarationSyntax d => d.Body,
                OperatorDeclarationSyntax op => op.Body,
                ConversionOperatorDeclarationSyntax conv => conv.Body,
                LocalFunctionStatementSyntax lf => lf.Body,
                AnonymousMethodExpressionSyntax am => am.Block,
                ParenthesizedLambdaExpressionSyntax pl => pl.Body as BlockSyntax,
                SimpleLambdaExpressionSyntax sl => sl.Body as BlockSyntax,
                _ => null
            };

            if (block == null || block.Statements.Count == 0)
                return;

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(block.OpenBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(block.CloseBraceToken.Span).EndLinePosition.Line;

            if (openBraceLine != closeBraceLine)
                return;

            var span = TextSpan.FromBounds(block.OpenBraceToken.SpanStart, block.CloseBraceToken.Span.End);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, Location.Create(tree, span)));
        }
    }
}
