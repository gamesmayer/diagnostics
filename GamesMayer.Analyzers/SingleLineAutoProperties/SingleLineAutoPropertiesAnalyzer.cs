using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SingleLineAutoPropertiesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0002";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Auto-implemented property must be on a single line",
            messageFormat: "Auto-implemented property '{0}' must be written on a single line",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Auto-implemented properties must not span multiple lines.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax)context.Node;

            if (!IsAutoImplemented(property))
                return;

            // Exclude leading attributes — measure only from the first non-attribute token.
            var firstToken = property.AttributeLists.Count > 0
                ? property.AttributeLists.Last().GetLastToken().GetNextToken()
                : property.GetFirstToken();

            var startLine = firstToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            var endLine = property.GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;

            if (startLine != endLine)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Descriptor, property.GetLocation(), property.Identifier.Text));
            }
        }

        private static bool IsAutoImplemented(PropertyDeclarationSyntax property)
        {
            if (property.AccessorList == null || property.AccessorList.Accessors.Count == 0)
                return false;

            foreach (var accessor in property.AccessorList.Accessors)
            {
                if (accessor.Body != null || accessor.ExpressionBody != null)
                    return false;
            }

            return true;
        }
    }
}
