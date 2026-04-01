namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0078Analyzer>;

    public class GM0078AnalyzerTests
    {
        [Fact]
        public async Task CorrectlyIndentedBraces_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyBlock_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OpenBrace_KAndRStyle_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method() {
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OpenBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
        {|GM0078:{|}
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CloseBrace_OverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
        {|GM0078:}|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CloseBrace_UnderIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0078:}|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BothBraces_WrongIndent_TwoDiagnostics()
        {
            var testCode = @"class Foo
{
    void Method()
        {|GM0078:{|}
        int x = 1;
        {|GM0078:}|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_InnerBraceWrongIndent_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
            {|GM0078:{|}
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockWithDirectives_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
#if SOME_DEFINE
        int x = 1;
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
