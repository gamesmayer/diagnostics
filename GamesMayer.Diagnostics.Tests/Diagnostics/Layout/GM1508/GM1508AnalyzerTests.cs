namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM1508Analyzer>;

    public class GM1508AnalyzerTests
    {
        [Fact]
        public async Task NoBlankLineBeforeCloseBrace_NoDiagnostic()
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
        public async Task ClassDeclaration_BlankLineBeforeCloseBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method() { }
{|GM1508:|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBody_BlankLineBeforeCloseBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM1508:|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceDeclaration_BlankLineBeforeCloseBrace_Diagnostic()
        {
            var testCode = @"namespace MyApp
{
    class Foo { }
{|GM1508:|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_BlankLineBeforeCloseBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
{|GM1508:|}
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForLoop_BlankLineBeforeCloseBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            int x = i;
{|GM1508:|}
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyBraces_NoDiagnostic()
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
        public async Task MultipleBlankLinesBeforeCloseBrace_AllFlagged()
        {
            var testCode = "class Foo\n{\n    void Method() { }\n{|GM1508:|}\n{|GM1508:|}\n}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
