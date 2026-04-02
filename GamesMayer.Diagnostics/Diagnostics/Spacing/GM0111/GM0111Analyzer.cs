using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0111Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0111";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Statement closing semicolon must be adjacent to previous token",
            messageFormat: "Move ';' next to the previous token",
            category: "Spacing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Statement-closing semicolons must be written immediately after the previous token, without spaces or line breaks.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);

            foreach (var semicolonToken in root.DescendantTokens())
            {
                if (!semicolonToken.IsKind(SyntaxKind.SemicolonToken) || !IsStatementClosingSemicolon(semicolonToken))
                    continue;

                var previousToken = semicolonToken.GetPreviousToken();
                if (previousToken.IsKind(SyntaxKind.None))
                    continue;

                var betweenSpan = TextSpan.FromBounds(previousToken.Span.End, semicolonToken.SpanStart);
                if (betweenSpan.Length == 0)
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, semicolonToken.GetLocation()));
            }
        }

        private static bool IsStatementClosingSemicolon(SyntaxToken semicolonToken)
        {
            if (semicolonToken.IsMissing || !semicolonToken.IsKind(SyntaxKind.SemicolonToken))
                return false;

            if (semicolonToken.Parent is ForStatementSyntax)
                return false;

            return semicolonToken.Parent is StatementSyntax;
        }
    }
}
