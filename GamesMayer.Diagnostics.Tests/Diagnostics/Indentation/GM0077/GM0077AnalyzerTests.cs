namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0077Analyzer>;

    public class GM0077AnalyzerTests
    {
        [Fact]
        public async Task CorrectlyIndentedMethod_NoDiagnostic()
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
        public async Task EmptyBlock_NoDiagnostic()
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
        public async Task UnindentedStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
{|GM0077:int|} x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task OverIndentedStatement_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
            {|GM0077:int|} x = 1;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MultipleStatements_OneWrong_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        int x = 1;
{|GM0077:int|} y = 2;
        int z = 3;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_CorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            int x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NestedBlock_WrongIndent_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
{|GM0077:int|} x = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task BlockWithDirectives_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
#if SOME_DEFINE
        int x = 1;
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMemberCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    private int _value;

    void Method() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMemberNotIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
{|GM0077:private|} int _value;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task ClassMemberOverIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
        {|GM0077:private|} int _value;
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAccessorCorrectlyIndented_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    int Value
    {
        get;
        set;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task PropertyAccessorNotIndented_Diagnostic()
        {
            var testCode = @"class Foo
{
    int Value
    {
{|GM0077:get|};
        set;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
