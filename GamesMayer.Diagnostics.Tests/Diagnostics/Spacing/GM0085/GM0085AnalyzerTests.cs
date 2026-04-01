namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0085Analyzer>;

    public class GM0085AnalyzerTests
    {
        [Fact]
        public async Task WithoutSpaces_DefaultSetting_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M(value);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithSpaces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0085:(|} value );
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MixedSpaces_DefaultSetting_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0085:(|} value);
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WithoutSpaces_EnabledTrue_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M{|GM0085:(|}value);
    }
}";

            var test = new CSharpAnalyzerTest<GM0085Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0085.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task WithSpaces_EnabledTrue_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M(int value) => value;

    int N(int value)
    {
        return M( value );
    }
}";

            var test = new CSharpAnalyzerTest<GM0085Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0085.enabled = true"));

            await test.RunAsync();
        }

        [Fact]
        public async Task EmptyArgumentList_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M() => 1;

    int N()
    {
        return M();
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
