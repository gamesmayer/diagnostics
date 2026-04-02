namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0114CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceBetweenAccessors_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    public int Value { get{|GM0114:;|}set; }
}";
            var fixedCode = @"class Foo
{
    public int Value { get; set; }
}";

            var test = new CSharpCodeFixTest<GM0114Analyzer, GM0114CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBetweenAccessors_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    public int Value { get{|GM0114:;|} set; }
}";
            var fixedCode = @"class Foo
{
    public int Value { get;set; }
}";

            var test = new CSharpCodeFixTest<GM0114Analyzer, GM0114CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0114.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0114.enabled = false"));

            await test.RunAsync();
        }
    }
}
