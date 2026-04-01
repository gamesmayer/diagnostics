namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0087CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceAfterColon_DefaultSetting_AddsSpace()
        {
            var testCode = @"interface I
{
}

class C {|GM0087::|}I
{
}";
            var fixedCode = @"interface I
{
}

class C : I
{
}";

            var test = new CSharpCodeFixTest<GM0087Analyzer, GM0087CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceAfterColon_EnabledFalse_RemovesSpace()
        {
            var testCode = @"interface I
{
}

class C {|GM0087::|} I
{
}";
            var fixedCode = @"interface I
{
}

class C :I
{
}";

            var test = new CSharpCodeFixTest<GM0087Analyzer, GM0087CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0087.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0087.enabled = false"));

            await test.RunAsync();
        }
    }
}
