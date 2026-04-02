namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0029Analyzer>;

    public class GM0029AnalyzerTests
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
        public async Task MultiLineArgumentList_OpenParenOnDeclarationLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo{|GM0029:(|}
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
        public async Task MultiLineArgumentList_OpenParenOnSeparateLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
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
        public async Task MultiLineParameterList_OpenParenOnDeclarationLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M{|GM0029:(|}
        int a,
        int b,
        int c
    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_OpenParenOnSeparateLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M
    (
        int a,
        int b,
        int c
    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
