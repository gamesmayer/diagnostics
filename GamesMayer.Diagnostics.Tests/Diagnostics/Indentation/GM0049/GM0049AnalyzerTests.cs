namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0049Analyzer>;

    public class GM0049AnalyzerTests
    {
        [Fact]
        public async Task SingleLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null && a.Length > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WrappedOneStepIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(string a, string b)
    {
        bool ok = a != null &&
            a.Length > 0 &&
            b != null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WrappedNotIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
        {|GM0049:a.Length > 0;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WrappedOverIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null &&
                {|GM0049:a.Length > 0;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ReturnExpression_WrappedNotIndented_Diagnostic()
        {
            var testCode = @"class C
{
    bool M(string a)
    {
        return a != null &&
        {|GM0049:a.Length > 0;|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLogicalExpression_WrappedItem_Diagnostic()
        {
            var testCode = @"class C
{
    bool M(string a, string b)
    {
        return (a != null &&
        {|GM0049:a.Length > 0) |||}
            b != null;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OperatorAtLineStart_OnlyGM0046_NoGM0049()
        {
            var testCode = @"class C
{
    void M(string a)
    {
        bool b = a != null
            && a.Length > 0;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
