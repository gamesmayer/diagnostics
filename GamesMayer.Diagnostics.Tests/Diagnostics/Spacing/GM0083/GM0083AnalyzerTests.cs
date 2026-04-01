namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0083Analyzer>;

    public class GM0083AnalyzerTests
    {
        [Fact]
        public async Task NoSpace_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        {|GM0083:if|}(value)
        {
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithSpace_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        if (value)
        {
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithSpace_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        {|GM0083:if|} (value)
        {
        }
    }
}";

            var test = new CSharpAnalyzerTest<GM0083Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0083.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        if(value)
        {
        }
    }
}";

            var test = new CSharpAnalyzerTest<GM0083Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0083.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task CatchWithoutDeclaration_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        try
        {
        }
        catch
        {
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
