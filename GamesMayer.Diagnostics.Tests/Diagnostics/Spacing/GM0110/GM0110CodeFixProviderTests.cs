namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0110CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceAfterAssign_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x {|GM0110:=|}1;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x = 1;
    }
}";

            var test = new CSharpCodeFixTest<GM0110Analyzer, GM0110CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceAfterAssign_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M()
    {
        var x {|GM0110:=|} 1;
    }
}";
            var fixedCode = @"class Foo
{
    void M()
    {
        var x =1;
    }
}";

            var test = new CSharpCodeFixTest<GM0110Analyzer, GM0110CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0110.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0110.enabled = false"));

            await test.RunAsync();
        }
    }
}
