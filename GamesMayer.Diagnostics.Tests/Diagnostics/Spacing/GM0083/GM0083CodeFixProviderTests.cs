namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0083CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpace_DefaultSetting_AddsSpace()
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
            var fixedCode = @"class Foo
{
    void M(bool value)
    {
        if (value)
        {
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0083Analyzer, GM0083CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpace_EnabledFalse_RemovesSpace()
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
            var fixedCode = @"class Foo
{
    void M(bool value)
    {
        if(value)
        {
        }
    }
}";

            var test = new CSharpCodeFixTest<GM0083Analyzer, GM0083CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0083.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0083.enabled = false"));

            await test.RunAsync();
        }
    }
}
