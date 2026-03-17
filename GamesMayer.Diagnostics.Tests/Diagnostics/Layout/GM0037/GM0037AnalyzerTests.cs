namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0037Analyzer>;

    public class GM0037AnalyzerTests
    {
        [Fact]
        public async Task SingleLineParameterHeader_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(int value)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParameterHeaderWithMultilineDefaultValue_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(int value =
        1 +
        2)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TypeAndIdentifierOnDifferentLines_Diagnostic()
        {
            var testCode = @"class C
{
    void M({|GM0037:int
        value|})
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IdentifierAndEqualsOnDifferentLines_Diagnostic()
        {
            var testCode = @"class C
{
    void M({|GM0037:int value
        = 1|})
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ModifierAndTypeOnDifferentLines_Diagnostic()
        {
            var testCode = @"class C
{
    void M({|GM0037:in
        int value|})
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
