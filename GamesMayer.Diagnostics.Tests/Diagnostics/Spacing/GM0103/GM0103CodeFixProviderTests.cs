namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0103CodeFixProviderTests
    {
        [Fact]
        public async Task SpaceBeforeLessThan_DefaultSetting_RemovesSpace()
        {
            var testCode = @"class Foo {|GM0103:<|}T> { }";
            var fixedCode = @"class Foo<T> { }";

            var test = new CSharpCodeFixTest<GM0103Analyzer, GM0103CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBeforeLessThan_EnabledTrue_AddsSpace()
        {
            var testCode = @"class Foo
{
    System.Collections.Generic.List{|GM0103:<|}int> Values;
}";
            var fixedCode = @"class Foo
{
    System.Collections.Generic.List <int> Values;
}";

            var test = new CSharpCodeFixTest<GM0103Analyzer, GM0103CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0103.enabled = true"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0103.enabled = true"));

            await test.RunAsync();
        }
    }
}
