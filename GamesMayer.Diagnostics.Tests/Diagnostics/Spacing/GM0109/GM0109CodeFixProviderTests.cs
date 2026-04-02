namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0109CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceBeforeAssign_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x{|GM0109:=|} 1;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x = 1;
    }
}";

            var test = new CSharpCodeFixTest<GM0109Analyzer, GM0109CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeAssign_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x {|GM0109:=|}1;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x=1;
    }
}";

            var test = new CSharpCodeFixTest<GM0109Analyzer, GM0109CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0109.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0109.enabled = false"));

            await test.RunAsync();
        }
    }
}
