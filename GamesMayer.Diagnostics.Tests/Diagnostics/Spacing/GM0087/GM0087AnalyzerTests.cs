namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0087Analyzer>;

    public class GM0087AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceAfterColon_DefaultSetting_Diagnostic()
        {
            var testCode = @"interface I
{
}

class C {|GM0087::|}I
{
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterColon_DefaultSetting_NoDiagnostic()
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
        public async Task SpaceAfterColon_EnabledFalse_Diagnostic()
        {
            var testCode = @"interface I
{
}

class C {|GM0087::|} I
{
}";

            var test = new CSharpAnalyzerTest<GM0087Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0087.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterColon_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"interface I
{
}

class C :I
{
}";

            var test = new CSharpAnalyzerTest<GM0087Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0087.enabled = false"));

            await test.RunAsync();
        }
    }
}
