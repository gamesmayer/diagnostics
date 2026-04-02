namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0099CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceAfterSemicolons_DefaultSetting_AddsSpaces()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0099:;|}i < 10{|GM0099:;|}i++) { }
    }
}";
            var fixedCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0; i < 10; i++) { }
    }
}";

            var test = new CSharpCodeFixTest<GM0099Analyzer, GM0099CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceAfterSemicolons_EnabledFalse_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0099:;|} i < 10{|GM0099:;|} i++) { }
    }
}";
            var fixedCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0;i < 10;i++) { }
    }
}";

            var test = new CSharpCodeFixTest<GM0099Analyzer, GM0099CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0099.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0099.enabled = false"));

            await test.RunAsync();
        }
    }
}
