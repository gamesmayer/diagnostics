namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0100Analyzer>;

    public class GM0100AnalyzerTests
    {
        [Fact]
        public async Task NoSpaceBeforeSemicolons_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0; i < 10; i++) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceBeforeSemicolons_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0 {|GM0100:;|} i < 10 {|GM0100:;|} i++) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBeforeSemicolons_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0; i < 10; i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0100Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0100.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeSemicolons_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0 {|GM0100:;|} i < 10 {|GM0100:;|} i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0100Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0100.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeSemicolons_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0 ; i < 10 ; i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0100Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0100.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeSemicolons_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0100:;|} i < 10{|GM0100:;|} i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0100Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0100.enabled = true"));

            await test.RunAsync();
        }
    }
}
