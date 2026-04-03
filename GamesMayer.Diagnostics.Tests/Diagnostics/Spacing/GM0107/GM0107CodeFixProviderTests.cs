namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0107CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceAfterArrow_DefaultSetting_AddsSpace()
        {
            var testCode = @"class Foo
{
    int M() {|GM0107:=>|}1;
}";
            var fixedCode = @"class Foo
{
    int M() => 1;
}";

            var test = new CSharpCodeFixTest<GM0107Analyzer, GM0107CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterArrow_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    int M() {|GM0107:=>|}1;
}";
            var fixedCode = @"class Foo
{
    int M() => 1;
}";

            var test = new CSharpCodeFixTest<GM0107Analyzer, GM0107CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0107.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0107.enabled = true"));

            await test.RunAsync();
        }
    }
}
