namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0022Analyzer>;

    public class GM0022AnalyzerTests
    {
        [Fact]
        public async Task EmptyMethodBraceOnNewLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyMethodBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        void Local() {|GM0022:{|} }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonEmptyMethodBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method() {
        int value = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyTypeBraceOnNewLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{ }
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyTypeBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo {|GM0022:{|} }
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnNewLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnSameLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true) {|GM0022:{|} }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBodyWithIfDirective_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
#if SOME_DEFINE
        int value = 1;
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CategoriesConfiguration_MethodsSkipped_TypesAndControlBlocksAnalyzed()
        {
            var testCode = @"class Foo {|GM0022:{|} }

class Bar
{
    void M() { }

    void N()
    {
        if (true) {|GM0022:{|} }
    }
}";

            var test = new CSharpAnalyzerTest<GM0022Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0022 = control_blocks, types"));

            await test.RunAsync();
        }
    }
}
