global using XUnitVerifier = Microsoft.CodeAnalysis.Testing.DefaultVerifier;

namespace Microsoft.CodeAnalysis.Testing.Verifiers
{
}

namespace Microsoft.CodeAnalysis.CSharp.Testing.XUnit
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;

    public static class AnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public static DiagnosticResult Diagnostic() =>
            Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<TAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>.Diagnostic();

        public static DiagnosticResult Diagnostic(string diagnosticId) =>
            Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<TAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>.Diagnostic(diagnosticId);

        public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected) =>
            Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<TAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>.VerifyAnalyzerAsync(source, expected);
    }
}