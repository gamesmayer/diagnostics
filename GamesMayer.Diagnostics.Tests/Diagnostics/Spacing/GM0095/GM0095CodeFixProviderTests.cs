namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0095CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBeforeComma_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1 {|GM0095:,|} 2); }
}";
            var fixedCode = @"class Foo
{
    void M(int a, int b) { }
    void N() { M(1, 2); }
}";

            var test = new CSharpCodeFixTest<GM0095Analyzer, GM0095CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeComma_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    void M(int a , int b) { }
    void N() { M(1{|GM0095:,|} 2); }
}";
            var fixedCode = @"class Foo
{
    void M(int a , int b) { }
    void N() { M(1 , 2); }
}";

            var test = new CSharpCodeFixTest<GM0095Analyzer, GM0095CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0095.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0095.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeComma_ParameterList_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    void M(int a {|GM0095:,|} int b) { }
}";
            var fixedCode = @"class Foo
{
    void M(int a, int b) { }
}";

            var test = new CSharpCodeFixTest<GM0095Analyzer, GM0095CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }
    }
}
