using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GamesMayer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class GM0076Analyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GM0076";

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Avoid using Unity Debug.Log methods directly",
            messageFormat: "Use a log wrapper instead of Debug.{0}",
            category: "Architecture",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid using UnityEngine.Debug.Log, Debug.LogWarning, and Debug.LogError directly. Use IAppLogger abstractions instead.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;

            if (symbol == null)
                return;

            if (!IsUnityDebugType(symbol.ContainingType))
                return;

            var methodName = symbol.Name;
            if (methodName != "Log" && methodName != "LogWarning" && methodName != "LogError")
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation(), methodName));
        }

        private static bool IsUnityDebugType(INamedTypeSymbol? typeSymbol)
        {
            if (typeSymbol == null)
                return false;

            return typeSymbol.Name == "Debug"
                && typeSymbol.ContainingNamespace?.ToDisplayString() == "UnityEngine";
        }
    }
}
