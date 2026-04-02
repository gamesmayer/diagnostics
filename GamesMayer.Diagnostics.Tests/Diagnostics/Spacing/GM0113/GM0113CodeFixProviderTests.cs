namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0113CodeFixProviderTests
    {
        [Fact]
        public async Task NoSpaceBetweenSingleLineAccessorBraces_DefaultSetting_AddsSpaces()
        {
            var testCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized {|GM0113:{|}get;}
}";
            var fixedCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized { get; }
}";

            var test = new CSharpCodeFixTest<GM0113Analyzer, GM0113CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task NoSpaceBetweenSingleLineBlockBraces_DefaultSetting_AddsSpaces()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        if (value) {|GM0113:{|}return;}
    }
}";
            var fixedCode = @"class Foo
{
    void M(bool value)
    {
        if (value) { return; }
    }
}";

            var test = new CSharpCodeFixTest<GM0113Analyzer, GM0113CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBetweenSingleLineAccessorBraces_EnabledFalse_RemovesSpaces()
        {
            var testCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized {|GM0113:{|} get; }
}";
            var fixedCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized {get;}
}";

            var test = new CSharpCodeFixTest<GM0113Analyzer, GM0113CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0113.enabled = false"));

            test.FixedState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0113.enabled = false"));

            await test.RunAsync();
        }
    }
}