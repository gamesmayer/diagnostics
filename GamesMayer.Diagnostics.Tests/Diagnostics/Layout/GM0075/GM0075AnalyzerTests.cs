namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0075Analyzer>;

    public class GM0075AnalyzerTests
    {
        [Fact]
        public async Task EndIfDirective_NoBlankLineBefore_NoDiagnostic()
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
        public async Task EndIfDirective_BlankLineBefore_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElseDirective_NoBlankLineBefore_NoDiagnostic()
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
        public async Task ElseDirective_BlankLineBefore_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
#else
        System.Console.Write();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ElifDirective_NoBlankLineBefore_NoDiagnostic()
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
        public async Task ElifDirective_BlankLineBefore_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
#elif false
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
        public async Task MultipleBlankLinesBeforeEndIf_AllFlagged()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0075:|}
{|GM0075:|}
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
