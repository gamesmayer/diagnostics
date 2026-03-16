namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0030Analyzer>;

    public class GM0030AnalyzerTests
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
        public async Task MultiLineArgumentList_NoBlankLineBetweenDeclarationAndOpenParen_NoDiagnostic()
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
        public async Task MultiLineArgumentList_BlankLineBetweenDeclarationAndOpenParen_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
{|GM0030:|}
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
        public async Task MultiLineParameterList_BlankLineBetweenDeclarationAndOpenParen_Diagnostic()
        {
            var testCode = @"class C
{
    void M
{|GM0030:|}
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

        [Fact]
        public async Task MultiLineArgumentList_CommentBetweenDeclarationAndOpenParen_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        // comment
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
        public async Task EmptyArgumentList_BlankLineBetweenDeclarationAndOpenParen_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Unregister
{|GM0030:|}
        ();
    }

    void Unregister() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyParameterList_BlankLineBetweenDeclarationAndOpenParen_Diagnostic()
        {
            var testCode = @"class C
{
    void M
{|GM0030:|}
    ()
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
