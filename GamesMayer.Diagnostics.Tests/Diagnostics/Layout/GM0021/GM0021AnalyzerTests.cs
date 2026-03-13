namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0021Analyzer>;

    public class GM0021AnalyzerTests
    {
        [Fact]
        public async Task EmptyMethodBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyMethodBraceOnNewLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {|GM0021:{|} }
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
        public async Task EmptyTypeBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyTypeBraceOnNewLine_Diagnostic()
        {
            var testCode = @"class Foo
{|GM0021:{|} }
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyControlBlockBraceOnNewLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {|GM0021:{|} }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CategoriesConfiguration_MethodsSkipped_TypesAndControlBlocksAnalyzed()
        {
            var testCode = @"class Foo
{|GM0021:{|} }

class Bar
{
    void M()
    { }

    void N()
    {
        if (true)
        {|GM0021:{|} }
    }
}";

            var test = new CSharpAnalyzerTest<GM0021Analyzer, XUnitVerifier>
            {
                TestCode = testCode,
            };

            test.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", @"root = true

[*.cs]
dotnet_diagnostic.GM0021 = control_blocks, types"));

            await test.RunAsync();
        }
    }
}
