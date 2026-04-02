namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0100CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBeforeSemicolons_DefaultSetting_RemovesSpaces()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0 {|GM0100:;|} i < 10 {|GM0100:;|} i++) { }
    }
}";
            var fixedCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0; i < 10; i++) { }
    }
}";

            var test = new CSharpCodeFixTest<GM0100Analyzer, GM0100CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeSemicolons_EnabledTrue_AddsSpaces()
        {
            var testCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0{|GM0100:;|} i < 10{|GM0100:;|} i++) { }
    }
}";
            var fixedCode = @"class Foo
{
    void Bar()
    {
        for (int i = 0 ; i < 10 ; i++) { }
    }
}";

            var test = new CSharpCodeFixTest<GM0100Analyzer, GM0100CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0100.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0100.enabled = true"));

            await test.RunAsync();
        }
    }
}
