namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0098CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBeforeOpenBracket_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    int Bar(int[] arr) => arr {|GM0098:[|}0];
}";
            var fixedCode = @"class Foo
{
    int Bar(int[] arr) => arr[0];
}";

            var test = new CSharpCodeFixTest<GM0098Analyzer, GM0098CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeOpenBracket_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    char Bar(string s) => s{|GM0098:[|}0];
}";
            var fixedCode = @"class Foo
{
    char Bar(string s) => s [0];
}";

            var test = new CSharpCodeFixTest<GM0098Analyzer, GM0098CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0098.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0098.enabled = true"));

            await test.RunAsync();
        }
    }
}
