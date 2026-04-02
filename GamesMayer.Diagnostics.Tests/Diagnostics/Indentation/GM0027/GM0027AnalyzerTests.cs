namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0027Analyzer>;

    public class GM0027AnalyzerTests
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
        public async Task MultiLineArgumentList_OpenParenOnDeclarationLine_FirstItemOnNextLine_NoDiagnostic()
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
        public async Task MultiLineArgumentList_FirstItemOnSameLineAsOpenParen_DiagnosticOnOpenParenOnly()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo{|GM0027:(|}1,
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineArgumentList_OpenParenOnSeparateLine_DiagnosticOnOpenParenOnly()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        {|GM0027:(|}
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
        public async Task MultiLineArgumentList_BlankLineAfterOpenParen_NoDiagnostic()
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
        public async Task MultiLineParameterList_FirstParameterOnSameLineAsOpenParen_Diagnostic()
        {
            var testCode = @"class C
{
    void M{|GM0027:(|}int a,
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