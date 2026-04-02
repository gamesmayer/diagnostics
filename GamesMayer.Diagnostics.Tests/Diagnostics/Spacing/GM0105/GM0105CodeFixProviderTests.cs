namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0105CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpacesAroundTernary_DefaultSetting_AddsSpaces()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a{|GM0105:?|}b : c;
}";
            var fixedCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a ? b : c;
}";

            var test = new CSharpCodeFixTest<GM0105Analyzer, GM0105CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpacesAroundTernary_EnabledFalse_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a?b{|GM0105::|} c;
}";
            var fixedCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a?b:c;
}";

            var test = new CSharpCodeFixTest<GM0105Analyzer, GM0105CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0105.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0105.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task PartialSpacing_DefaultSetting_AddsMissingSpacesAroundTernary()
        {
            var testCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a ? b{|GM0105::|}c;
}";
            var fixedCode = @"class Foo
{
    bool M(bool a, bool b, bool c) => a ? b : c;
}";

            var test = new CSharpCodeFixTest<GM0105Analyzer, GM0105CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
