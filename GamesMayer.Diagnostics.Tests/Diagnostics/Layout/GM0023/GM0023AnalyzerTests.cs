namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0023Analyzer>;

    public class GM0023AnalyzerTests
    {
        [Fact]
        public async Task CorrectlyIndentedArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WronglyIndentedArgument_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            1,
        {|GM0023:2|},
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleWronglyIndentedArguments_MultipleDiagnostics()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
        {|GM0023:1|},
        {|GM0023:2|},
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleLineArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(1, 2, 3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FirstArgOnSameLineAsParen_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(1,
            2,
            3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task CorrectlyIndentedParameterList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(
        int a,
        int b,
        int c)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task WronglyIndentedParameter_Diagnostic()
        {
            var testCode = @"class C
{
    void M(
        int a,
    {|GM0023:int b|},
        int c)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamedArgumentsCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            a: 1,
            b: 2,
            c: 3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NamedArgumentWronglyIndented_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(
            a: 1,
        {|GM0023:b: 2|},
            c: 3);
    }

    void Foo(int a, int b, int c) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo();
    }

    void Foo() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
