namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM1505Analyzer>;

    public class GM1505AnalyzerTests
    {
        [Fact]
        public async Task NoBlankLineAfterOpenBrace_NoDiagnostic()
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
        public async Task ClassDeclaration_BlankLineAfterOpenBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
{|GM1505:|}
    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBody_BlankLineAfterOpenBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
{|GM1505:|}
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamespaceDeclaration_BlankLineAfterOpenBrace_Diagnostic()
        {
            var testCode = @"namespace MyApp
{
{|GM1505:|}
    class Foo { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfStatement_BlankLineAfterOpenBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
{|GM1505:|}
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseBlock_BlankLineAfterOpenBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
        }
        else
        {
{|GM1505:|}
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ForLoop_BlankLineAfterOpenBrace_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
        {
{|GM1505:|}
            int x = i;
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
        public async Task MultipleBlankLinesAfterOpenBrace_AllFlagged()
        {
            var testCode = "class Foo\n{\n{|GM1505:|}\n{|GM1505:|}\n    void Method() { }\n}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
