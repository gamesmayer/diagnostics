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
    }
}
