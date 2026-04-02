namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0114Analyzer>;

    public class GM0114AnalyzerTests
    {
        [Fact]
        public async Task SpaceBetweenAccessors_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get; set; }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenAccessors_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get{|GM0114:;|}set; }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NoSpaceBetweenAccessors_EnabledFalse_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get;set; }
}";

            var test = new CSharpAnalyzerTest<GM0114Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0114.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task SpaceBetweenAccessors_EnabledFalse_Diagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get{|GM0114:;|} set; }
}";

            var test = new CSharpAnalyzerTest<GM0114Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0114.enabled = false"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MultilineAccessorList_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int _value;
    public int Value
    {
        get => _value;
        set => _value = value;
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleAccessor_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public int Value { get; }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
