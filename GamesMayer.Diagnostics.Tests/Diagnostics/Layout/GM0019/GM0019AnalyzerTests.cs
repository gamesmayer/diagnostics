namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0019Analyzer>;

    public class GM0019AnalyzerTests
    {
        [Fact]
        public async Task EmptyMethodBody_BracesOnOneLineWithSingleSpace_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    protected void Method(
        int a,
        int b,
        int c
    )
    { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyClassBody_BracesOnOneLineWithSingleSpace_NoDiagnostic()
        {
            var testCode = @"class Foo<
    T>
    where T : class
{ }
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonEmptyMethodBody_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method(
        int a,
        int b)
    {
        _ = a + b;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyMethodBody_BracesOnDifferentLines_Diagnostic()
        {
            var testCode = @"class Foo
{
    protected void Method(
        int a,
        int b,
        int c
    )
    {|GM0019:{
    }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyClassBody_BracesOnDifferentLines_Diagnostic()
        {
            var testCode = @"class Foo<
    T>
    where T : class
{|GM0019:{
}|}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyMethodBody_BracesWithoutSpace_Diagnostic()
        {
            var testCode = @"class Foo
{
    protected void Method(
        int a,
        int b,
        int c
    )
    {|GM0019:{}|}
}
";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyConstructorBody_BracesOnDifferentLines_Diagnostic()
        {
            var testCode = @"class Foo
{
    public Foo()
    {|GM0019:{
    }|}
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyConstructorBody_BracesOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public Foo() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyIfBlock_BracesOnDifferentLines_Diagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {|GM0019:{
        }|}
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyIfBlock_BracesOnSameLine_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true) { }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyStructBody_BracesOnDifferentLines_Diagnostic()
        {
            var testCode = @"struct Foo
{|GM0019:{
}|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task EmptyInterfaceBody_BracesOnDifferentLines_Diagnostic()
        {
            var testCode = @"interface IFoo
{|GM0019:{
}|}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task NonEmptyIfBlock_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    void Method()
    {
        if (true)
        {
            _ = 1;
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBodyWithPreprocessorDirective_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method()
    {
#if SOME_SYMBOL
        _ = 1;
#endif
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodBodyWithComment_NoDiagnostic()
        {
            var testCode = @"class Foo
{
    public void Method()
    {
        // TODO: implement
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
