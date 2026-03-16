namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0034Analyzer>;

    public class GM0034AnalyzerTests
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
        public async Task MultiLineArgumentList_FirstItemOnNextLine_NoDiagnostic()
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
        public async Task MultiLineArgumentList_FirstItemOnSameLine_DiagnosticOnFirstItemAndComma()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo({|GM0034:1,|}
            2,
            3
        );
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_FirstItemOnSameLine_DiagnosticOnIdentifierAndComma()
        {
            var testCode = @"class C
{
    void M({|GM0034:int a,|}
        int b,
        int c
    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineList_FirstItemOnSameLineAndLastItem_NoCommaDiagnosticStillRaised()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo({|GM0034:1|}
        );
    }

    void Foo(int a)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
