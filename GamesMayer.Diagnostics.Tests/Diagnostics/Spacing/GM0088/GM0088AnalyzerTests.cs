namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0088Analyzer>;

    public class GM0088AnalyzerTests
    {
        [Fact]
        public async Task NoSpaces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return a{|GM0088:+|}b;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return a + b;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Spaces_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    bool M(int a, int b)
    {
        return a {|GM0088:==|} b;
    }
}";

            var test = new CSharpAnalyzerTest<GM0088Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0088.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaces_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    bool M(int a, int b)
    {
        return a==b;
    }
}";

            var test = new CSharpAnalyzerTest<GM0088Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0088.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MixedSpaces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return a {|GM0088:+|}b;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
