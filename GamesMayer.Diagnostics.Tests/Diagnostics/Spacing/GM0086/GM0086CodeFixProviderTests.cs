namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0086CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceBeforeColon_DefaultSetting_AddsSpace()
        {
            var testCode = @"interface I
{
}

class C{|GM0086::|}I
{
}";
            var fixedCode = @"interface I
{
}

class C :I
{
}";

            var test = new CSharpCodeFixTest<GM0086Analyzer, GM0086CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBeforeColon_EnabledFalse_RemovesSpace()
        {
            var testCode = @"interface I
{
}

class C {|GM0086::|} I
{
}";
            var fixedCode = @"interface I
{
}

class C: I
{
}";

            var test = new CSharpCodeFixTest<GM0086Analyzer, GM0086CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0086.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0086.enabled = false"));

            await test.RunAsync();
        }
    }
}
