namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0012Analyzer>;

    public class GM0012AnalyzerTests
    {
        [Fact]
        public async Task NoBlankLineBeforeBrace_NoDiagnostic()
        {
            var testCode = @"class Foo
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassDeclaration_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{|GM0012:|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceDeclaration_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"namespace MyApp
{|GM0012:|}
{
    class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBody_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
{|GM0012:|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
{|GM0012:|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseBlock_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
        else
{|GM0012:|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForLoop_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
{|GM0012:|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WhileLoop_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        while (true)
{|GM0012:|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForeachLoop_BlankLineBeforeBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method(int[] items)
    {
        foreach (var item in items)
{|GM0012:|}
        {
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesBeforeBrace_OnlyLastFlagged()
        {
            var testCode = @"class Foo

{|GM0012:|}
{
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
