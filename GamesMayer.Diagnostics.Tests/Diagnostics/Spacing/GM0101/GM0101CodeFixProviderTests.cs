namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0101CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBetweenEmptyBrackets_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    int{|GM0101:[|} ] arr = new int[3];
}";
            var fixedCode = @"class Foo
{
    int[] arr = new int[3];
}";

            var test = new CSharpCodeFixTest<GM0101Analyzer, GM0101CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBetweenEmptyBrackets_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    int{|GM0101:[|}] arr = new int[3];
}";
            var fixedCode = @"class Foo
{
    int[ ] arr = new int[3];
}";

            var test = new CSharpCodeFixTest<GM0101Analyzer, GM0101CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0101.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0101.enabled = true"));

            await test.RunAsync();
        }
    }
}
