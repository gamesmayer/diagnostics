namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0032Analyzer>;

    public class GM0032AnalyzerTests
    {
        [Fact]
        public async Task MethodDeclaration_SingleLineParameterList_SameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M(int a, int b)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Invocation_SingleLineArgumentList_SameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo(1, 2);
    }

    void Foo(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclaration_MultiLineParameterList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M
    (
        int a,
        int b
    )
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Invocation_MultiLineArgumentList_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        Foo
        (
            1,
            2
        );
    }

    void Foo(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclaration_SingleLineParameterList_OnDifferentLine_Diagnostic()
        {
            var testCode = @"class C
{
    void {|GM0032:M
    (int a, int b)|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Invocation_SingleLineArgumentList_OnDifferentLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        {|GM0032:Foo
        (1, 2)|};
    }

    void Foo(int a, int b) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Constructor_SingleLineParameterList_OnDifferentLine_Diagnostic()
        {
            var testCode = @"class C
{
    {|GM0032:C
    (int a, int b)|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}