namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0031CodeFixProviderTests
    {
        [Fact]
        public async Task MethodDeclaration_EmptyParameterList_OnDifferentLine_Fix()
        {
            var testCode = @"class C
{
    void {|GM0031:M
    ()|}
    {
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0031Analyzer, GM0031CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Invocation_EmptyArgumentList_OnDifferentLine_Fix()
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
            var fixedCode = @"class C
{
    void M()
    {
        Foo();
    }

    void Foo() { }
}";
            var test = new CSharpCodeFixTest<GM0031Analyzer, GM0031CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Constructor_EmptyParameterList_OnDifferentLine_Fix()
        {
            var testCode = @"class C
{
    {|GM0031:C
    ()|}
    {
    }
}";
            var fixedCode = @"class C
{
    C()
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0031Analyzer, GM0031CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task MethodDeclaration_EmptyParameterList_SpreadAcrossLines_Fix()
        {
            var testCode = @"class C
{
    void {|GM0031:M(
    )|}
    {
    }
}";
            var fixedCode = @"class C
{
    void M()
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0031Analyzer, GM0031CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}
