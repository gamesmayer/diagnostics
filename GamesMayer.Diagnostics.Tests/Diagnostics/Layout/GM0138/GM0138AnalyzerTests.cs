namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0138Analyzer>;

    public class GM0138AnalyzerTests
    {
        [Fact]
        public async Task ParameterListAndArrowOnSameLine_NoDiagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = (a, b) => a + b;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyParameterListAndArrowOnSameLine_NoDiagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Action f = () => { };
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SimpleLambdaOnSameLine_NoDiagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int> f = x => x + 1;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SimpleLambdaArrowOnNextLine_Diagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int> f = {|GM0138:x
            => x + 1|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrowOnNextLine_Diagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = {|GM0138:(a, b)
            => a + b|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ArrowSeparatedByBlankLine_Diagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Func<int, int, int> f = {|GM0138:(a, b)

            => a + b|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyParamsArrowOnNextLine_Diagnostic()
        {
            var testCode = @"
using System;

class Foo
{
    void Test()
    {
        Action f = {|GM0138:()
            => { }|};
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
