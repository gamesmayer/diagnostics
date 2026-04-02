namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0110Analyzer>;

    public class GM0110AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceAfterAssign_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x {|GM0110:=|}1;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterAssign_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x= 1;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterAssign_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x {|GM0110:=|} 1;
    }
}";

            var test = new CSharpAnalyzerTest<GM0110Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0110.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterAssign_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x=1;
    }
}";

            var test = new CSharpAnalyzerTest<GM0110Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0110.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task EqualsFollowedByNewline_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x =
            1;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
