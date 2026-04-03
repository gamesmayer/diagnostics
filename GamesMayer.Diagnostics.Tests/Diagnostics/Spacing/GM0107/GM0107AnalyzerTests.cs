namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0107Analyzer>;

    public class GM0107AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceAfterArrow_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M() {|GM0107:=>|}1;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterArrow_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M() => 1;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterArrow_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M() => 1;
}";

            var test = new CSharpAnalyzerTest<GM0107Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0107.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterArrow_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M() {|GM0107:=>|}1;
}";

            var test = new CSharpAnalyzerTest<GM0107Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0107.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task ArrowFollowedByNewline_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"using System;
class Foo
{
    Func<int, int> F = x =>
        x + 1;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
