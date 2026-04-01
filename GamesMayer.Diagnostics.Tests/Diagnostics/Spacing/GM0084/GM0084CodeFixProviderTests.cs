namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0084CodeFixProviderTests
    {
        [Fact]
        public async Task WithSpaces_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|} int value )
    {
    }
}";
            var fixedCode = @"class Foo
{
    void M(int value)
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0084Analyzer, GM0084CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task WithoutSpaces_EnabledTrue_AddsSpaces()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|}int value)
    {
    }
}";
            var fixedCode = @"class Foo
{
    void M( int value )
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0084Analyzer, GM0084CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0084.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0084.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MixedSpaces_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|} int value)
    {
    }
}";
            var fixedCode = @"class Foo
{
    void M(int value)
    {
    }
}";

            var test = new CSharpCodeFixTest<GM0084Analyzer, GM0084CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
