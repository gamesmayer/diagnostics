namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0072Analyzer>;

    public class GM0072AnalyzerTests
    {
        [Fact]
        public async Task IfDirective_FirstInBlock_NoDiagnostic()
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
        public async Task IfDirective_PrecededByBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 1;

#if true
        System.Console.WriteLine(x);
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_NotPrecededByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 1;
{|GM0072:#if|} true
        System.Console.WriteLine(x);
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_FirstInFile_NoDiagnostic()
        {
            var testCode = @"#if true
using System;
#endif

class Foo { }";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_FirstStatementAfterOpenBrace_NoDiagnostic()
        {
            var testCode = @"class Foo
{
#if true
    void M() { }
#endif
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_AfterOpenBraceWithBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 1;

#if true
        System.Console.WriteLine(x);
#endif

#if false
        System.Console.WriteLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleIfDirectives_SecondMissingBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        int x = 1;

#if true
        System.Console.WriteLine(x);
#endif
{|GM0072:#if|} false
        System.Console.WriteLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_InsideIfBlock_FirstStatement_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
#if true
            System.Console.WriteLine();
#endif
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task IfDirective_InsideIfBlock_NotFirst_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            int x = 1;
{|GM0072:#if|} true
            System.Console.WriteLine(x);
#endif
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
