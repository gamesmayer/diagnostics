namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0135Analyzer>;

    public class GM0135AnalyzerTests
    {
        [Fact]
        public async Task NoNamedArguments_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar(int a, int b, int c) { }

    void Test()
    {
        Bar(1, 2, 3);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AllNamedArguments_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar(int a, int b, int c) { }

    void Test()
    {
        Bar(a: 1, b: 2, c: 3);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OneNamedArgument_OthersUnnamed_Diagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar(int a, int b, int c) { }

    void Test()
    {
        Bar({|GM0135:1|}, b: 2, {|GM0135:3|});
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task FirstArgumentNamed_RestUnnamed_Diagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar(int a, int b, int c) { }

    void Test()
    {
        Bar(a: 1, {|GM0135:2|}, {|GM0135:3|});
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ConstructorCall_MixedNamedUnnamed_Diagnostic()
        {
            var testCode = @"
class Foo
{
    public Foo(int x, int y) { }

    static Foo Create()
    {
        return new Foo({|GM0135:1|}, y: 2);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleArgument_Named_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar(int a) { }

    void Test()
    {
        Bar(a: 1);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SingleArgument_Unnamed_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar(int a) { }

    void Test()
    {
        Bar(1);
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyArgumentList_NoDiagnostic()
        {
            var testCode = @"
class Foo
{
    void Bar() { }

    void Test()
    {
        Bar();
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
