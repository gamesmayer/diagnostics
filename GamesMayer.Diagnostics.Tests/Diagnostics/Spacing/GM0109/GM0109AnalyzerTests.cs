namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0109Analyzer>;

    public class GM0109AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeAssign_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x{|GM0109:=|} 1;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeAssign_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x =1;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeAssign_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x {|GM0109:=|}1;
    }
}";

            var test = new CSharpAnalyzerTest<GM0109Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0109.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeAssign_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x=1;
    }
}";

            var test = new CSharpAnalyzerTest<GM0109Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0109.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task EqualsPrecededByNewline_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x
            = 1;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
