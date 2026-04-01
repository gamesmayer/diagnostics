namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0082CodeFixProviderTests
    {
        [Fact]
        public async Task WithSpace_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int{|GM0082:)|} 1.0f;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x = (int)1.0f;
    }
}";
            var test = new CSharpCodeFixTest<GM0082Analyzer, GM0082CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int{|GM0082:)|}1.0f;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x = (int) 1.0f;
    }
}";
            var test = new CSharpCodeFixTest<GM0082Analyzer, GM0082CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpace_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x = (int{|GM0082:)|} 1.0f;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x = (int)1.0f;
    }
}";
            var test = new CSharpCodeFixTest<GM0082Analyzer, GM0082CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0082.enabled = false"));

            await test.RunAsync();
        }
    }
}
