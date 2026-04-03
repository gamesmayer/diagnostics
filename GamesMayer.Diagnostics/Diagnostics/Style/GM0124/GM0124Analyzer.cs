using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0124Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0124";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Trailing commas are not allowed",
            messageFormat: "Remove trailing comma",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Disallows trailing commas in places where they are syntactically valid.");

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

            foreach (var commaToken in root.DescendantTokens())
            {
                if (!commaToken.IsKind(SyntaxKind.CommaToken))
                    continue;

                if (commaToken.IsMissing || commaToken.Parent == null || commaToken.Parent.ContainsDiagnostics)
                    continue;

                var nextToken = commaToken.GetNextToken();
                if (nextToken == default || nextToken.IsMissing)
                    continue;

                if (!IsClosingDelimiter(nextToken.Kind()))
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, commaToken.GetLocation()));
            }
        }

        private static bool IsClosingDelimiter(SyntaxKind kind)
        {
            return kind == SyntaxKind.CloseParenToken
                || kind == SyntaxKind.CloseBracketToken
                || kind == SyntaxKind.CloseBraceToken;
        }
    }
}