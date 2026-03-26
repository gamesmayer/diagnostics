namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0052Analyzer>;

    public class GM0052AnalyzerTests
    {
        [Fact]
        public async Task ReturnExpressionOnNextLineWithoutBlankLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    int M(int a, int b)
    {
        return
            a + b;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReturnWithoutExpression_NoDiagnostic()
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
        public async Task BlankLineBetweenReturnAndExpression_Diagnostic()
        {
            var testCode = @"class C
{
    int M()
    {
        return
{|GM0052:
|}            42;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesBetweenReturnAndExpression_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    int[] M(int[] items)
    {
        return
{|GM0052:
|}{|GM0052:
|}            items;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CommentBetweenReturnAndExpression_NoDiagnostic()
        {
            var testCode = @"class C
{
    int M()
    {
        return
            // explanation
            42;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
