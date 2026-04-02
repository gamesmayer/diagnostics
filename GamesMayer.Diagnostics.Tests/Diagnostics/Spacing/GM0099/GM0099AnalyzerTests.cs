namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0099Analyzer>;

    public class GM0099AnalyzerTests
    {
        [Fact]
        public async Task SpaceAfterSemicolons_DefaultSetting_NoDiagnostic()
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
        public async Task NoSpaceAfterSemicolons_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0099:;|}i < 10{|GM0099:;|}i++) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SpaceAfterSemicolons_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0; i < 10; i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0099Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0099.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterSemicolons_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0099:;|}i < 10{|GM0099:;|}i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0099Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0099.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterSemicolons_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0;i < 10;i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0099Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0099.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceAfterSemicolons_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0099:;|} i < 10{|GM0099:;|} i++) { }
    }
}";

            var test = new CSharpAnalyzerTest<GM0099Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0099.enabled = false"));

            await test.RunAsync();
        }
    }
}
