namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0051Analyzer>;

    public class GM0051AnalyzerTests
    {
        [Fact]
        public async Task Return_SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    int M()
    {
        return 42;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_WithoutExpression_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(bool flag)
    {
        if (flag)
        {
            return;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_MultiLineExpression_StartsOnSameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    int M(int a, int b)
    {
        return a +
            b;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_ExpressionOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    int M()
    {
        return
            {|GM0051:42|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Return_ComplexExpressionOnNextLine_Diagnostic()
        {
            var testCode = @"class C
{
    int[] items = new int[0];

    int[] M()
    {
        return
            {|GM0051:items|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
