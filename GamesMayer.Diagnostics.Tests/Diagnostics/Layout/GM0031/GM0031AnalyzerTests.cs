namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Xunit;
    using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
        GamesMayer.Diagnostics.GM0031Analyzer>;

    public class GM0031AnalyzerTests
    {
        [Fact]
        public async Task MethodDeclaration_EmptyParameterList_SameLine_NoDiagnostic()
        {
            var testCode = @"class C
{
    void M() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Invocation_EmptyArgumentList_SameLine_NoDiagnostic()
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

        [Fact]
        public async Task MethodDeclaration_NonEmptyParameterList_MultiLine_NoDiagnostic()
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
        public async Task Invocation_NonEmptyArgumentList_MultiLine_NoDiagnostic()
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
        public async Task MethodDeclaration_EmptyParameterList_OnDifferentLine_Diagnostic()
        {
            var testCode = @"class C
{
    void {|GM0031:M
    ()|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Invocation_EmptyArgumentList_OnDifferentLine_Diagnostic()
        {
            var testCode = @"class C
{
    void M()
    {
        {|GM0031:Foo
        ()|};
    }

    void Foo() { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task MethodDeclaration_EmptyParameterList_SpreadAcrossLines_Diagnostic()
        {
            var testCode = @"class C
{
    void {|GM0031:M(
    )|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Constructor_EmptyParameterList_OnDifferentLine_Diagnostic()
        {
            var testCode = @"class C
{
    {|GM0031:C
    ()|}
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LambdaWithEmptyParameterList_SameLine_NoDiagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Foo
        (
            0.1f,
            () =>
            {
            }
        );
    }

    void Foo(float f, Action a) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task LambdaWithEmptyParameterList_SpreadAcrossLines_Diagnostic()
        {
            var testCode = @"using System;
class C
{
    void M()
    {
        Foo
        (
            0.1f,
            {|GM0031:(
            )|} =>
            {
            }
        );
    }

    void Foo(float f, Action a) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}
