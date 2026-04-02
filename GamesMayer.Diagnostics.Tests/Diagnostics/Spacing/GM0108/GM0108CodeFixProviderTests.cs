namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0108CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceAfterType_DefaultSetting_AddsSpace()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<(string key, object value)>{|GM0108:parameters|} = new();
}";
            var fixedCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<(string key, object value)> parameters = new();
}";

            var test = new CSharpCodeFixTest<GM0108Analyzer, GM0108CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceAfterType_EnabledFalse_RemovesSpace()
        {
            var testCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<int> {|GM0108:value|} = new();
}";
            var fixedCode = @"using System.Collections.Generic;
class Foo
{
    private readonly List<int>value = new();
}";

            var test = new CSharpCodeFixTest<GM0108Analyzer, GM0108CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0108.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0108.enabled = false"));

            await test.RunAsync();
        }
    }
}
