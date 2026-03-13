using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0019Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0019";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Empty braces enclosure must be on a single line",
            messageFormat: "Write empty braces on a single line",
            category: "Layout",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Empty class and method body braces must be written on a single line.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ClassDeclarationSyntax)context.Node;

            if (declaration.Members.Count != 0)
            {
                return;
            }

            ReportIfBraceEnclosureInvalid(context, declaration.OpenBraceToken, declaration.CloseBraceToken);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (MethodDeclarationSyntax)context.Node;
            var body = declaration.Body;

            if (body == null || body.Statements.Count != 0)
            {
                return;
            }

            ReportIfBraceEnclosureInvalid(context, body.OpenBraceToken, body.CloseBraceToken);
        }

        private static void ReportIfBraceEnclosureInvalid(
            SyntaxNodeAnalysisContext context,
            SyntaxToken openBraceToken,
            SyntaxToken closeBraceToken)
        {
            if (openBraceToken.IsMissing || closeBraceToken.IsMissing)
            {
                return;
            }

            var tree = context.Node.SyntaxTree;
            var openBraceLine = tree.GetLineSpan(openBraceToken.Span).StartLinePosition.Line;
            var closeBraceLine = tree.GetLineSpan(closeBraceToken.Span).StartLinePosition.Line;
            var textBetweenBraces = tree.GetText(context.CancellationToken).ToString(TextSpan.FromBounds(openBraceToken.Span.End, closeBraceToken.Span.Start));

            if (openBraceLine == closeBraceLine && textBetweenBraces == " ")
            {
                return;
            }

            var span = TextSpan.FromBounds(openBraceToken.SpanStart, closeBraceToken.Span.End);
            var location = Location.Create(tree, span);
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
