namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0096CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceAfterDot_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello""{|GM0096:.|} Length;
}";
            var fixedCode = @"class Foo
{
    int Bar => ""hello"".Length;
}";

            var test = new CSharpCodeFixTest<GM0096Analyzer, GM0096CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceAfterDot_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    int Bar => ""hello""{|GM0096:.|}Length;
}";
            var fixedCode = @"class Foo
{
    int Bar => ""hello"". Length;
}";

            var test = new CSharpCodeFixTest<GM0096Analyzer, GM0096CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0096.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0096.enabled = true"));

            await test.RunAsync();
        }
    }
}
