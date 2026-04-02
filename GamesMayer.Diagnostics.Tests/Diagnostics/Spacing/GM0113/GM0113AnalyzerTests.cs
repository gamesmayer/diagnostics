namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0113Analyzer>;

    public class GM0113AnalyzerTests
    {
        [Fact]
        public async Task SpaceBetweenSingleLineAccessorBraces_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized { get; }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenSingleLineAccessorBraces_DefaultSetting_Diagnostic()
        {
            var testCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized {|GM0113:{|}get;}
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenSingleLineBlockBraces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        if (value) {|GM0113:{|}return;}
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenSingleLineAccessorBraces_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized {get;}
}";

            var test = new CSharpAnalyzerTest<GM0113Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0113.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBetweenSingleLineAccessorBraces_EnabledFalse_Diagnostic()
        {
            var testCode = @"interface IListenOnly<T> { }
class AdsInitializedEvent { }
class Foo
{
    public IListenOnly<AdsInitializedEvent> AdsInitialized {|GM0113:{|} get; }
}";

            var test = new CSharpAnalyzerTest<GM0113Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0113.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MultilineBlock_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        if (value)
        {
            return;
        }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptySingleLineBlock_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool value)
    {
        if (value) { }
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}