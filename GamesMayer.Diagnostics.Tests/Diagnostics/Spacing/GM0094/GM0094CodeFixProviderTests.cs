namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0094CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceAfterComma_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1{|GM0094:,|}2); }
}";
            var fixedCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1, 2); }
}";

            var test = new CSharpCodeFixTest<GM0094Analyzer, GM0094CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceAfterComma_EnabledFalse_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M(int a,int b) { }
    void N() { M(1{|GM0094:,|} 2); }
}";
            var fixedCode = @"class Foo
{
    void M(int a,int b) { }
    void N() { M(1,2); }
}";

            var test = new CSharpCodeFixTest<GM0094Analyzer, GM0094CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0094.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0094.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterComma_ParameterList_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M(int a{|GM0094:,|}int b) { }
}";
            var fixedCode = @"class Foo
{
    void M(int a, int b) { }
}";

            var test = new CSharpCodeFixTest<GM0094Analyzer, GM0094CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
