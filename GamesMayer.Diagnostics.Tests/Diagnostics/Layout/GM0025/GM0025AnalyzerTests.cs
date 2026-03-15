namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0025Analyzer>;

    public class GM0025AnalyzerTests
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
        public async Task MultiLineArgumentList_EachOnOwnLine_NoDiagnostic()
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

        [Fact]
        public async Task FirstArgOnParenLine_MultiLine_EachOnOwnLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleArgList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1);
    }

    void Foo(int a) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MiddleArgNotOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1, {|GM0025:2,|}
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LastArgNotOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2, {|GM0025:3|});
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleArgsOnSameLine_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1, {|GM0025:2,|} {|GM0025:3|});
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AllArgsOnOneLineAfterParen_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1, {|GM0025:2,|} {|GM0025:3|});
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_EachOnOwnLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b,
        int c)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_ParameterNotOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M(
        int a, {|GM0025:int b,|}
        int c)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultiLineParameterList_LastParameterNotOnOwnLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b, {|GM0025:int c|})
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
