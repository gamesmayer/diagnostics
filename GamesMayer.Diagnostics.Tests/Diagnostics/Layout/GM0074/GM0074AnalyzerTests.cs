namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0074Analyzer>;

    public class GM0074AnalyzerTests
    {
        [Fact]
        public async Task IfDirective_NoBlankLineAfter_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_BlankLineAfter_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
{|GM0074:|}
        System.Console.WriteLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseDirective_NoBlankLineAfter_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#else
        System.Console.Write();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseDirective_BlankLineAfter_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#else
{|GM0074:|}
        System.Console.Write();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElifDirective_NoBlankLineAfter_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#elif false
        System.Console.Write();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElifDirective_BlankLineAfter_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#elif false
{|GM0074:|}
        System.Console.Write();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FullConditionalStructure_AllClean_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#elif false
        System.Console.Write();
#else
        System.Console.ReadLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleBlankLinesAfterIfDirective_AllFlagged()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
{|GM0074:|}
{|GM0074:|}
        System.Console.WriteLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
