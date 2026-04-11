using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0142Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0142";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Generic array creation is not allowed",
            messageFormat: "Replace 'Array.Empty<{0}>()' with 'new {0}[0]'",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Use 'new T[0]' instead of 'Array.Empty<T>()' to create empty arrays.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            if (memberAccess.Expression is not IdentifierNameSyntax typeName || typeName.Identifier.Text != "Array")
                return;

            if (memberAccess.Name is not GenericNameSyntax genericName || genericName.Identifier.Text != "Empty")
                return;

            if (genericName.TypeArgumentList.Arguments.Count != 1)
                return;

            var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol == null || symbol.ContainingType?.ToDisplayString() != "System.Array")
                return;

            var typeArg = genericName.TypeArgumentList.Arguments[0].ToString();

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), typeArg));
        }
    }
}
