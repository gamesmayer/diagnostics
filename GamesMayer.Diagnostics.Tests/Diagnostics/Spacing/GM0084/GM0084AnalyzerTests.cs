namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0084Analyzer>;

    public class GM0084AnalyzerTests
    {
        [Fact]
        public async Task WithoutSpaces_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(int value)
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithSpaces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|} int value )
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MixedSpaces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|} int value)
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithoutSpaces_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|}int value)
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0084Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0084.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpaces_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M( int value )
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0084Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0084.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task MixedSpaces_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M{|GM0084:(|} int value)
    {
    }
}";

            var test = new CSharpAnalyzerTest<GM0084Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0084.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyParameterList_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
