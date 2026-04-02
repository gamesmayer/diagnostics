namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0028Analyzer>;

    public class GM0028AnalyzerTests
    {
        [Fact]
        public async Task SingleLineArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(1, 2, 3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_CloseParenAlignedWithOpenLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_CloseParenOverIndented_DiagnosticOnCloseParenOnly()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3
            {|GM0028:)|};
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_CloseParenUnderIndented_DiagnosticOnCloseParenOnly()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b,
        int c
{|GM0028:)|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineList_CloseParenOnLastItemLine_NoDiagnosticFromThisRule()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
