namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0106CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceBeforeArrow_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    int M(){|GM0106:=>|} 1;
}";
            var fixedCode = @"class Foo
{
    int M() => 1;
}";

            var test = new CSharpCodeFixTest<GM0106Analyzer, GM0106CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeArrow_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    int M() {|GM0106:=>|} 1;
}";
            var fixedCode = @"class Foo
{
    int M()=> 1;
}";

            var test = new CSharpCodeFixTest<GM0106Analyzer, GM0106CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0106.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0106.enabled = false"));

            await test.RunAsync();
        }
    }
}
