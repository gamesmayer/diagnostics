namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0125Analyzer>;

    public class GM0125AnalyzerTests
    {
        [Fact]
        public async Task WithSpace_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsBusy { get; set; }
    public void M() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsBusy{|GM0125:{|}get; set; }
    public void M(){|GM0125:{|} }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithSpace_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsBusy { get; set; }
    public void M() { }
}";
            var test = new CSharpAnalyzerTest<GM0125Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsBusy{|GM0125:{|}get; set; }
    public void M(){|GM0125:{|} }
}";
            var test = new CSharpAnalyzerTest<GM0125Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpace_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsBusy {|GM0125:{|} get; set; }
    public void M() {|GM0125:{|}
    }
}";
            var test = new CSharpAnalyzerTest<GM0125Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsBusy{get; set; }
    public void M(){ }
}";
            var test = new CSharpAnalyzerTest<GM0125Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task BraceOnNextLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void M()
    {
    }
}";
            var test = new CSharpAnalyzerTest<GM0125Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = true"));

            await test.RunAsync();
        }
    }
}
