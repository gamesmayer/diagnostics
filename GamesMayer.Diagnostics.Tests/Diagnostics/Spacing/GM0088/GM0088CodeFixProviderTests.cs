namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0088CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaces_DefaultSetting_AddsSpaces()
        {
            var testCode = @"class Foo
{
    int M(int a, int b)
    {
        return a{|GM0088:+|}b;
    }
}";
            var fixedCode = @"class Foo
{
    int M(int a, int b)
    {
        return a + b;
    }
}";

            var test = new CSharpCodeFixTest<GM0088Analyzer, GM0088CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task Spaces_EnabledFalse_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    bool M(int a, int b)
    {
        return a {|GM0088:==|} b;
    }
}";
            var fixedCode = @"class Foo
{
    bool M(int a, int b)
    {
        return a==b;
    }
}";

            var test = new CSharpCodeFixTest<GM0088Analyzer, GM0088CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0088.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0088.enabled = false"));

            await test.RunAsync();
        }
    }
}
