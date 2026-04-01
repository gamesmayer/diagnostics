namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0086Analyzer>;

    public class GM0086AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeColon_DefaultSetting_Diagnostic()
        {
            var testCode = @"interface I
{
}

class C{|GM0086::|}I
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeColon_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"interface I
{
}

class C : I
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeColon_EnabledFalse_Diagnostic()
        {
            var testCode = @"interface I
{
}

class C {|GM0086::|} I
{
}";

            var test = new CSharpAnalyzerTest<GM0086Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0086.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeColon_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"interface I
{
}

class C: I
{
}";

            var test = new CSharpAnalyzerTest<GM0086Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0086.enabled = false"));

            await test.RunAsync();
        }
    }
}
