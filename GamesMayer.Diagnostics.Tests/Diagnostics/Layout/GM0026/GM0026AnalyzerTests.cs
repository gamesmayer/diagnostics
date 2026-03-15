namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0026Analyzer>;

    public class GM0026AnalyzerTests
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
        public async Task MultiLineArgumentList_CloseParenOnOwnLine_NoDiagnostic()
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
        public async Task MultiLineArgumentList_CloseParenOnLastItemLine_DiagnosticOnCloseParenOnly()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3{|GM0026:)|};
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_BlankLineBeforeCloseParen_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3

        {|GM0026:)|};
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_CloseParenOnLastItemLine_DiagnosticOnCloseParenOnly()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b,
        int c{|GM0026:)|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyMultiLineList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
        );
    }

    void Foo() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}