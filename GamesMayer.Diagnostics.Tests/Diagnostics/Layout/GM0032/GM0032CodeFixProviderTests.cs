namespace GamesMayer.Diagnostics.Tests
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

    public class GM0032CodeFixProviderTests
    {
        [Fact]
        public async Task MethodDeclaration_SingleLineParameterList_OnDifferentLine_Fix()
        {
            var testCode = @"class C
{
    void {|GM0032:M
    (int a, int b)|}
    {
    }
}";
            var fixedCode = @"class C
{
    void M(int a, int b)
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0032Analyzer, GM0032CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Invocation_SingleLineArgumentList_OnDifferentLine_Fix()
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
            var fixedCode = @"class C
{
    void M()
    {
        Foo(1, 2);
    }

    void Foo(int a, int b) { }
}";
            var test = new CSharpCodeFixTest<GM0032Analyzer, GM0032CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task Constructor_SingleLineParameterList_OnDifferentLine_Fix()
        {
            var testCode = @"class C
{
    {|GM0032:C
    (int a, int b)|}
    {
    }
}";
            var fixedCode = @"class C
{
    C(int a, int b)
    {
    }
}";
            var test = new CSharpCodeFixTest<GM0032Analyzer, GM0032CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
            };
            await test.RunAsync();
        }
    }
}