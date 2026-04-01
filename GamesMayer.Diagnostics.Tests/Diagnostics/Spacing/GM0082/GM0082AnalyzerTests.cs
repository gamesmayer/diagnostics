namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0082Analyzer>;

    public class GM0082AnalyzerTests
    {
        [Fact]
        public async Task NoSpace_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int)1.0f;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithSpace_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int{|GM0082:)|} 1.0f;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpace_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int{|GM0082:)|}1.0f;
    }
}";
            var test = new CSharpAnalyzerTest<GM0082Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpace_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int) 1.0f;
    }
}";
            var test = new CSharpAnalyzerTest<GM0082Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int)1.0f;
    }
}";
            var test = new CSharpAnalyzerTest<GM0082Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpace_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int{|GM0082:)|} 1.0f;
    }
}";
            var test = new CSharpAnalyzerTest<GM0082Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task CastAcrossLines_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int)
            1.0f;
    }
}";
            var test = new CSharpAnalyzerTest<GM0082Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = true"));

            await test.RunAsync();
        }
    }
}
