namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0073Analyzer>;

    public class GM0073AnalyzerTests
    {
        [Fact]
        public async Task EndIfDirective_LastInBlock_NoDiagnostic()
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
        public async Task EndIfDirective_FollowedByBlankLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
#endif

        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EndIfDirective_NotFollowedByBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0073:#endif|}
        int x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EndIfDirective_LastInFile_NoDiagnostic()
        {
            var testCode = @"#if true
using System;
#endif";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EndIfDirective_LastStatementInClass_NoDiagnostic()
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
        public async Task EndIfDirective_FollowedByAnotherIfDirective_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0073:#endif|}
#if false
        System.Console.WriteLine();
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EndIfDirective_InsideIfBlock_LastStatement_NoDiagnostic()
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
        public async Task EndIfDirective_InsideIfBlock_NotLast_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
#if true
            System.Console.WriteLine();
{|GM0073:#endif|}
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleEndIfDirectives_BothMissingBlankLine_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
#if true
        System.Console.WriteLine();
{|GM0073:#endif|}
        int x = 1;

#if false
        System.Console.WriteLine();
{|GM0073:#endif|}
        int y = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
