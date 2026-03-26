namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0044Analyzer>;

    public class GM0044AnalyzerTests
    {
        [Fact]
        public async Task ExpressionBody_OnNextLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
            x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockBody_OnNextLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
        {
            return x + 1;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ExpressionBody_WithBlankLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
{|GM0044:|}
            x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockBody_WithBlankLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
{|GM0044:|}
        {
            return x + 1;
        };
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CommentBetweenArrowAndBody_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
        // explain expression
            x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLines_ReportOnFirstBlankLine()
        {
            var testCode = @"class C
{
    void M()
    {
        System.Func<int, int> f = x =>
{|GM0044:|}

            x + 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
