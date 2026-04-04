namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0125CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpace_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    public bool IsBusy{|GM0125:{|}get; set; }
}";
            var fixedCode = @"class Foo
{
    public bool IsBusy {get; set; }
}";
            var test = new CSharpCodeFixTest<GM0125Analyzer, GM0125CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpace_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    public void M(){|GM0125:{|} }
}";
            var fixedCode = @"class Foo
{
    public void M() { }
}";
            var test = new CSharpCodeFixTest<GM0125Analyzer, GM0125CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpace_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    public bool IsBusy {|GM0125:{|} get; set; }
}";
            var fixedCode = @"class Foo
{
    public bool IsBusy{ get; set; }
}";
            var test = new CSharpCodeFixTest<GM0125Analyzer, GM0125CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0125.enabled = false"));

            await test.RunAsync();
        }
    }
}
