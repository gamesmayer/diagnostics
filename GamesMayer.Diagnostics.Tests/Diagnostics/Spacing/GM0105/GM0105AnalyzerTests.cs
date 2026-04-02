namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0105Analyzer>;

    public class GM0105AnalyzerTests
    {
        [Fact]
        public async Task NoSpacesAroundTernary_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a{|GM0105:?|}b{|GM0105::|}c;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpacesAroundTernary_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a ? b : c;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MixedSpacingAroundTernary_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a ? b{|GM0105::|}c;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpacesAroundTernary_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a {|GM0105:?|} b {|GM0105::|} c;
}";

            var test = new CSharpAnalyzerTest<GM0105Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0105.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpacesAroundTernary_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a?b:c;
}";

            var test = new CSharpAnalyzerTest<GM0105Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0105.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MultilineTernary_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a
        ? b
        : c;
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
