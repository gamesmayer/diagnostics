namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0063Analyzer>;

    public class GM0063AnalyzerTests
    {
        [Fact]
        public async Task Method_MultiLineBody_NoDiagnostic()
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
        public async Task Method_SingleLineBody_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {|GM0063:{ System.Console.WriteLine(); }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Method_EmptySingleLineBody_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void M() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task AutoProperty_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public bool IsRevealed { get; protected set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Constructor_SingleLineBody_Diagnostic()
        {
            var testCode = @"class Foo
{
    int _x;

    public Foo()
    {|GM0063:{ _x = 1; }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Constructor_MultiLineBody_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int _x;

    public Foo()
    {
        _x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LocalFunction_SingleLineBody_Diagnostic()
        {
            var testCode = @"class Foo
{
    void M()
    {
        Local();

        void Local() {|GM0063:{ System.Console.WriteLine(); }|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLambda_SingleLineBlock_Diagnostic()
        {
            var testCode = @"using System;

class Foo
{
    void M()
    {
        Action a = () => {|GM0063:{ System.Console.WriteLine(); }|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task SimpleLambda_SingleLineBlock_Diagnostic()
        {
            var testCode = @"using System;

class Foo
{
    void M()
    {
        Action<int> a = x => {|GM0063:{ System.Console.WriteLine(x); }|};
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ParenthesizedLambda_ExpressionBody_NoDiagnostic()
        {
            var testCode = @"using System;

class Foo
{
    void M()
    {
        Func<int> f = () => 42;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
