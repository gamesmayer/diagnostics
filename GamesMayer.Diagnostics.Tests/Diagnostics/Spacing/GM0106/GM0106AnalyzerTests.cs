namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0106Analyzer>;

    public class GM0106AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeArrow_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(){|GM0106:=>|} 1;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeArrow_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M() => 1;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeArrow_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M() {|GM0106:=>|} 1;
}";

            var test = new CSharpAnalyzerTest<GM0106Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0106.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeArrow_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M()=> 1;
}";

            var test = new CSharpAnalyzerTest<GM0106Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0106.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ArrowPrecededByNewline_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo
{
    Func<int, int> F = x
        => x + 1;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
