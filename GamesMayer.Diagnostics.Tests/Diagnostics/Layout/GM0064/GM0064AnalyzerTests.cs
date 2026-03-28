namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0064Analyzer>;

    public class GM0064AnalyzerTests
    {
        [Fact]
        public async Task Method_SingleReturnAtEnd_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int M()
    {
        int x = 1;
        return x;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_NoReturn_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        System.Console.WriteLine();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_EarlyReturnInIf_Diagnostic()
        {
            var testCode = @"class Foo
{
    int M(bool condition)
    {
        if (condition)
            {|GM0064:return 0;|}
        return 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_MultipleReturns_DiagnosticOnEarlyOnes()
        {
            var testCode = @"class Foo
{
    int M(int x)
    {
        if (x < 0)
            {|GM0064:return -1;|}
        if (x == 0)
            {|GM0064:return 0;|}
        return 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_EarlyReturnInBlock_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M(bool condition)
    {
        if (condition)
        {
            {|GM0064:return;|}
        }
        System.Console.WriteLine();
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_ReturnInsideLambda_NoDiagnostic()
        {
            var testCode = @"using System;

class Foo
{
    void M()
    {
        Func<int> f = () => { return 42; };
        System.Console.WriteLine(f());
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalFunction_EarlyReturn_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        _ = Local(true);

        int Local(bool condition)
        {
            if (condition)
                {|GM0064:return 0;|}
            return 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalFunction_SingleReturnAtEnd_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        _ = Local();

        int Local()
        {
            return 42;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_ReturnInsideAnonymousMethod_NoDiagnostic()
        {
            var testCode = @"using System;

class Foo
{
    void M()
    {
        Func<int> f = delegate { return 42; };
        System.Console.WriteLine(f());
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
