namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0085CodeFixProviderTests
    {
        [Fact]
        public async Task WithSpaces_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0085:(|} value );
    }
}";
            var fixedCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M(value);
    }
}";

            var test = new CSharpCodeFixTest<GM0085Analyzer, GM0085CodeFixProvider, XUnitVerifier>
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
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0085:(|}value);
    }
}";
            var fixedCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M( value );
    }
}";

            var test = new CSharpCodeFixTest<GM0085Analyzer, GM0085CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0085.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0085.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MixedSpaces_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0085:(|} value);
    }
}";
            var fixedCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M(value);
    }
}";

            var test = new CSharpCodeFixTest<GM0085Analyzer, GM0085CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
